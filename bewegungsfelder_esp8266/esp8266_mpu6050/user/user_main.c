/*
 Send mpu motion data a server via wifi
 Copyright (C) 2016  Ivo Herzig

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#include <ets_sys.h>
#include <osapi.h>
#include <mem.h>
#include <os_type.h>
#include <gpio.h>
#include <user_interface.h>
#include <espconn.h>
#include <uart.h>

#include <esp_mpu.h>
#include <inv_mpu.h>
#include <inv_mpu_dmp_motion_driver.h>

// wifi settings
#define SSID "Bewegungsfelder"
#define PASSWORD "bewegungsfelder"
#define LOCAL_IP "10.0.0.8"
#define SUBNET "255.255.255.0"

// server settings
#define SERVER "10.0.0.254"
#define SERVER_PORT 5555
#define LOCAL_PORT 1025

// sensor settings
#define SENSOR_ID 8
#define SAMPLE_RATE 25

#define HEARTBEAT_INTERVAL 2500

// MPU interrupt pins
#define SENSOR_INT_MUX PERIPHS_IO_MUX_MTMS_U
#define SENSOR_INT_PIN FUNC_GPIO14
#define SENSOR_INT_PIN_NO 14

#define SEND_DATA_QUEUE_LEN 4 // the queue length for send_data tasks
// task queue is used to offload work from the interrupt handler
static os_event_t* send_data_queue;

// set as soon as we get an ip address
static bool got_ip = false;

static struct espconn data_connection;

// heartbeat timer
os_timer_t heartbeat_timer;

extern int ets_uart_printf(const char *fmt, ...);

static void ICACHE_FLASH_ATTR init();
static void ICACHE_FLASH_ATTR init_wifi();
static void ICACHE_FLASH_ATTR init_data_connection();
static int ICACHE_FLASH_ATTR init_sensor();
static void ICACHE_FLASH_ATTR init_sensor_interrupt();

static void ICACHE_FLASH_ATTR on_wifi_event(System_Event_t *event);
static void gpio_intr_handler(uint32 intr_mask, void *arg);
static void send_data_handler(os_event_t* e);

static void ICACHE_FLASH_ATTR heartbeat_tick();

void ICACHE_FLASH_ATTR user_init(void) {
	// initialise system
	gpio_init();

	uart_init(BIT_RATE_115200, BIT_RATE_115200);
	os_delay_us(2000);

	ets_uart_printf("\n Sensor %d Startup! \n", SENSOR_ID);

	// setup callback to start program
	system_init_done_cb(init);
}

void init() {
	init_wifi();

	i2c_init();

	init_data_connection();

	if (init_sensor()) {
		ets_uart_printf("init_sensor failed.\n");
		return;
	}

	init_sensor_interrupt();

    //os_timer_disarm(&heartbeat_timer);
    //os_timer_setfn(&heartbeat_timer, (os_timer_func_t *)heartbeat_tick, (void *)0);
    //os_timer_arm(&heartbeat_timer, HEARTBEAT_INTERVAL, 1);
}

/*
 * configure wifi station mode and connect to the network
 */
void init_wifi() {
	ets_uart_printf("Initialising Wifi\n");
	ets_uart_printf("ssid: %s\n", SSID);


	// the esp saves the last joined wifi network and
	// tries to connect automatically. Stop that.
	wifi_station_disconnect();
	wifi_station_dhcpc_stop();

	// deactivate built-in access point.
	wifi_set_opmode(STATION_MODE);

	// configure station mode
	struct station_config stconfig;
	wifi_station_get_config(&stconfig);

	os_memset(stconfig.ssid, 0, sizeof(stconfig.ssid));
	os_memset(stconfig.password, 0, sizeof(stconfig.password));
	os_sprintf(stconfig.ssid, "%s", SSID);
	os_sprintf(stconfig.password, "%s", PASSWORD);
	if (!wifi_station_set_config(&stconfig)) {
		ets_uart_printf("Failed to configure Wifi station mode\n");
	}

    struct ip_info info;
    info.ip.addr = ipaddr_addr(LOCAL_IP);
    info.netmask.addr = ipaddr_addr(SUBNET);
    wifi_set_ip_info(STATION_IF, &info);

	// try to connect wifi and get an ip addres
	wifi_set_event_handler_cb(on_wifi_event);
	wifi_station_connect();
	//wifi_station_dhcpc_start();

	// automatically reconnect wifi
	wifi_station_set_auto_connect(1);
}

/*
 * initialise the data connection structure used to send
 * motion data to the server.
 */
void init_data_connection() {
	ets_uart_printf("Creating data connection \n");

	// use udp to send data
	//data_connection.proto.udp = &conn_proto;
	data_connection.type = ESPCONN_UDP;

	esp_udp conn_proto;
	data_connection.proto.udp = &conn_proto;

	// setup address/port
	char server_ip[15];
	os_sprintf(server_ip, "%s", SERVER);

	uint32_t ip = ipaddr_addr(server_ip);
	os_memcpy(data_connection.proto.udp->remote_ip, &ip, 4);
	data_connection.proto.udp->remote_port = SERVER_PORT;
	data_connection.proto.udp->local_port = LOCAL_PORT;

	espconn_create(&data_connection);
}

int init_sensor() {
	// mostly taken from InvenSenses example implementation
	// in the motion_driver release 5.1.3
	ets_uart_printf("i2c Scan \n");
	uint8_t i;
	for (i = 1; i < 127; i++) {
		i2c_start();
		i2c_writeByte(i << 1);
		if (i2c_check_ack()) {
			ets_uart_printf("found device at: 0x%2x\n", i);
		}
		i2c_stop();
	}
	ets_uart_printf("done\n");

	ets_uart_printf("initialising sensor \n");

	int status;
	if ((status = mpu_init(0)) != 0) {
		ets_uart_printf("mpu_init failed. Status: %d\n", status);
		return 1;
	}

	// enable accelerometer and gyro sensors
	if (mpu_set_sensors(INV_XYZ_ACCEL | INV_XYZ_GYRO)) {
		ets_uart_printf("mpu_set_sensors failed\n");
		return 1;
	}

	if (mpu_configure_fifo(INV_XYZ_GYRO | INV_XYZ_ACCEL)) {
		ets_uart_printf("mpu_configure_fifo failed\n");
		return 1;
	}

	if (mpu_set_sample_rate(100)) {
		ets_uart_printf("mpu_set_sample_rate failed\n");
		return 1;
	}

	ets_uart_printf("uploading dmp firmware\n");

	// the upload takes a while so we have to stop the watchdog timer
	// otherwise the chip may reset.
	system_soft_wdt_stop();
	if (dmp_load_motion_driver_firmware()) {
		ets_uart_printf("dmp_load_motion_driver_firmware failed\n");
		system_soft_wdt_restart();
		return 1;
	}
	system_soft_wdt_restart();

	// register feature callbacks... (we can probably remove this)
	if (dmp_register_tap_cb(0))
		ets_uart_printf("dmp_register_tap_cb failed\n");
	if (dmp_register_android_orient_cb(0))
		ets_uart_printf("dmp_register_android_orient_cb failed\n");

	if (dmp_enable_feature(
			DMP_FEATURE_6X_LP_QUAT | DMP_FEATURE_TAP |
			DMP_FEATURE_ANDROID_ORIENT | DMP_FEATURE_SEND_RAW_ACCEL
					| DMP_FEATURE_SEND_CAL_GYRO |
					DMP_FEATURE_GYRO_CAL)) {
		ets_uart_printf("dmp_enable_feature failed\n");
		return 1;
	}

	if (mpu_set_accel_fsr(4)) {
		ets_uart_printf("mpu_set_accel_fsr failed\n");
		return 1;
	}

	if (dmp_set_fifo_rate(SAMPLE_RATE)) {
		ets_uart_printf("dmp_set_fifo_rate failed\n");
	}

	// start dmp processing
	if (mpu_set_dmp_state(1)) {
		ets_uart_printf("mpu_set_dmp_state failed\n");
		return 1;
	}

	ets_uart_printf("mpu/dmp running\n");

	return 0;
}

void init_sensor_interrupt() {
	// setup interrupt pins
	PIN_FUNC_SELECT(SENSOR_INT_MUX, SENSOR_INT_PIN);
	GPIO_DIS_OUTPUT(SENSOR_INT_PIN_NO);
	PIN_PULLUP_EN(SENSOR_INT_MUX); // pull - up pin

	// setup task queue
	send_data_queue = (os_event_t*) os_malloc(
			sizeof(os_event_t) * SEND_DATA_QUEUE_LEN);
	if (!(system_os_task(send_data_handler, USER_TASK_PRIO_2, send_data_queue,
			SEND_DATA_QUEUE_LEN))) {
		ets_uart_printf("task setup failed\n");
	}

	// enable interrup handler
	gpio_pin_intr_state_set(GPIO_ID_PIN(14), GPIO_PIN_INTR_NEGEDGE);
	gpio_intr_handler_register(gpio_intr_handler, NULL);
}

void heartbeat_tick() {
	ets_uart_printf("vdd: %d\n", readvdd33());
}

/*
 * is called by the wifi driver on any wifi related event
 */
void on_wifi_event(System_Event_t *event) {
	switch (event->event) {
	case EVENT_STAMODE_GOT_IP:
		ets_uart_printf("Event: EVENT_STAMODE_GOT_IP\n");
		got_ip = true;
		break;
	case EVENT_STAMODE_CONNECTED:
		ets_uart_printf("EVENT_STAMODE_CONNECTED\n");
		got_ip = true;
		break;
	case EVENT_STAMODE_DISCONNECTED:
		ets_uart_printf("EVENT_STAMODE_DISCONNECTED\n");
		got_ip = false;
		break;
	case EVENT_STAMODE_AUTHMODE_CHANGE:
		ets_uart_printf("EVENT_STAMODE_AUTHMODE_CHANGE\n");
		break;
	case EVENT_SOFTAPMODE_STACONNECTED:
		ets_uart_printf("EVENT_SOFTAPMODE_STACONNECTED\n");
		break;
	case EVENT_SOFTAPMODE_STADISCONNECTED:
		ets_uart_printf("EVENT_SOFTAPMODE_STADISCONNECTED\n");
		break;
	default:
		ets_uart_printf("Unexpected wifi event: %d\n", event->event);
		break;
	}
}

/*
 * called when the mpu raises an interrupt to show that data is ready
 */
void gpio_intr_handler(uint32 intr_mask, void *arg) {
	if (got_ip)
	{
		if (!system_os_post(USER_TASK_PRIO_2, 0, 0)) {
			ets_uart_printf("post failed!\n");
		}
	}

	// this seems weird, we have to reset the interrupt state
	// every time
	gpio_intr_ack(intr_mask);
	gpio_pin_intr_state_set(GPIO_ID_PIN(SENSOR_INT_PIN_NO),
			GPIO_PIN_INTR_NEGEDGE);
}

static void send_data_handler(os_event_t* e) {
	if (!got_ip)
		return;

	// read data from mpu buffer
	short gyro[3], accel[3], sensors;
	unsigned char more;
	long quat[4];
	unsigned long timestamp;
	if (dmp_read_fifo(gyro, accel, quat, &timestamp, &sensors, &more)) {
		ets_uart_printf("read_fifo_failed \n");
		return;
	}

	// send sensor data to server
	long data[12];
	data[0] = SENSOR_ID;
	data[1] = quat[0];
	data[2] = quat[1];
	data[3] = quat[2];
	data[4] = quat[3];
	data[5] = accel[0];
	data[6] = accel[1];
	data[7] = accel[2];
	data[8] = gyro[0];
	data[9] = gyro[1];
	data[10] = gyro[2];
	data[11] = timestamp;

	sint8 status = espconn_sendto(&data_connection, (uint8*) data,
			12 * sizeof(long));
	if (status) {
		ets_uart_printf("espconn_sendto failed. status: %d \n", status);
	}
}

