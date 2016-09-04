/*
   Defines functions needed by the MPU6050 & DMP driver
   (inv_mpu.c, inv_mpu_dmp_motion_driver.c)

   Include this file before any MPU/DMP header
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

#ifndef ESP_MPU_H
#define ESP_MPU_H

#include <i2c.h>
#include <osapi.h>
#include <c_types.h>

#define MPU6050 // we use mpu6050 sensors

// i2c methods to reand and write bytes from a slave
#define i2c_write i2c_writeBytes
#define i2c_read i2c_readBytes

// timing functions.
#define delay_ms(ms) os_delay_us( ms * 1000)
static inline void get_ms(unsigned long* t) {
	*t = (system_get_time() / 1000);
}

// arithmetics
#define labs(x) (((x)>0)?(x):-(x))
#define fabs(x) (((x)>0)?(x):-(x))
#define min(a,b) ((a<b)?a:b)

// logging
#define log_i ets_uart_printf
#define log_e ets_uart_printf
#define uart_printf ets_uart_printf

#endif




