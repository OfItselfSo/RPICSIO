# RPICSIO

RPICSIO is a free and open source .NET v4 library which provides a comprehensive C# input/output solution for the Raspberry Pi 2 Mono environment. Using RPICSIO, you can easily read and write to the GPIO pins (and trigger interrupt events from state changes) and launch and control SPI, I2C, PWM and UART devices. RPICSIO is intended to be a comprehensive solution for I/O on the Raspberry Pi 2. 

At the current time, RPICSIO has only been tested on the Raspberry Pi 2. Most things will probably work on the Raspberry Pi 3 - but it has not been verified and the functionality on the Raspberry Pi 0 is unknown.

## Capabilities

- Provides simple and transparent read/write access (including the triggering of events) to the GPIO pins of a Raspberry Pi 2. A maximum output frequency of about 2.4MHz is possible when using a memory mapped port class and about 1.1Khz when using the SYSFS class.
- The SPI port is fully supported. It is possible to use GPIO ports to provide a large number of SPI device select lines.
- The I2C port is fully supported.
- The Serial/UART port is fully supported.
- The Pulse Width Modulation (PWM) device is fully supported. The PWM duty cycle can be specified in nano-seconds or as a percentage of the base frequency.
- Developed on Raspbian Linux v4.4.32 freely available for the Raspberry Pi.
- Tested with the Mono JIT compiler v3.2.8 but is probably also compatible with other versions.
- The software is written in C# and a .NET project is included with the source code. 

The RPICSIO Project is open source and released under the MIT License. The home page for this project can be found at [http://www.OfItselfSo.com/ RPICSIO](http://www.OfItselfSo.com/ RPICSIO) and contains help files, manual, sample code and useful advice and assistance.
