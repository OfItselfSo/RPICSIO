using System;
using System.IO;
using System.Threading;

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
    /// Provides the Interrupt Port functionality for the RPICSIO Library
    /// (MemoryMapped Version)
    /// 
    /// Q: What is really happening here!
    /// A: These are not true interrupts. They have much more in common with
    ///    a polled event although the event will "interrupt" the Main() thread
    ///    and execute code in an object listening on them.
    ///   
    ///    All GPIOs are associated with one of two banks. The act of creating 
    ///    an InterruptPort on a GPIO will start an event monitoring thread for 
    ///    that bank. This GPIO bank event monitoring thread is continuously 
    ///    computable and there is only one thread per bank. 
    /// 
    ///    The InterruptPort registers with the GPIO bank event monitoring thread  
    ///    and when a pin state change of significance is detected the SendInterruptEvent
    ///    function in this class will be called. The SendInterruptEvent function
    ///    will place the relevant information into an EventData object and then
    ///    send it on to anybody listening on the InterruptPorts OnInterrupt delegate
    /// 
    ///    Be aware that the OnInterrupt handler method which accepts the data from 
    ///    the C# event is executing in the Gpio banks event monitoring thread 
    ///    - not the main() thread. You have to be careful what you do in there to 
    ///    avoid the many issues associated with multi-threaded applications. 
    ///    Also, it is not advisable to take too long to process the incoming event 
    ///    data - you cannot receive another interrupt while processing that one.
    /// 
    ///    PRIORITYS: THe GPIO bank event monitoring thread will detect changes
    ///    and then run down its list of registered InterruptPort until it finds
    ///    the correct one for that pin state change. The order of these registered
    ///    InterruptPorts is the Priority - highest first. Thus, if two pins change
    ///    state simultaneously then only one will get processed. If on a subsequent
    ///    pass the second interrupts pin state is still changed, it will get 
    ///    processed then. If it has changed back it will not be processed and the
    ///    interrupt event will be lost.
    /// 
    ///    CLOSING DOWN: Be aware that a call to DisableInterrupt() does not stop 
    ///    the worker thread. You need to do an explicit ClosePort() call to stop 
    ///    the thread and release any resources the InterruptPort is using. Always 
    ///    explicitly Dispose() all ports when you are done with them. 
    /// 
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public class InterruptPortMM : PortMM, IComparable
    {
        private InterruptMode eventInterruptMode = InterruptMode.InterruptNone;
        private bool interruptIsEnabled = false;
        private bool interruptClearToSend = false;
        public event InterruptEventHandlerMM OnInterrupt;

        // a user specified code which is included with every event sent
        private int evCode=0;
        private int interruptEventsReceived = 0;
        private int interruptEventsSent = 0;

        // the priority of this InterruptPort relative to other
        // Interrupt Ports on the same bank. Larger value means
        // higher priority and hence is serviced first. Equal
        // priorities imply it is undefined which gets serviced first
        private int interruptEventPriority = 0;

        // Values to support interrupt event detection. These are public - we need 
        // speed of access here and I do not want to add the overhead of a property get accessor

        // a mask to detect our ports bit in the register. zero for disabled interrupt
        public int InterruptMask = 0; 

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventInterruptModeIn">The interrupt mode</param>
        /// <param name="gpioIn">The GPIO this interrupt is on.
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public InterruptPortMM (GpioEnum gpioIn, InterruptMode eventInterruptModeIn) : base(gpioIn)
        {
            EventInterruptMode = eventInterruptModeIn;
            // setup for event detection
            SetEventMasks();
            // open the port and turn on event detection
            OpenPort();
            // set the mode to disable pullUpDown
            MMDevMem.SetGPIOPullUpDownMode(GpioCfgObject, GPIOPullUpDownModeEnum.PULLUPDOWN_OFF);
            // set up the interrupts
            MMDevMem.DisableAllInterruptsAtGPIOLevel(GpioCfgObject);
            MMDevMem.EnableInterruptsAtGPIOLevelAccordingToEnum(GpioCfgObject, EventInterruptMode);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventInterruptModeIn">The interrupt mode</param>
        /// <param name="gpioIn">The GPIO this interrupt is on.
        /// <param name="pullUpDownMode">The pull up or pull down mode</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public InterruptPortMM (GpioEnum gpioIn, GPIOPullUpDownModeEnum pullUpDownMode, InterruptMode eventInterruptModeIn) : base(gpioIn)
        {
            EventInterruptMode = eventInterruptModeIn;
            // setup for event detection
            SetEventMasks();
            // open the port and turn on event detection
            OpenPort();
            // set the mode to disable pullUpDown
            MMDevMem.SetGPIOPullUpDownMode(GpioCfgObject, pullUpDownMode);
            // set up the interrupts
            MMDevMem.DisableAllInterruptsAtGPIOLevel(GpioCfgObject);
            MMDevMem.EnableInterruptsAtGPIOLevelAccordingToEnum(GpioCfgObject, EventInterruptMode);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventInterruptModeIn">The interrupt mode</param>
        /// <param name="gpioIn">The GPIO this interrupt is on.</param>
        /// <param name="evCodeIn">A user specified code which appears in the events</param>
        /// <param name="evPriorityIN">the event priority, higher numbers processed first</param>
        /// <param name="pullUpDownMode">The pull up or pull down mode</param>
         /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public InterruptPortMM (GpioEnum gpioIn, GPIOPullUpDownModeEnum pullUpDownMode, InterruptMode eventInterruptModeIn, int evPriorityIn, int evCodeIn) : base(gpioIn)
        {
            EventInterruptMode = eventInterruptModeIn;
            evCode = evCodeIn;
            interruptEventPriority = evPriorityIn;
            // setup for event detection
            SetEventMasks();
            // open the port and turn on event detection
            OpenPort();
            // set the pullUpDown mode
            MMDevMem.SetGPIOPullUpDownMode(GpioCfgObject, pullUpDownMode);
            // set up the interrupts
            MMDevMem.DisableAllInterruptsAtGPIOLevel(GpioCfgObject);
            MMDevMem.EnableInterruptsAtGPIOLevelAccordingToEnum(GpioCfgObject, EventInterruptMode);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the event masks from the ports GpioCfgObject
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        private void SetEventMasks()
        {
            // set this flag, we set them pre-set for speed during event detection
            InterruptMask = GpioCfgObject.GpioMask;
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
        /// Gets/Sets the event priority. For InterruptPorts on the same bank,
        /// the InterruptPort with the higher priority will be activated in
        /// preference to the others if there simultaneous events
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public int InterruptEventPriority
        {
            get
            {
                return interruptEventPriority;
            }
            set
            {
                interruptEventPriority = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the event code. This is a userdefined value specified when 
        /// the interrupt port was created. 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public int EvCode
        {
            get
            {
                return evCode;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the count of events received
        /// 
        /// NOTE: events triggered and lost because the processing of another event
        ///   took too long are not included in this count. 
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public int InterruptEventsReceived
        {
            get
            {
                return interruptEventsReceived;
            }
            set
            {
                interruptEventsReceived = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the cound of events sent. It is possible to receive an event
        /// and not send it if the port is not enabled when the event is received.
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public int InterruptEventsSent
        {
            get
            {
                return interruptEventsSent;
            }
            set
            {
                interruptEventsSent = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the events interrupt mode
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public InterruptMode EventInterruptMode
        {
            get
            {
                return eventInterruptMode;
            }
            set
            {
                eventInterruptMode = value;
            }
        }                      

        // #########################################################################
        // ### Event Manipulation Code
        // #########################################################################
        #region Event Manipulation Code

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Opens the port. Throws an exception on failure.
        /// 
        /// This is really just starting a worker thread which does all the work
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        protected override void OpenPort()
        {
            // perform base operations
            base.OpenPort();
            // make sure that we are looking for events on the 
            // GPIO bank that this port belongs to
            MMDevMem.StartEventThreadForInterruptPort(this);
            // activate interrupt events for this port 
            MMDevMem.ActivateInterrupt(this);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Closes the port. Throws an exception on failure
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public override void ClosePort()
        {
            OnInterrupt = null;
            interruptIsEnabled = false;
            interruptClearToSend = false;

            // deactivate interrupt events for this port 
            MMDevMem.DeactivateInterrupt(this);

            // close in the base class too
            base.ClosePort();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Enables the interrupt
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void EnableInterrupt()
        {
            interruptIsEnabled = true;
            // start cleared to send
            ClearInterrupt();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Disables the interrupt
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void DisableInterrupt()
        {
            interruptIsEnabled = false;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Clears the interrupt. Must be called after every interrupt.
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void ClearInterrupt()
        {
            interruptClearToSend = true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// A function to send an interrupt event. This function expects to be called
        /// from within our interrupt event monitoring thread. It fills in all the
        /// required parts and sends event off to the registered subscribers.
        /// 
        /// NOTE: you are NOT in the main form thread here. You are in the thread
        ///       which processes the interrupts over in MemoryMapDevMem
        /// 
        /// NOTE: we do not need to check for the InterruptMode. This is done
        ///       in the thread worker and we would not be called if it wasn't
        ///       an event we had to send on.
        /// 
        /// </summary>
        /// <param name="evValue">The current state of the pin 0 or 1</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void SendInterruptEvent(uint evValue)
        {
            EventData evData;

            // count it
            interruptEventsReceived++;

            // some diagnostics for testing
            //   Console.WriteLine("interruptEventsReceived=" + interruptEventsReceived.ToString() +", value="+evValue.ToString());

            // send on the data - if we should
            if (OnInterrupt == null) return;
            if (interruptIsEnabled == false) return;
            if (interruptClearToSend == false) return;

            // build our event data structure
            evData = new EventData(this, evCode, evValue);

            // set flag
            interruptClearToSend=false;
            // send the data
            OnInterrupt(this.GpioID, evData.EvState, evData.EventDateTime, evData);
            // count it
            interruptEventsSent++;

        }
        #endregion

        // #########################################################################
        // ### IComparable Code
        // #########################################################################
        #region IComparable Code

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Designed to compare priorities. This makes it possible to sort a list
        /// of InterruptPorts based on priorities
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public int CompareTo(object obj)
        {
            if ((obj is InterruptPortMM) == false) return 0;

            if ((obj as InterruptPortMM).InterruptEventPriority < this.InterruptEventPriority)
            {
                return -1;
            }
            if ((obj as InterruptPortMM).InterruptEventPriority > this.InterruptEventPriority)
            {
                return 1;
            }
            // The orders are equivalent.
            return 0;
        }
        #endregion
    }
}

