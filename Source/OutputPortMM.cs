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
    /// Provides the Output Port functionality for the RPICSIO Library
    /// (Memory Mapped Version)
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public class OutputPortMM : PortMM
    {

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gpioIDIn">The gpio we open the port on</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public OutputPortMM (GpioEnum gpioIDIn) : base(gpioIDIn)
        {
            // open the port
            OpenPort ();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gpioIDIn">The gpio we open the port on</param>
        /// <param name="initialState">The initial state, true or false, of the port
        /// after we open it.
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public OutputPortMM (GpioEnum gpioIDIn, bool initialState) : base(gpioIDIn)
        {
            // open the port
            OpenPort ();
            // set the initial value
            Write(initialState);
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
            // be GPIOPinMuxModeEnum.GPIOMODE_OUTPUT
            MMDevMem.SetGPIOPinMuxState(this.GpioCfgObject, GPIOPinMuxModeEnum.GPIOMODE_OUTPUT);
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

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the port value
        /// </summary>
        /// <param name="valueToSet>true - 1 or pin high, false - 0 or pin low/param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void Write(bool valueToSet)
        {
            if (PortIsOpen == false) throw new Exception("Port is not open");

            // write the gpio pin state
            MMDevMem.WriteGPIOPin(GpioCfgObject, valueToSet);
        }
    }
}

