/*
    I2C driver for the ESP8266 
    Copyright (C) 2014 Rudy Hardeman (zarya)
    Copyright (C) 2016 Ivo Herzig:
    	Added i2c_readBytes and i2c_writeBytes methods.


    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

#ifndef __I2C_H__
#define __I2C_H__

#include "ets_sys.h"
#include "osapi.h"
#include "gpio.h"

#define I2C_SLEEP_TIME 5

// SDA on GPIO4
#define I2C_SDA_MUX PERIPHS_IO_MUX_GPIO4_U
#define I2C_SDA_FUNC FUNC_GPIO4
#define I2C_SDA_PIN 4

// SCK on GPIO5
#define I2C_SCK_MUX PERIPHS_IO_MUX_GPIO5_U
#define I2C_SCK_FUNC FUNC_GPIO5
#define I2C_SCK_PIN 5

#define esp_i2c_read() GPIO_INPUT_GET(GPIO_ID_PIN(I2C_SDA_PIN));

void i2c_init(void);
void i2c_start(void);
void i2c_stop(void);
void i2c_send_ack(uint8 state);
uint8 i2c_check_ack(void);
uint8 i2c_readByte(void);
void i2c_writeByte(uint8 data);

int i2c_readBytes(unsigned char slave_addr, unsigned char reg_addr,
		unsigned char length, unsigned char* data);

int i2c_writeBytes(unsigned char slave_addr, unsigned char reg_addr,
		unsigned char length, unsigned char const *data);


#endif
