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
    /// Provides the Input Port functionality for the RPICSIO library
    /// This is the SYSFS version
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public class InputPortFS : PortFS
    {
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gpioIDIn">The gpio we open the port on</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public InputPortFS(GpioEnum gpioIDIn) : base(gpioIDIn)
        {
            // open the port
            OpenPort ();
            // set the direction to input
            SetSysFsDirection();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the port value
        /// 
        /// This is really just doing the equivalent of a shell command 
        ///    echo <value_as_string> > /sys/class/gpio/gpio<gpioID>/value
        /// 
        /// </summary>
        /// <returns>true or false - the ports value</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public bool Read()
        {
            string outStr = System.IO.File.ReadAllText(RPIDefinitions.SYSFS_GPIODIR+RPIDefinitions.SYSFS_GPIODIRNAMEBASE+GpioUtils.GpioIDToString(GpioID)+"/"+RPIDefinitions.SYSFS_GPIOVALUE);

            if(outStr.Trim() == "0") return false;
            else return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the pinmux modes for the port 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public override void SetPinMuxModesForPort()
        {
            // do nothing, FS handles this
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the PortDirection
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public override FSPortDirectionEnum PortDirection()
        {
            return FSPortDirectionEnum.PORTDIR_INPUT;
        }

    }
}

