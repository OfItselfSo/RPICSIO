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
    /// Provides a container to transport information on an open Slave Device
    /// around the system. 
    /// 
    /// NOTE:
    ///   The RPI SPI ports only have two slave select lines both CS0 and CS1 
    ///   available on the header. The upshot is that and you will only have
    ///   two built in slave select (CS0, CS1) lines available to you. 
    /// 
    ///   This means of you have more than two SPI devices you need to use 
    ///   regular GPIO outputs to implement the Slave Select lines for them.
    ///   The SPI ports cope with this and manage it all internally and a 
    ///   slave device configured on a GPIO can be used in exactly the same 
    ///   way as the built-in slave select lines.
    /// 
    ///   However, in the RPICSIO library you cannot mix-and-match. Either
    ///   you use ALL GPIO based slave select lines or you use only the 
    ///   built in CS0 and/or CS1 lines.
    /// 
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public class SPISlaveDeviceHandle
    {
        // the slave device
        private SPISlaveDeviceEnum spiSlaveDevice = SPISlaveDeviceEnum.SPI_SLAVEDEVICE_NONE;

        // the file descriptor of the slave device. Only meaningful when the SlaveDevice
        /// is SPISlaveDeviceEnum.SPI_SLAVEDEVICE_CS*
        int spiDevFileDescriptor = -1;

        // GPIO output port for the slave device. Only meaningful when the SlaveDevice
        /// is SPISlaveDeviceEnum.SPI_SLAVEDEVICE_GPIO
        private OutputPortMM gpioSlaveSelect=null;

        // the settings below get used on a slave by slave basis and can be different between 
        // slave devices on the same SPI port 

        // Temporary override of the device's bitrate for a specific slave
        private uint speedInHz = 0;
        // If nonzero, how long to delay after the last bit transfer
        // before optionally deselecting the device before the next transfer.
        private UInt16 delay_usecs = 0;
        // Temporary override of the device's default wordsize
        private byte bits_per_word = 0;
        // nz to deselect device before starting the next transfer.
        private byte cs_change = 0;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SPISlaveDeviceHandle()
        {
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spiSlaveDeviceIn">The SPI slave device</param>
        /// <param name="spiDevFileDescriptor">The SPI file descriptor</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SPISlaveDeviceHandle(SPISlaveDeviceEnum spiSlaveDeviceIn, int spiDevFileDescriptorIn)
        {
            spiSlaveDevice = spiSlaveDeviceIn;
            spiDevFileDescriptor = spiDevFileDescriptorIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spiSlaveDeviceIn">The SPI slave device</param>
        /// <param name="gpioSlaveSelectIn">The GPIO output port acting as the slave select line</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SPISlaveDeviceHandle(SPISlaveDeviceEnum spiSlaveDeviceIn, OutputPortMM gpioSlaveSelectIn)
        {
            spiSlaveDevice = spiSlaveDeviceIn;
            gpioSlaveSelect = gpioSlaveSelectIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Resets the object to the default state
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void Reset()
        {
            spiSlaveDevice = SPISlaveDeviceEnum.SPI_SLAVEDEVICE_NONE;
            spiDevFileDescriptor = -1;
            GpioSlaveSelect = null;
            speedInHz = 0;
            delay_usecs = 0;
            bits_per_word = 0;
            cs_change = 0;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the spiDevFileDescriptor. Only meaningful when the SlaveDevice
        /// is SPISlaveDeviceEnum.SPI_SLAVEDEVICE_CS*
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public int SpiDevFileDescriptor
        {
            get
            {
                return spiDevFileDescriptor;
            }
            set
            {
                spiDevFileDescriptor = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the port which acts as the GPIO based slave select. Only 
        /// meaningful when the SlaveDevice is SPISlaveDeviceEnum.SPI_SLAVEDEVICE_GPIO
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public OutputPortMM GpioSlaveSelect
        {
            get
            {
                return gpioSlaveSelect;
            }
            set
            {
                gpioSlaveSelect = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the spiSlaveDevice. 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SPISlaveDeviceEnum SPISlaveDevice
        {
            get
            {
                return spiSlaveDevice;
            }
            set
            {
                spiSlaveDevice = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the speedInHz for this slave device. Overrides the SPI port 
        /// defaults. 
        /// 
        /// Important Note: In SPIPortFS mode, setting the speed on the slave 
        /// device dramatically slows down the overall speed. The SPIDEV driver 
        /// seems to conduct very time consumingoperations to set this for each 
        /// transmission. It is far, far more efficient to use the SetDefaultSpeedInHz 
        /// call on the SPI port to set this once - unless you really need to 
        /// override the port speed for a specific slave device.
        ///
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public uint SpeedInHz
        {
            get
            {
                return speedInHz;
            }
            set
            {
                speedInHz = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the delay_usecs for this slave device. Overrides the SPI port 
        /// defaults. If nonzero, indicates how long to delay after the last bit 
        /// transfer before optionally deselecting the device before the next transfer.
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public UInt16 DelayUSecs
        {
            get
            {
                return delay_usecs;
            }
            set
            {
                delay_usecs = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the bits_per_word for this slave device. Overrides the SPI port 
        /// defaults
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public byte BitsPerWord
        {
            get
            {
                return bits_per_word;
            }
            set
            {
                bits_per_word = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Get/Set the cs change flag for this slave device. Overrides the SPI port 
        /// defaults. If nz device will be deselected  and reselected before 
        /// starting the next transfer.
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public byte CSChange
        {
            get
            {
                return cs_change;
            }
            set
            {
                cs_change = value;
            }
        }
    }
}

