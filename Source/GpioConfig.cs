using System;
using System.Text;
using System.IO;

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
    /// Provides a container to correlate HeaderPinNumber, RegisterOffsets etc
    /// for the GPIO and also contain the configuration state of the GPIO
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Started
    /// </history>
    public class GpioConfig
    {
        public const int NO_GPIOBANK = -1;
        public const int NO_GPIOBIT = -1;
        public const int NO_GPIOMASK = 0;
        public const int NO_HEADER = -1;
        public const int NO_GPIO = -1;
        public const int NO_HEADERPIN = -1;
        public const int NO_MUXPIN = -1;
        public const int NO_PINMUXREGISTEROFFSET = -1;
        public const int NO_GPIOSETTING = -1;
        public const string MUX_UNCLAIMED = "UNCLAIMED";
        public const string GPIO_UNCLAIMED = "UNCLAIMED";

        private GpioEnum gpio = GpioEnum.GPIO_NONE;
        private int gpioNum = NO_GPIO;
        private int headerPin = NO_HEADERPIN;
        private int muxPin = NO_MUXPIN;
        private bool isDefaultGPIOOnHeader = false;
        private int gpioBank = NO_GPIOBANK;
        private int gpioBit = NO_GPIOBIT;

        private int gpioSetting = NO_GPIOSETTING;
 
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor - only used to set up a dummy default GpioConfig object
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public GpioConfig()
        {
        }
            
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="headerPinIn">the number of the header pin
        /// <param name="isDefaultGPIOOnHeaderIn">if true it is available as a GPIO by default on the header. False it is not</param>
        /// <param name="gpioNumIn">The gpio as a number</param>
        /// <param name="gpioIn">The gpio as an enum</param>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public GpioConfig(int headerPinIn, bool isDefaultGPIOOnHeaderIn, int gpioNumIn, GpioEnum gpioIn)
        {
            // set some values, ignore others - they are for future use
            headerPin = headerPinIn;
            isDefaultGPIOOnHeader = isDefaultGPIOOnHeaderIn;
            gpioNum = gpioNumIn;
            gpio = gpioIn;

            // these values are pre-calculated. We use them a lot
            gpioBank = GpioNum / 32; // the bank number in the pinmux is the GPIO/32
            gpioBit = GpioNum % 32; // the bit number in the pinmux is the remainder of GPIO/32
 
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the Gpio
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public GpioEnum Gpio
        {
            get
            {
                return gpio;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the gpioNum
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int GpioNum
        {
            get
            {
                return gpioNum;
            }
        }
            
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the gpioBank
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int GpioBank
        {
            get
            {
                return gpioBank;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the gpioBank offset
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int GpioBankOffset
        {
            get
            {
                // There are 54 GPIOs, most of the functions they use
                // are in two consecutive 32 bit registers. GPIOs 0-31
                // are in the low register and 32-53 in the high register
                // this just provides a central location to calculate this offset
                if (GpioNum >= 32) return 4;
                else return 0;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the mode of the GPIO - this is the setting of the pinmux
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public GPIOPinMuxModeEnum GpioMode
        {
            get
            {
                return ConvertModeIntToEnum(MMDevMem.GetGPIOPinMuxState(this));
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the memory mappings for the GPIO banks. Will never return null.
        /// 
        /// NOTE: the MemoryMapDevMem class is a singleton class. This means that 
        /// program wide there can ever only be one instantiation of these. If you 
        /// don't know what a singleton class is, you should look it up. Otherwise 
        /// you will never really understand how this works.
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        protected MemoryMapDevMem MMDevMem
        {
            get
            {
                return MemoryMapDevMem.Instance;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the mode mask for the GPIO 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int GpioPinMuxModeMask
        {
            get
            {
                // There are 10 GPIOs of three bits each 
                // per function register. LSB of the register
                // has the lowest GPIO and so in ascending order
                // for example, GPIO_0 is in bits 2-0
                //              GPIO_1 is in bits 5-3
                //              GPIO_10 is in bits 2-0 but in the next register up

                // the mask can be calculated by the formula below
                int mask = 0x07; // (111 in binary)
                mask = mask << GpioPinMuxModeShift;
                return mask;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the number of bits we have to shift the function (pinmux) register to 
        /// convert it to or from an int
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int GpioPinMuxModeShift
        {
            get
            {
                // There are 10 GPIOs of three bits each 
                // per function register. LSB of the register
                // has the lowest GPIO and so in ascending order
                // for example, GPIO_0 is in bits 2-0
                //              GPIO_1 is in bits 5-3
                //              GPIO_10 is in bits 2-0 but in the next register up

                // the shift can be calculated by the formula below
                return ((GpioNum % 10)*3);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a pinmux mode integer to an enum
        /// </summary>
        /// <param name="gpioModeNum">The GPIO Mode as an integer</param>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public GPIOPinMuxModeEnum ConvertModeIntToEnum(int gpioModeNum)
        {
            // the range is 0-7
            if ((gpioModeNum < 0) || (gpioModeNum > 7))
            {
                // probably the safest we can do
                return GPIOPinMuxModeEnum.GPIOMODE_INPUT;
            }
            // the cast should convert it for us
            return (GPIOPinMuxModeEnum)gpioModeNum;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a pinmux mode enum to an integer
        /// </summary>
        /// <param name="gpioModeIn">The GPIO Mode</param>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int ConvertModeEnumToInt(GPIOPinMuxModeEnum gpioModeIn)
        {
             // the cast should convert it for us
            return (int)gpioModeIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the GPIO is a default GPIO on the header rather than a
        /// needing to be muxed in because that pin is used by some other device.
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public bool IsDefaultGPIOOnHeader
        {
            get
            {
                return isDefaultGPIOOnHeader;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the gpioBit
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int GpioBit
        {
            get
            {
                return gpioBit;
            }
        }
            
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the gpioMask
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int GpioMask
        {
            get
            {
                // the gpios are mapped into two sequential registers. GPIOs 0-31
                // are in the first register and 32-53 are in the next one. Usually things
                // are set and reset by writing a 1 to the appropriate register
                // and the position of this bit is determined by the gpio number
                if (GpioNum >= 32)
                {
                    // we are in the second bank, shift it up by 
                    // the gpioNum-32
                    return 0x01 << (GpioNum-32);
                }
                else
                {
                    // just shift it up by the GpioNum
                    return 0x01 << GpioNum;
                }
            }
        }
            
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the Gpio Settings as a human readable string
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public string GpioSettingsAsString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Gpio.ToString());
            sb.Append(", MuxMode=" + GpioMode.ToString());

            return sb.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the headerPin
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int HeaderPin
        {
            get
            {
                return headerPin;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the muxPin
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int MuxPin
        {
            get
            {
                return muxPin;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the pinmuxRegisterOffset
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int PinmuxRegisterOffset
        {
            get
            {
                // GPIOs 0-9 get an offset of 0
                // GPIOs 10-19 get an offset of 4
                // GPIOs 20-29 get an offset of 8
                // and so on. So the formula below works
                return ((GpioNum/10)*4);
            }
        }
            
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the pinmuxRegister offset as hex string
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public string PinmuxRegisterOffsetAsHexString
        {
            get
            {
                return PinmuxRegisterOffset.ToString("x3");
            }
        }
                        
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the event bank number.  This function just recovers the bank from the calculated 
        /// GPIO number. 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int EventBank
        {
            get
            {
                // the bank number in the pinmux is the GPIO/32
                return GpioNum / 32;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the event bit number. This is the offset of a GPIO into its bank. 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int EventBit
        {
            get
            {
                // the bit number in the pinmux is the remainder of GPIO/32
                return GpioNum % 32;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the GpioSettings. This is the raw hex bits that determine the various
        /// mode settings
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int GpioSetting
        {
            get
            {
                return gpioSetting;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the MuxMode. These are the three lsb bits of the gpioSetting. 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int MuxMode
        {
            get
            {
                return gpioSetting & 0x07;
            }
        }           

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if pullups/pulldowns are enabled
        /// </summary>
        /// <returns>true they are enabled, false they are not</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public bool PullupsPulldownsEnabled
        {
            get
            {
                // a 0 at bit 3 means "enabled"
                if ((gpioSetting & 0x08) == 0) return true;
                else return false;    
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if the gpio is in pull up or pull down mode. Note the actual
        /// pull up or pull down is only truely operational if PullupsPulldownsEnabled
        /// is true
        /// </summary>
        /// <returns>1 pullup mode active, 0 pull down mode active</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Started
        /// </history>
        public int PullupPulldownMode
        {
            get
            {
                if ((gpioSetting & 0x10) != 0)
                    return 1;
                else
                    return 0;
            }
        }           
    }
}

