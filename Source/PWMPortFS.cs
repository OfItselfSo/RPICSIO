using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
    /// Provides the Pulse Width Modulation Port functionality for the RPICSIO
    /// library. This is the SYSFS version
    /// 
    /// Be aware that you need to ensure the PWM port is configured in /boot/config.txt
    /// before this code will work. This is done by including one of the statements
    /// 
    /// dtoverlay=pwm-with-clk
    /// 
    /// or
    /// 
    /// dtoverlay=pwm-2chan-with-clk 
    /// 
    /// You also need to disable the audio (it conflicts with the PWM system) by 
    /// commenting out the line in the config.txt as shown below
    /// #dtparam=audio=on
    /// 
    /// The pwm-with-clk statement will give you PWM0 on GPIO_18 which is Pin12 on the 
    /// RPi2 Header. The pwm-2chan-with-clk will give you the same PWM0 but also PWM1
    /// which will be available on GPIO_19 which is Pin35 on the RPi2 header.
    /// 
    /// NOTE: these two overlays do not ship with the default raspbian (at this time)
    ///       you will need to download them from
    /// 
    ///   http://www.jumpnowtek.com/rpi/Using-the-Raspberry-Pi-Hardware-PWM-timers.html
    ///
    ///    without these drivers the PWM subsystem will not work. You cannot use the 
    ///    drivers that ship with the default raspbian. They do not enable an internal
    ///    clock.
    /// 
    ///
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public class PWMPortFS : PortFS
    {

        // the PWM port we use
        private PWMPortEnum pwmPort = PWMPortEnum.PWM_NONE;      

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pwmPortIn">The PWM port we use</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public PWMPortFS(PWMPortEnum pwmPortIn) : base(GpioEnum.GPIO_NONE)
        {
            pwmPort = pwmPortIn;
            //Console.WriteLine("PWMPort Starts");
            // open the port
            OpenPort();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the period of the PWM output square wave form
        /// 
        /// </summary>
        /// <value>the period (1/Freq) in nano seconds</value>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public uint PeriodNS
        { 
            get
            {
                if(portIsOpen != true) throw new Exception("Port "+ PWMPort.ToString() + " is not open");

                // read the contents of the file
                string pwmFileName = GetFullPathToPWMFile(PWMPort, RPIDefinitions.PWM_FILENAME_PERIOD);
                //string pwmFileName = RPIDefinitions.PWM_FILENAME_PERIOD.Replace("%port%", ConvertPWMPortEnumToExportNumber(PWMPort).ToString());
                string outStr = System.IO.File.ReadAllText(pwmFileName);

                // return the contents as a UINT
                return Convert.ToUInt32(outStr);
            }
            set
            {
                //Console.WriteLine("Set PeriodNS : " + value.ToString());
                if(portIsOpen != true) throw new Exception("Port "+ PWMPort.ToString() + " is not open");

                // set the period
                //  string pwmFileName = RPIDefinitions.PWM_FILENAME_PERIOD.Replace("%port%", ConvertPWMPortEnumToExportNumber(PWMPort).ToString());
                string pwmFileName = GetFullPathToPWMFile(PWMPort, RPIDefinitions.PWM_FILENAME_PERIOD);
                System.IO.File.WriteAllText(pwmFileName, value.ToString());
            }
        }
       
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the frequency of the PWM output square wave form
        /// 
        /// </summary>
        /// <value>frequency in Hz</value>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public uint FrequencyHz
        { 
            get
            {
                UInt64 freqInHz = 1000000000/PeriodNS;
                return (uint)freqInHz;
            }
            set
            {
                UInt64 periodInNs = 1000000000/value;
                PeriodNS = (uint)periodInNs;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the duty cycle of the PWM output square wave form as a percent
        /// 
        /// </summary>
        /// <value>percentage of the input value. Must be between 0 and 100 </value>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public double DutyPercent
        { 
            get
            {
                return (( float)DutyNS/(float)PeriodNS)*100;
            }
            set
            {
                DutyNS = (uint)((float)PeriodNS*(value/100));
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the duty cycle of the PWM output square wave form in nanoseconds
        /// 
        /// NOTE: the duty cycle is the part of the wave form devoted to a high state
        /// (if polarity is PWM_POLARITY_NORMAL) or the percent of the wave form devoted to
        /// the low state (if polarity is PWM_POLARITY_INVERTED)
        /// 
        /// </summary>
        /// <value>the duty cycle of the PWM output in nanoseconds. This should always
        /// be less than the PeriodNS</value>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public uint DutyNS
        { 
            get
            {
                if(portIsOpen != true) throw new Exception("Port "+ PWMPort.ToString() + " is not open");

                // read the contents of the file
                //string pwmFileName = RPIDefinitions.PWM_FILENAME_DUTY.Replace("%port%", ConvertPWMPortEnumToExportNumber(PWMPort).ToString());
                string pwmFileName = GetFullPathToPWMFile(PWMPort, RPIDefinitions.PWM_FILENAME_DUTY);
                string outStr = System.IO.File.ReadAllText(pwmFileName);

                // return the contents as a UINT
                return Convert.ToUInt32(outStr);
            }
            set
            {
                //Console.WriteLine("Set DutyNS : " + value.ToString());
                if(portIsOpen != true) throw new Exception("Port "+ PWMPort.ToString() + " is not open");

                // set the duty
                //  string pwmFileName = RPIDefinitions.PWM_FILENAME_DUTY.Replace("%port%", ConvertPWMPortEnumToExportNumber(PWMPort).ToString());
                string pwmFileName = GetFullPathToPWMFile(PWMPort, RPIDefinitions.PWM_FILENAME_DUTY);
                System.IO.File.WriteAllText(pwmFileName, value.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the run state of the PWM output square wave
        /// 
        /// </summary>
        /// <value>true - begin running, false - stop running</value>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public bool RunState
        { 
            get
            {
                if(portIsOpen != true) throw new Exception("Port "+ PWMPort.ToString() + " is not open");

                // read the contents of the file
                //string pwmFileName = RPIDefinitions.PWM_FILENAME_RUN.Replace("%port%", ConvertPWMPortEnumToExportNumber(PWMPort).ToString());
                string pwmFileName = GetFullPathToPWMFile(PWMPort, RPIDefinitions.PWM_FILENAME_RUN);
                string outStr = System.IO.File.ReadAllText(pwmFileName);
                //Console.WriteLine("Get Run State : outStr=" + outStr);

                // return the contents as a UINT
                if (outStr.Trim() == "1") return true;
                else return false;
            }
            set
            {
                // Console.WriteLine("Set Run State : " + value.ToString());
                if(portIsOpen != true) throw new Exception("Port "+ PWMPort.ToString() + " is not open");

                // set the run state
                string outVal = "0";
                if (value == true) outVal = "1";
                //      string pwmFileName = RPIDefinitions.PWM_FILENAME_RUN.Replace("%port%", ConvertPWMPortEnumToExportNumber(PWMPort).ToString());
                string pwmFileName = GetFullPathToPWMFile(PWMPort, RPIDefinitions.PWM_FILENAME_RUN);
                System.IO.File.WriteAllText(pwmFileName, outVal);
            }
        }            

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Opens the port. Throws an exception on failure
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        protected override void OpenPort()
        { 
            // compose the name of the export file
            string pwmFile = GetFullPathToPWMFile(PWMPort, RPIDefinitions.PWM_FILENAME_EXPORT);

            Console.WriteLine("PWMPort Port Opening: "+ pwmFile  + ", " + ConvertPWMPortEnumToExportNumber(PWMPort).ToString());
                  
            // do the export
            System.IO.File.WriteAllText(pwmFile, ConvertPWMPortEnumToExportNumber(PWMPort).ToString());
            portIsOpen = true;

            // the act of writing the PortExportNumber to the export file
            // will create a directory /sys/class/pwm/pwmchip<PortChipNumber>/pwm<exportNumber> 
            // This directory contains files which we can use to enable the PWM
            // and set the pulse widths etc

            //Console.WriteLine("PWMPort Port Opened: "+ PWMPort.ToString());
        }


        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Closes the port. 
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public override void ClosePort()
        {
            // do the unexport
            string pwmFileName = GetFullPathToPWMFile(PWMPort, RPIDefinitions.PWM_FILENAME_UNEXPORT);
            System.IO.File.WriteAllText(pwmFileName, ConvertPWMPortEnumToExportNumber(PWMPort).ToString());
            portIsOpen = false;

            // the act of writing the PortExportNumber to the unexport file
            // will remove the directory /sys/class/pwm/pwmchip<PortChipNumber> 
    
            //Console.WriteLine("PWMPort Port Closed: "+ PWMPort.ToString());

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the PWM Port. There is no Set accessor this is set in the constructor
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public PWMPortEnum PWMPort
        {
            get
            {
                return pwmPort;
            }
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
            return FSPortDirectionEnum.PORTDIR_OUTPUT;
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
        /// Gets the a fully qualified path for the FS files controlling the pwm
        /// </summary>
        /// <param name="subFileName">The name of the sub file</param>
        /// <param name="pwmPortIn">The PWM port we use</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        private string GetFullPathToPWMFile(PWMPortEnum pwmPortIn, string subFileName)
        {
            StringBuilder chipFileName = new StringBuilder();
            chipFileName.Append(RPIDefinitions.PWM_CHIPFILENAME_BASE);
            if ((subFileName != null) && (subFileName.Length > 0))
            {
                chipFileName.Append(@"/");
                chipFileName.Append(subFileName);               
            }
            chipFileName.Replace("%chip%", ConvertPWMPortEnumToChipNumber(pwmPortIn).ToString());
            chipFileName.Replace("%port%", ConvertPWMPortEnumToExportNumber(pwmPortIn).ToString());
            return chipFileName.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The sysfs system requires a number to be used in order to 
        /// locate the appropriate pwmchip? file placed there by the
        /// device driver
        /// </summary>
        /// <param name="pwmPortIn">The PWM port we use</param>
        /// <returns>the chip number</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public uint ConvertPWMPortEnumToChipNumber(PWMPortEnum pwmPortIn)
        {
            // the drivers on the RPi2 only seem to support pwmchip0
            // so we always return 0 here.
            return 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// The sysfs system requires a number to be used in order to 
        /// export and manipulate the PWM device we wish to use. This function 
        /// converts the PWMPortEnum value to that number
        /// </summary>
        /// <param name="pwmPortIn">The PWM port we use</param>
        /// <returns>the export number</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public uint ConvertPWMPortEnumToExportNumber(PWMPortEnum pwmPortIn)
        {
            if (pwmPortIn == PWMPortEnum.PWM_0) return 0; 
            if (pwmPortIn == PWMPortEnum.PWM_1) return 1; 
            throw new Exception("Unknown PWM Port: "+ pwmPortIn.ToString());
        }

        // #########################################################################
        // ### Dispose Code
        // #########################################################################
        #region

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Implement IDisposable. 
        /// Dispose(bool disposing) executes in two distinct scenarios. 
        /// 
        ///    If disposing equals true, the method has been called directly 
        ///    or indirectly by a user's code. Managed and unmanaged resources 
        ///    can be disposed.
        ///  
        ///    If disposing equals false, the method has been called by the 
        ///    runtime from inside the finalizer and you should not reference 
        ///    other objects. Only unmanaged resources can be disposed. 
        /// 
        ///  see: http://msdn.microsoft.com/en-us/library/system.idisposable.dispose%28v=vs.110%29.aspx
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if(Disposed==false)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if(disposing==true)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. If disposing is false, 
                // only the following code is executed.

                // Clean up our code
                //  Console.WriteLine("Disposing PWMPORT");
         
                // call the base to dispose there
                base.Dispose(disposing);

            }
        }
        #endregion

    }
}

