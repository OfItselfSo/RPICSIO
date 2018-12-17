using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

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
    /// Provides Tools and Utilities for RPI GPIO's
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Started
    /// </history>
    public static class GpioUtils
    {
        public const string GPIOENUM_PREFIX = "GPIO_";

        // used for external file open calls
        const int O_RDONLY = 0x0;
        const int O_NONBLOCK = 0x0004;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a GpioID to a numeric string value. Will not accept a gpioID of 
        /// Gpio.GPIO_NONE
        /// </summary>
        /// <param name="gpioIDIn">The gpio we open the port on</param>
        /// <returns>>the GPIO as a string</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public static string GpioIDToString(GpioEnum gpioIDIn)
        {
            if (gpioIDIn == GpioEnum.GPIO_NONE) 
            {
                throw new Exception ("Invalid Gpio : " + gpioIDIn.ToString ());
            }

            // strip out non digits - want to do this fast so no regex
            // or cycling through the string testing every char.
            return gpioIDIn.ToString().Replace(GPIOENUM_PREFIX, "");
        }                

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets a GpioConfig object for a given gpio object. NOTE when the 
        /// GpioConfig object is instantiated it acesses system information
        /// for things like mux pin number and usage in the pinmux pins table
        /// </summary>
        /// <returns>>the GpioConfig object or a dummy (GpioEnum.GPIO_NONE) for fail</returns>
        /// <history>
        ///    01 Dec 16  Cynic  Originally written
        /// </history>
        public static GpioConfig GetGpioConfigForGpio(GpioEnum gpioIn)
        {
            switch (gpioIn)
            {
            case GpioEnum.GPIO_4:
                return new GpioConfig(7, true, 4, GpioEnum.GPIO_4);
            case GpioEnum.GPIO_5:
                return new GpioConfig(29, true, 5, GpioEnum.GPIO_5);
            case GpioEnum.GPIO_6:
                return new GpioConfig(31, true, 6, GpioEnum.GPIO_6);
            case GpioEnum.GPIO_12:
                return new GpioConfig(32, true, 12, GpioEnum.GPIO_12);
            case GpioEnum.GPIO_13:
                return new GpioConfig(33, true, 13, GpioEnum.GPIO_13);
            case GpioEnum.GPIO_16:
                return new GpioConfig(36, true, 16, GpioEnum.GPIO_16);
            case GpioEnum.GPIO_17:
                return new GpioConfig(11, true, 17, GpioEnum.GPIO_17);
            case GpioEnum.GPIO_18:
                return new GpioConfig(12, true, 18, GpioEnum.GPIO_18);
            case GpioEnum.GPIO_19:
                return new GpioConfig(35, true, 19, GpioEnum.GPIO_19);
            case GpioEnum.GPIO_20:
                return new GpioConfig(38, true, 20, GpioEnum.GPIO_20);
            case GpioEnum.GPIO_21:
                return new GpioConfig(40, true, 21, GpioEnum.GPIO_21);
            case GpioEnum.GPIO_22:
                return new GpioConfig(15, true, 22, GpioEnum.GPIO_22);
            case GpioEnum.GPIO_23:
                return new GpioConfig(16, true, 23, GpioEnum.GPIO_23);
            case GpioEnum.GPIO_24:
                return new GpioConfig(18, true, 24, GpioEnum.GPIO_24);
            case GpioEnum.GPIO_25:
                return new GpioConfig(22, true, 25, GpioEnum.GPIO_25);
            case GpioEnum.GPIO_26:
                return new GpioConfig(37, true, 26, GpioEnum.GPIO_26);
            case GpioEnum.GPIO_27:
                return new GpioConfig(13, true, 27, GpioEnum.GPIO_27);
            default:
                return new GpioConfig();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get all known GpioConfig objects. These are returned with hard coded 
        /// contents based on the documentation and not fully filled in with 
        /// information derived from the system. Never returns NULL.
        /// </summary>
        /// <returns>>a list of all known GpioConfig objects</returns>
        /// <history>
        ///    01 Dec 16  Cynic  Originally written
        /// </history>
        public static List<GpioConfig> BuildAllKnownGpioConfigObjects()
        {
            List<GpioConfig> outList = new List<GpioConfig> ();

            // run down through the GpioEnum and create one for every
            // value it contains. 
            foreach (GpioEnum gpioVal in (GpioEnum[])Enum.GetValues(typeof(GpioEnum)))
            {
                // never do this one
                if (gpioVal == GpioEnum.GPIO_NONE) continue;
                outList.Add(GetGpioConfigForGpio(gpioVal));
            }
            return outList;
        }       
    }
}

