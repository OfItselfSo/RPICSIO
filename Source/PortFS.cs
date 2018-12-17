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
    /// Provides the base functionality for FileSystem ports
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public abstract class PortFS :  Port
    {
           
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gpioIDIn">The gpio we open the port on</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public PortFS (GpioEnum gpioIDIn) : base(gpioIDIn)
        {

        }
            
        // #########################################################################
        // ### Port Manipulation Code
        // #########################################################################
        #region

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Opens the port. Throws an exception on failure, including if the port is
        /// already open. 
        /// 
        /// This is really just doing the equivalent of a shell command 
        ///    echo <gpioID> > /sys/class/gpio/export
        ///  after which the /sys/class/gpio/gpio<gpioID> directory should exist.
        ///  If it already exists the port is in use by someone else
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        protected override void OpenPort()
        {
            // some tests
            if (GpioID == GpioEnum.GPIO_NONE) 
            {
                throw new Exception ("Cannot open port. Invalid port: " + GpioID.ToString ());
            }
            // do the open
            System.IO.File.WriteAllText(RPIDefinitions.SYSFS_GPIODIR+RPIDefinitions.SYSFS_GPIOEXPORT, GpioUtils.GpioIDToString(GpioID));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Closes the port. Throws an exception on failure, including if the port is
        /// already closed
        /// 
        /// This is really just doing the equivalent of a shell command 
        ///    echo <gpioID> > /sys/class/gpio/unexport
        ///  after which the /sys/class/gpio/gpio<gpioID> directory should not exist.
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public override void ClosePort()
        {
            // do the close
            System.IO.File.WriteAllText(RPIDefinitions.SYSFS_GPIODIR+RPIDefinitions.SYSFS_GPIOUNEXPORT, GpioUtils.GpioIDToString(GpioID));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the PortDirection - derived classes must implement
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public abstract FSPortDirectionEnum PortDirection();

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the port direction at the SysFs level
        /// 
        /// This is really just doing the equivalent of a shell command 
        ///    echo <direction_as_string> > /sys/class/gpio/gpio<gpioID>/direction
        /// 
        /// </summary>
        /// <param name="portDirIn">the port direction INPUT/OUTPUT</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        protected void SetSysFsDirection()
        {
            string dirStr;

            if (PortDirection() == FSPortDirectionEnum.PORTDIR_INPUT)
            {
                dirStr = "in";
            } 
            else if (PortDirection() == FSPortDirectionEnum.PORTDIR_OUTPUT)
            {
                dirStr = "out";
            }
            else if (PortDirection() == FSPortDirectionEnum.PORTDIR_INPUTOUTPUT)
            {
                // handled elsewhere
                return;
            }
            else
            {
                // should never happen
                throw new Exception ("unknown port direction:" + PortDirection().ToString ());
            }
            // set the direction now
            System.IO.File.WriteAllText(RPIDefinitions.SYSFS_GPIODIR+RPIDefinitions.SYSFS_GPIODIRNAMEBASE+GpioUtils.GpioIDToString(GpioID)+"/"+RPIDefinitions.SYSFS_GPIODIRECTION, dirStr);
        }
                   
        #endregion

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

                // Clean up our port        
                ClosePort();

                // call the base to dispose there
                base.Dispose(disposing);

            }
        }
        #endregion

    }
}

