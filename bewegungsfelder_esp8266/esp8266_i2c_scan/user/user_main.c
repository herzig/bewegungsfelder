/*
   Small ESP8266 program to scan for available i2c devices
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
#include "uart.h"

extern int ets_uart_printf(const char *fmt, ...);

#define INTERVAL 1000 // scan interval in ms
static ETSTimer scan_timer;


static void ICACHE_FLASH_ATTR i2c_scan();

void user_rf_pre_init(void) {}

void ICACHE_FLASH_ATTR init()
{
	// initialise i2c driver
	i2c_init();

	// setup timer to scan every second
	os_timer_disarm(&scan_timer);
	os_timer_setfn(&scan_timer, (os_timer_func_t*)i2c_scan, NULL);
	os_timer_arm(&scan_timer, INTERVAL, 1);
}

void ICACHE_FLASH_ATTR user_init(void)
{
	// initialise system
	gpio_init();
	uart_init(BIT_RATE_115200, BIT_RATE_115200);
	os_delay_us(2000);

	ets_uart_printf("ESP8266 STARTUP\n");

	// setup callback to start program
	system_init_done_cb(init);

}

// Scans all i2c addresses [0,127] and writes a list of responding
// devices to UART
//
static void ICACHE_FLASH_ATTR i2c_scan() {

	ets_uart_printf("i2c Scan \n");
	uint8_t i;
	for (i=1; i<127; i++) {
		i2c_start();
		i2c_writeByte(i << 1);
		if (i2c_check_ack()) {
			ets_uart_printf("Found device at: 0x%2x\n", i);
		}
		i2c_stop();
	}
	ets_uart_printf("done\n");
}
