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
    /// Provides the Input Port functionality for a RPICSIO Library. This
    /// is the memory mapped version.
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public class InputPortMM : PortMM
    {
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor, disables PullUps and PullDowns
        /// </summary>
        /// <param name="gpioIDIn">The gpio we open the port on</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public InputPortMM(GpioEnum gpioIDIn) : base(gpioIDIn)
        {
            // open the port
            OpenPort();
            // set the mode to disable pullUpDown
            SetPullUpDownModeForPort(GPIOPullUpDownModeEnum.PULLUPDOWN_OFF);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gpioIDIn">The gpio we open the port on</param>
        /// <param name="pullUpDownMode">The pull up or pull down mode</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public InputPortMM(GpioEnum gpioIDIn, GPIOPullUpDownModeEnum pullUpDownMode) : base(gpioIDIn)
        {
            // open the port
            OpenPort();
            // set the mode
            SetPullUpDownModeForPort(pullUpDownMode);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the pinmux modes for the port 
        /// </summary>
        /// <param name="pullUpDownMode">The pull up or pull down mode</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void SetPullUpDownModeForPort(GPIOPullUpDownModeEnum pullUpDownMode)
        {
             MMDevMem.SetGPIOPullUpDownMode(GpioCfgObject, pullUpDownMode);
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
            // only one pin for this type of port and the mode should always
            // be GPIOPinMuxModeEnum.GPIOMODE_INPUT
            MMDevMem.SetGPIOPinMuxState(GpioCfgObject, GPIOPinMuxModeEnum.GPIOMODE_INPUT);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the port value
        /// 
        /// </summary>
        /// <returns>1 or 0 for true or false - the ports value</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public bool Read()
        {
            if (PortIsOpen == false) throw new Exception("Port is not open");
            // read the gpio pin state and return it
            return MMDevMem.ReadGPIOPin(GpioCfgObject);
        }

    }
}

