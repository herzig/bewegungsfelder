<img alt='CI Build Status' src='https://ci.appveyor.com/api/projects/status/github/herzig/bewegungsfelder?svg=true'></img>

# Bewegungsfelder
Inertial Motion Capture for everyone

<img alt="Bewegungsfelder Screenshot" src="mainwindow.png" width='500px'></img>

Bewegungsfelder is a mobile & customizable inertial motion capture system for skeletal animation. It consists of C#/WPF Application to capture and record skeletal animations and standalone IMU sensor modules based on the ESP8266 Wifi SoC.

* Flexible skeleton definition.
* Live 3D visualisation of sensors and skeleton pose.
* Recording/Playback of animations.
* BVH export & import.
* UDP Server accepts incoming sensor values.

### Future
* Smartphones as sensors (poc done).
* Improve timing issues/add proper interpolation.
* Support intermediate joint/smoothin using IK.
* Docs.

#### Smartphone Sensors
Prototype using websockets in develop branch:
1. Start Bewegungsfelder.exe
2. Make sure Smartphone can connect to your PC (i.e. same wifi network)
2. On Smartphone open http://[your-local-ip]:8080
3. Mocap!

#### ESP8266 & MPU6050 Sensors

Sensor fusion is done on the MPU6050 by the InvenSense DMP Firmware.

The ESP8266 reads values from the MPU6050 motion sensors using I2C.

The official Esspressif ESP8266 non-os SDK is used.

<img alt='Bewegungsfelder ESP8265 and MPU6050 Hardware' src='hardware.jpg' width='500px'></img>
