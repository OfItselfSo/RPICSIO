using System;

/// +------------------------------------------------------------------------------------------------------------------------------+
/// |                                                   TERMS OF USE: MIT License                                                  |
/// +------------------------------------------------------------------------------------------------------------------------------|
/// |Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    |
/// |files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    |
/// |modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software|
/// |is furnished to do so, subject to the following conditions:                                                                   |
/// |                                                                                                                              |
/// |The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.|
/// |                                                                                                                              |
/// |THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          |
/// |WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         |
/// |COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   |
/// |ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         |
/// +------------------------------------------------------------------------------------------------------------------------------+

namespace RPICSIO
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// Provides a central location to define various system values 
    /// for the RPICSIO library
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public static class RPIDefinitions
    {
        // the sysfs base GPIODIR
        public const string SYSFS_GPIODIR = "/sys/class/gpio/";
        // the sysfs export file/device
        public const string SYSFS_GPIOEXPORT = "export";
        // the sysfs unexport file/device
        public const string SYSFS_GPIOUNEXPORT = "unexport";
        // the base part of the gpio directory created in the
        // SYSFS_GPIODIR once we export the gpio
        public const string SYSFS_GPIODIRNAMEBASE = "gpio";
        // the sysfs direction file/device
        public const string SYSFS_GPIODIRECTION = "direction";
        // the sysfs value file/device
        public const string SYSFS_GPIOVALUE = "value";

        // the memory mapped device file on the RPI
        public const string DEVMEM_FILE = @"/dev/mem";

        // the base address of the FUNCTION SELECT registers
        public const long GPIOPINMUX_BASE_ADDRESS = 0x3F200000 ;
        // the size of memory we map into from the DEVMEM_FILE
        public const long GPIOPINMUX_MAPSIZE = 0x18;  // the six function select registers

        // the location of the GPIO base memory
        public const long GPIO_BASE_ADDRESS = 0x3F200000;
        // the size of memory we map into from the DEVMEM_FILE
        public const long GPIO_MAPSIZE = 0xA0;

        // individual GPIO register offsets
        public const long GPIO_SETDATAOUT = 0x1C;
        public const long GPIO_CLEARDATAOUT = 0x28;
        public const long GPIO_DATAIN = 0x34;

        public const long GPIO_GPEDS = 0x40;
        public const long GPIO_GPREN = 0x4C;
        public const long GPIO_GPFEN = 0x58;
        public const long GPIO_GPHEN = 0x64;
        public const long GPIO_GPLEN = 0x70;
        public const long GPIO_GPAREN = 0x7C;
        public const long GPIO_GPAFEN = 0x88;

        public const long GPIO_GPPUD = 0x94;
        public const long GPIO_GPPUDCLK = 0x98;

        // the template for the I2CDEV device file
        public const string I2CDEV_FILENAME = "/dev/i2c-%port%";

        // the template for the SPIDEV device file
        public const string SPIDEV_FILENAME = "/dev/spidev%device%.%slave%";

        // the templates for the PWM device files
        public const string PWM_CHIPFILENAME_BASE = "/sys/class/pwm/pwmchip%chip%";
        public const string PWM_FILENAME_EXPORT = "export";
        public const string PWM_FILENAME_UNEXPORT = "unexport";
        public const string PWM_FILENAME_DUTY = "pwm%port%/duty_cycle";
        public const string PWM_FILENAME_PERIOD = "pwm%port%/period";
        public const string PWM_FILENAME_RUN = "pwm%port%/enable";
        public const string PWM_FILENAME_POLARITY = "pwm%port%/polarity";

        // the template for the TTY device file
        public const string TTYDEV_FILENAME = "/dev/ttyAMA%tty%";

    }
}

