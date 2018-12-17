using System;
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
    /// Provides the Serial Port functionality for a Raspberry Pi 2
    /// This is the SYSFS version
    /// 
    /// Be aware that you need to eonsole serial port driver otherwise the 
    /// RPICSIO system calls do not work. Do this by issuing the commands
    /// below (as root) 
    /// 
    ///    sudo systemctl stop serial-getty@ttyAMA0.service
    ///    sudo systemctl disable serial-getty@ttyAMA0.service
    ///
    /// MEGA IMPORTANT NOTE: The pins (including the serial port pins) on the 
    ///   Raspberry Pi are 3.3Volt. You CANNOT connect them up to a 5V
    ///   serial device without level shifting the voltage or you will burn out
    ///   the RPI. You DEFINITELY CANNOT connect them to the +/- 18V of a true
    ///   RS232 serial port (on the back of a PC) because your RPI will instantly 
    ///   die.
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public class SerialPortFS : PortFS
    {

        // the Serial port we use
        private SerialPortEnum serialPort = SerialPortEnum.UART_NONE;

        // used for external file open calls
        private const int O_RDONLY = 0x0;
        private const int O_WRONLY = 0x1;
        private const int O_RDWR = 0x2;
        private const int O_SYNC = 0x101000;
        private const int O_NONBLOCK = 0x800;
        private const int O_NOCTTY = 0x100;

        // the file descriptor for the open serial port
        int serialPortFD = -1;

        // these magic numbers are defined by the ioctl driver 
        // to tell it what to do. Each byte and nibble has meaning and it is generally
        // built on the fly in a C program by a macro which shifts flags about and OR's
        // them together. I have not attempted to reproduce this build here and simply
        // use the end result since the resulting value is essentially constant for
        // an particular ioctl call to a specific driver type
        public const uint TCGETS = 0x5401;
        public const uint TCSETS = 0x5402;
        public const uint TCFLSH = 0x540b;
        public const uint FIONREAD = 0x541b;
        public const uint TCSBRK = 0x5409;

        // misc termios flags
        public const uint CBAUD = 4111;
        public const uint CBAUDEX = 4096;

        // baud rate termios flags
        public const int B0 = 0;
        public const int B50 = 1;
        public const int B75 = 2;
        public const int B110 = 3;
        public const int B134 = 4;
        public const int B150 = 5;
        public const int B200 = 6;
        public const int B300 = 7;
        public const int B600 = 8;
        public const int B1200 = 9;
        public const int B1800 = 10;
        public const int B2400 = 11;
        public const int B4800 = 12;
        public const int B9600 = 13;
        public const int B19200 = 14;
        public const int B38400 = 15;
        public const int B57600 = 4097;
        public const int B115200 = 4098;
        public const int B230400 = 4099;
        public const int B460800 = 4100;
 
        // bit length termios flags
        public const uint CS5 = 0;
        public const uint CS6 = 16;
        public const uint CS7 = 32;
        public const uint CS8 = 48;

        // stop bit termios flag
        public const uint CSTOPB = 64;

        // parity termios flags
        public const uint PARENB = 256;
        public const uint PARODD = 512;

        // UART termios constants not used below but which you may wish to use
        // in conjunction with GetTermiosState() and SetTermiosState()
        public const int BRKINT = 2;
        public const int BS0 = 0;
        public const int BS1 = 8192;
        public const int BSDLY = 8192;
        public const int CDSUSP = 25;
        public const int CEOF = 4;
        public const int CEOL = 0;
        public const int CEOT = 4;
        public const int CERASE = 127;
        public const int CFLUSH = 15;
        public const int CIBAUD = 269418496;
        public const int CINTR = 3;
        public const int CKILL = 21;
        public const int CLNEXT = 22;
        public const int CLOCAL = 2048;
        public const int CQUIT = 28;
        public const int CR0 = 0;
        public const int CR1 = 512;
        public const int CR2 = 1024;
        public const int CR3 = 1536;
        public const int CRDLY = 1536;
        public const int CREAD = 128;
        public const int CRPRNT = 18;
        public const int CRTSCTS = -2147483648;
        public const int CSIZE = 48;
        public const int CSTART = 17;
        public const int CSTOP = 19;
        public const int CSUSP = 26;
        public const int CWERASE = 23;
        public const int ECHO = 8;
        public const int ECHOCTL = 512;
        public const int ECHOE = 16;
        public const int ECHOK = 32;
        public const int ECHOKE = 2048;
        public const int ECHONL = 64;
        public const int ECHOPRT = 1024;
        public const int EXTA = 14;
        public const int EXTB = 15;
        public const int FF0 = 0;
        public const int FF1 = 32768;
        public const int FFDLY = 32768;
        public const int FIOASYNC = 21586;
        public const int FIOCLEX = 21585;
        public const int FIONBIO = 21537;
        public const int FIONCLEX = 21584;
        public const int FLUSHO = 4096;
        public const int HUPCL = 1024;
        public const int ICANON = 2;
        public const int ICRNL = 256;
        public const int IEXTEN = 32768;
        public const int IGNBRK = 1;
        public const int IGNCR = 128;
        public const int IGNPAR = 4;
        public const int IMAXBEL = 8192;
        public const int INLCR = 64;
        public const int INPCK = 16;
        public const int IOCSIZE_MASK = 1073676288;
        public const int IOCSIZE_SHIFT = 16;
        public const int ISIG = 1;
        public const int ISTRIP = 32;
        public const int IUCLC = 512;
        public const int IXANY = 2048;
        public const int IXOFF = 4096;
        public const int IXON = 1024;
        public const int NCC = 8;
        public const int NCCS = 32;
        public const int NL0 = 0;
        public const int NL1 = 256;
        public const int NLDLY = 256;
        public const int NOFLSH = 128;
        public const int N_MOUSE = 2;
        public const int N_PPP = 3;
        public const int N_SLIP = 1;
        public const int N_STRIP = 4;
        public const int N_TTY = 0;
        public const int OCRNL = 8;
        public const int OFDEL = 128;
        public const int OFILL = 64;
        public const int OLCUC = 2;
        public const int ONLCR = 4;
        public const int ONLRET = 32;
        public const int ONOCR = 16;
        public const int OPOST = 1;
        public const int PARMRK = 8;
        public const int PENDIN = 16384;
        public const int TAB0 = 0;
        public const int TAB1 = 2048;
        public const int TAB2 = 4096;
        public const int TAB3 = 6144;
        public const int TABDLY = 6144;
        public const int TCGETA = 21509;
        public const int TCIFLUSH = 0;
        public const int TCIOFF = 2;
        public const int TCIOFLUSH = 2;
        public const int TCION = 3;
        public const int TCOFLUSH = 1;
        public const int TCOOFF = 0;
        public const int TCOON = 1;
        public const int TCSADRAIN = 1;
        public const int TCSAFLUSH = 2;
        public const int TCSANOW = 0;
        public const int TCSBRKP = 21541;
        public const int TCSETA = 21510;
        public const int TCSETAF = 21512;
        public const int TCSETAW = 21511;
        public const int TCSETSF = 21508;
        public const int TCSETSW = 21507;
        public const int TCXONC = 21514;
        public const int TIOCCONS = 21533;
        public const int TIOCEXCL = 21516;
        public const int TIOCGETD = 21540;
        public const int TIOCGICOUNT = 21597;
        public const int TIOCGLCKTRMIOS = 21590;
        public const int TIOCGPGRP = 21519;
        public const int TIOCGSERIAL = 21534;
        public const int TIOCGSOFTCAR = 21529;
        public const int TIOCGWINSZ = 21523;
        public const int TIOCINQ = 21531;
        public const int TIOCLINUX = 21532;
        public const int TIOCMBIC = 21527;
        public const int TIOCMBIS = 21526;
        public const int TIOCMGET = 21525;
        public const int TIOCMIWAIT = 21596;
        public const int TIOCMSET = 21528;
        public const int TIOCM_CAR = 64;
        public const int TIOCM_CD = 64;
        public const int TIOCM_CTS = 32;
        public const int TIOCM_DSR = 256;
        public const int TIOCM_DTR = 2;
        public const int TIOCM_LE = 1;
        public const int TIOCM_RI = 128;
        public const int TIOCM_RNG = 128;
        public const int TIOCM_RTS = 4;
        public const int TIOCM_SR = 16;
        public const int TIOCM_ST = 8;
        public const int TIOCNOTTY = 21538;
        public const int TIOCNXCL = 21517;
        public const int TIOCOUTQ = 21521;
        public const int TIOCPKT = 21536;
        public const int TIOCPKT_DATA = 0;
        public const int TIOCPKT_DOSTOP = 32;
        public const int TIOCPKT_FLUSHREAD = 1;
        public const int TIOCPKT_FLUSHWRITE = 2;
        public const int TIOCPKT_NOSTOP = 16;
        public const int TIOCPKT_START = 8;
        public const int TIOCPKT_STOP = 4;
        public const int TIOCSCTTY = 21518;
        public const int TIOCSERCONFIG = 21587;
        public const int TIOCSERGETLSR = 21593;
        public const int TIOCSERGETMULTI = 21594;
        public const int TIOCSERGSTRUCT = 21592;
        public const int TIOCSERGWILD = 21588;
        public const int TIOCSERSETMULTI = 21595;
        public const int TIOCSERSWILD = 21589;
        public const int TIOCSER_TEMT = 1;
        public const int TIOCSETD = 21539;
        public const int TIOCSLCKTRMIOS = 21591;
        public const int TIOCSPGRP = 21520;
        public const int TIOCSSERIAL = 21535;
        public const int TIOCSSOFTCAR = 21530;
        public const int TIOCSTI = 21522;
        public const int TIOCSWINSZ = 21524;
        public const int TOSTOP = 256;
        public const int VDISCARD = 13;
        public const int VEOF = 4;
        public const int VEOL = 11;
        public const int VEOL2 = 16;
        public const int VERASE = 2;
        public const int VINTR = 0;
        public const int VKILL = 3;
        public const int VLNEXT = 15;
        public const int VMIN = 6;
        public const int VQUIT = 1;
        public const int VREPRINT = 12;
        public const int VSTART = 8;
        public const int VSTOP = 9;
        public const int VSUSP = 10;
        public const int VSWTC = 7;
        public const int VSWTCH = 7;
        public const int VT0 = 0;
        public const int VT1 = 16384;
        public const int VTDLY = 16384;
        public const int VTIME = 5;
        public const int VWERASE = 14;
        public const int XCASE = 4;
        public const int XTABS = 6144;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serialPortIn">The Serial port we use</param>
        /// <param name="openMode">the read mode (BLOCK or NONBLOCK) in which we 
        /// open the serial port.</param>
        /// 
        /// NOTE: the blocking mode refers to to the way the serial port code deals
        /// with a request to read more data than is present in the input queue. If
        /// the port is in OPEN_BLOCK mode and more data is requested than is present
        /// then the call will wait until the required amount of data arrives. In
        /// OPEN_NONBLOCK mode the port will just return what it has.
        /// 
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SerialPortFS(SerialPortEnum serialPortIn, SerialPortOpenModeEnum openMode) : base(GpioEnum.GPIO_NONE)
        {
            serialPort = serialPortIn;
            // Console.WriteLine("SerialPort Starts");
            // open the port
            OpenPort(openMode);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Writes a byte array out to an Serial Device. 
        /// 
        /// </summary>
        /// <param name="txByteBuf">The byte array buffer with bytes to write</param>
        /// <param name="numBytes">The number of bytes to send
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void Write(byte[] txByteBuf, int numBytes)
        {
            // sanity check
            if (txByteBuf == null)
            {
                throw new Exception ("Null tx buffer");
            }
            if (numBytes <= 0)
            {
                throw new Exception ("numBytes <= 0");
            }
            if (serialPortFD <= 0)
            {
                throw new Exception ("Serial port is not open, fd=0");
            }
            if (PortIsOpen == false)
            {
                throw new Exception ("Serial port is not open");
            }
     
            // the write is an external write to the device file
            int numWritten = ExternalWrite(serialPortFD, txByteBuf, numBytes);
            if (numWritten != numBytes)
            {
                throw new Exception ("Error writing to Serial device " + SerialPort.ToString() + ", numWritten != numBytes" + numWritten.ToString() + " != " + numBytes.ToString());
            }
        }
       
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Writes a string out to an Serial Device as a series of ASCII bytes. 
        /// 
        /// </summary>
        /// <param name="outStr">The string to write</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void Write(string outStr)
        {
            if (outStr == null)
            {
                throw new Exception("NULL string on Write() to serial port");
            }
            // convert the string to an ASCII byte array
            byte[] outData = ASCIIEncoding.ASCII.GetBytes(outStr);
            // write it out
            Write(outData, outData.Length);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Reads a buffer in from an Serial Device. We choose to structure this so that
        /// the caller passes in the byte array. This allows it to be re-used. If we 
        /// just create and return one here then C# will have to deal with a lot of 
        /// garbage collection (potentially) on very busy calls which would slow things
        /// down.
        /// 
        /// </summary>
        /// <param name="rxByteBuf">The buffer in which we store the bytes read</param>
        /// <param name="numBytes">The max number of bytes to read</param>
        /// <returns>The number of bytes read</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public int ReadByteArray(byte[] rxByteBuf, int numBytes)
        {
            // sanity check
            if (rxByteBuf == null)
            {
                throw new Exception ("Null rx buffer");
            }
            if (numBytes <= 0)
            {
                throw new Exception ("numBytes <= 0");
            }
            if (serialPortFD <= 0)
            {
                throw new Exception ("Serial port is not open, fd=0");
            }
            if (PortIsOpen == false)
            {
                throw new Exception ("Serial port is not open");
            }

            // the read is an external read to the device file. IF the port is opened
            // in OPEN_BLOCK mode this will block and wait if there is no data in the queue
            return ExternalRead(serialPortFD, rxByteBuf, numBytes);

        }
            
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Reads a Serial Device and returns the contents as a string. This is
        /// not especially efficient in terms of memory use as both the return
        /// string and an intermediate byte[] array need to be created and
        /// subsequently garbage collected
        /// 
        /// <returns>a string with the contents of the serial port receive
        /// buffer up to the specified maxBytes. If nothing is ready to be
        /// read on the serial port an empty string will be returned.
        /// </returns>
        /// 
        /// </summary>
        /// <param name="maxBytes">The maximum number of bytes to read.</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public string ReadString(int maxBytes)
        {
            int bytesToRead = 0;
            byte[] rxByteBuf = null;

            if (serialPortFD <= 0)
            {
                throw new Exception ("Serial port is not open, fd=0");
            }
            if (PortIsOpen == false)
            {
                throw new Exception ("Serial port is not open");
            }

            // get the number of bytes
            int rxBytes = BytesInRxBuffer;
            // if there is nothing there just return empty (never a null)
            if (rxBytes <= 0) return "";
            // cap the number of bytes to read at our maximum
            if (rxBytes > maxBytes) bytesToRead = maxBytes;
            else bytesToRead = rxBytes;

            // create the buffer
            rxByteBuf = new byte[bytesToRead];
            // read the data
            int retInt = ReadByteArray(rxByteBuf, bytesToRead);
            if (retInt <= 0) return "";
            // convert the byte to a string and return it
            string outStr = System.Text.Encoding.Default.GetString(rxByteBuf);
            if (outStr == null) return "";
            return outStr;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts the Termios bit flags to a baudrate
        /// </summary>
        /// <param name="baudBits">The termios baud bits flag</param>
        /// <returns>the baud rate or SerialPortBaudRateEnum.BAUDRATE_0 for fail</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        private SerialPortBaudRateEnum ConvertBaudBitsToRate(uint baudBits)
        {
            if(baudBits==B0) return SerialPortBaudRateEnum.BAUDRATE_0;
            else if(baudBits==B50) return SerialPortBaudRateEnum.BAUDRATE_50;
            else if(baudBits==B75) return SerialPortBaudRateEnum.BAUDRATE_75;
            else if(baudBits==B110) return SerialPortBaudRateEnum.BAUDRATE_110;
            else if(baudBits==B134) return SerialPortBaudRateEnum.BAUDRATE_134;
            else if(baudBits==B150) return SerialPortBaudRateEnum.BAUDRATE_150;
            else if(baudBits==B200) return SerialPortBaudRateEnum.BAUDRATE_200;
            else if(baudBits==B300) return SerialPortBaudRateEnum.BAUDRATE_300;
            else if(baudBits==B600) return SerialPortBaudRateEnum.BAUDRATE_600;
            else if(baudBits==B1200) return SerialPortBaudRateEnum.BAUDRATE_1200;
            else if(baudBits==B1800) return SerialPortBaudRateEnum.BAUDRATE_1800;
            else if(baudBits==B2400) return SerialPortBaudRateEnum.BAUDRATE_2400;
            else if(baudBits==B4800) return SerialPortBaudRateEnum.BAUDRATE_4800;
            else if(baudBits==B9600) return SerialPortBaudRateEnum.BAUDRATE_9600;
            else if(baudBits==B19200) return SerialPortBaudRateEnum.BAUDRATE_19200;
            else if(baudBits==B38400) return SerialPortBaudRateEnum.BAUDRATE_38400;
            else if(baudBits==B57600) return SerialPortBaudRateEnum.BAUDRATE_57600;
            else if(baudBits==B115200) return SerialPortBaudRateEnum.BAUDRATE_115200;
            else if(baudBits==B230400) return SerialPortBaudRateEnum.BAUDRATE_230400;
            else if(baudBits==B460800) return SerialPortBaudRateEnum.BAUDRATE_460800;
            /*
            else if(baudBits==B500000) return SerialPortBaudRateEnum.BAUDRATE_500000;
            else if(baudBits==B576000) return SerialPortBaudRateEnum.BAUDRATE_576000;
            else if(baudBits==B921600) return SerialPortBaudRateEnum.BAUDRATE_921600;
            else if(baudBits==B1000000) return SerialPortBaudRateEnum.BAUDRATE_1000000;
            else if(baudBits==B1152000) return SerialPortBaudRateEnum.BAUDRATE_1152000;
            else if(baudBits==B1500000) return SerialPortBaudRateEnum.BAUDRATE_1500000;
            else if(baudBits==B2000000) return SerialPortBaudRateEnum.BAUDRATE_2000000;
            else if(baudBits==B2500000) return SerialPortBaudRateEnum.BAUDRATE_2500000;
            else if(baudBits==B3000000) return SerialPortBaudRateEnum.BAUDRATE_3000000;
            else if(baudBits==B3500000) return SerialPortBaudRateEnum.BAUDRATE_3500000;
            else if(baudBits==B4000000) return SerialPortBaudRateEnum.BAUDRATE_4000000;
            */
            return SerialPortBaudRateEnum.BAUDRATE_0; // unknown
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts a baud rate to Termios bit flags
        /// </summary>
        /// <param name="baudRate">The baud rate. Can only be standard values</param>
        /// <returns>the termios baud bits flag or 0 for fail</returns>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        private uint ConvertBaudRateToBits(SerialPortBaudRateEnum baudRate)
        {
            if(baudRate==SerialPortBaudRateEnum.BAUDRATE_0) return B0;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_50) return B50;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_75) return B75;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_110) return B110;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_134) return B134;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_150) return B150;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_200) return B200;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_300) return B300;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_600) return B600;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_1200) return B1200;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_1800) return B1800;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_2400) return B2400;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_4800) return B4800;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_9600) return B9600;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_19200) return B19200;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_38400) return B38400;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_57600) return B57600;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_115200) return B115200;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_230400) return B230400;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_460800) return B460800;
            /*
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_500000) return B500000;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_576000) return B576000;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_921600) return B921600;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_1000000) return B1000000;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_1152000) return B1152000;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_1500000) return B1500000;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_2000000) return B2000000;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_2500000) return B2500000;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_3000000) return B3000000;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_3500000) return B3500000;
            else if(baudRate==SerialPortBaudRateEnum.BAUDRATE_4000000) return B4000000;*/
            return B0; // unknown
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the baud rate
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SerialPortBaudRateEnum BaudRate
        {
            get
            {
                int ioctlRetVal = -1;
                // create our transfer struct
                TermiosStruct xfer = new TermiosStruct();

                // get the settings from the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCGETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error reading terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }

                // this will contain a lot of other things besides our baud rate
                // the baud rate is represented by Termios flags B0-B4000000
                // get the bits
                uint baudBits = xfer.c_cflag & CBAUD;
                // return the converted value
                return ConvertBaudBitsToRate(baudBits);
            }
            set
            {
                int ioctlRetVal = -1;
                // create our transfer struct
                TermiosStruct xfer = new TermiosStruct();

                // get the settings from the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCGETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error reading terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }

                // convert the baud rate to the termios bit flags
                uint bitFlags = ConvertBaudRateToBits(value);
                if (bitFlags == 0)
                {
                    throw new Exception("Invalid baud rate:"+ value.ToString());
                }
                // clear bit flags in the struct
                xfer.c_cflag &= (~CBAUD);
                xfer.c_cflag &= (~CBAUDEX);
                // and set the new ones
                xfer.c_cflag |= bitFlags;

                // set the new speed in the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCSETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error writing terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the bit length of the bytes transmitted via the RPI Serial port
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SerialPortBitLengthEnum BitLength
        {
            get
            {
                int ioctlRetVal = -1;
                // create our transfer struct
                TermiosStruct xfer = new TermiosStruct();

                // get the settings from the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCGETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error reading terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }

                // this will contain a lot of other things besides our bit length
                uint lengthBits = xfer.c_cflag & CS8;
               
                // return the converted value
                if (lengthBits == CS8) return SerialPortBitLengthEnum.BITLENGTH_8;
                else if (lengthBits == CS7) return SerialPortBitLengthEnum.BITLENGTH_7;
                else if (lengthBits == CS6) return SerialPortBitLengthEnum.BITLENGTH_6;
                else if (lengthBits == CS5) return SerialPortBitLengthEnum.BITLENGTH_5;
                else return SerialPortBitLengthEnum.BITLENGTH_NONE;
            }
            set
            {
                int ioctlRetVal = -1;
                // create our transfer struct
                TermiosStruct xfer = new TermiosStruct();

                // get the current settings from the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCGETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error reading terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }
                    
                // convert the bit length to the termios bit flags
                uint lengthBits = CS8;
                if (value == SerialPortBitLengthEnum.BITLENGTH_8) lengthBits = CS8;
                else if (value == SerialPortBitLengthEnum.BITLENGTH_7) lengthBits = CS7;
                else if (value == SerialPortBitLengthEnum.BITLENGTH_6) lengthBits = CS6;
                else if (value == SerialPortBitLengthEnum.BITLENGTH_5) lengthBits = CS5;
                else
                {
                    throw new Exception("Unknown bit length@ " + value.ToString());
                }
                // clear length bits in the struct
                xfer.c_cflag &= (~CS8);
                // and set the new ones
                xfer.c_cflag |= lengthBits;

                // set the new bit length in the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCSETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error writing terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the parity bits of the bytes transmitted via the RPI Serial port
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SerialPortParityEnum Parity
        {
            get
            {
                int ioctlRetVal = -1;
                // create our transfer struct
                TermiosStruct xfer = new TermiosStruct();

                // get the current settings from the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCGETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error reading terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }

                // figure out what our parity bits say
                if ((xfer.c_cflag & PARENB) == 0)
                {
                    // parity not enabled?, this always means no parity
                    return SerialPortParityEnum.PARITY_NONE;
                }
                else if ((xfer.c_cflag & PARODD) == 0)
                {
                    // parity enabled, but PARODD not enabled? this means PAREVEN
                    return SerialPortParityEnum.PARITY_EVEN;
                }
                else
                {
                    // must be odd parity
                    return SerialPortParityEnum.PARITY_ODD;
                }
            }
            set
            {
                int ioctlRetVal = -1;
                // create our transfer struct
                TermiosStruct xfer = new TermiosStruct();

                // get the current settings from the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCGETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error reading terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }

                // set the appropriate termios bit flags
                if (value == SerialPortParityEnum.PARITY_NONE)
                {
                    // clear the enable bit
                    xfer.c_cflag &= (~PARENB);
                    // clear the odd/even bit
                    xfer.c_cflag &= (~PARODD);
                }
                else if (value == SerialPortParityEnum.PARITY_EVEN)
                {
                    // set the enable bit
                    xfer.c_cflag |= (PARENB);
                    // clear the odd/even bit
                    xfer.c_cflag &= (~PARODD);
                }
                else if (value == SerialPortParityEnum.PARITY_ODD)
                {
                    // set the enable bit
                    xfer.c_cflag |= (PARENB);
                    // set the odd/even bit
                    xfer.c_cflag |= (PARODD);
                }
                else
                {
                    throw new Exception("Unknown Parity" + value.ToString());
                }

                // set the new parity in the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCSETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error writing terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }
            }
        }
            
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the stop bit setting of the bytes transmitted via the RPI Serial port
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SerialPortStopBitsEnum StopBits
        {
            get
            {
                int ioctlRetVal = -1;
                // create our transfer struct
                TermiosStruct xfer = new TermiosStruct();

                // get the current settings from the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCGETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error reading terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }

                // figure out what our stop bits say
                if ((xfer.c_cflag & CSTOPB) != 0)
                {
                    // bit set, this means two stop bits
                    return SerialPortStopBitsEnum.STOPBITS_TWO;
                }
                else
                {
                    // must be one stop bit
                    return SerialPortStopBitsEnum.STOPBITS_ONE;
                }
            }
            set
            {
                int ioctlRetVal = -1;
                // create our transfer struct
                TermiosStruct xfer = new TermiosStruct();

                // get the current settings from the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCGETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error reading terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }
                    
                // set the appropriate termios bit flags
                if (value == SerialPortStopBitsEnum.STOPBITS_TWO)
                {
                    // set the two stop bits flag
                    xfer.c_cflag |= (CSTOPB);
                }
                else if (value == SerialPortStopBitsEnum.STOPBITS_ONE)
                {
                    // clear the two stop bits flag
                    xfer.c_cflag &= (~CSTOPB);
                }
                else
                {
                    throw new Exception("Unknown StopBits" + value.ToString());
                }

                // set the new parity in the uart
                ioctlRetVal = ExternalIoCtl(serialPortFD, TCSETS, ref xfer);
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error writing terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Dumps out the Termios structure to the console. For diagnostic
        /// purposes only.
        /// 
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void DumpTermiosStruct(TermiosStruct xfer)
        {
            Console.WriteLine("termios.c_iflag=0x"+xfer.c_iflag.ToString("x8"));
            Console.WriteLine("termios.c_oflag=0x"+xfer.c_oflag.ToString("x8"));
            Console.WriteLine("termios.c_cflag=0x"+xfer.c_cflag.ToString("x8"));
            Console.WriteLine("termios.c_lflag=0x"+xfer.c_lflag.ToString("x8"));
            Console.WriteLine("termios.c_line=0x"+xfer.c_line.ToString("x8"));
            Console.WriteLine("termios.c_ispeed=0x"+xfer.c_ispeed.ToString("x8"));
            Console.WriteLine("termios.c_c_ospeed=0x"+xfer.c_ospeed.ToString("x8"));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the Termios structure. The equivalent of the termios.c 
        /// tcgetattr call
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public TermiosStruct GetTermiosState()
        {
            int ioctlRetVal = -1;
            TermiosStruct xfer = new TermiosStruct();

            // get the current settings from the uart
            ioctlRetVal = ExternalIoCtl(serialPortFD, TCGETS, ref xfer);
            if (ioctlRetVal < 0)
            {
                throw new Exception ("Error reading terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
            }                    
            return xfer;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the Termios structure. The equivalent of the termios.c 
        /// tcsetattr call
        /// </summary>
        /// <param name="xfer">the termios structure to set</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void SetTermiosState(ref TermiosStruct xfer)
        {
            int ioctlRetVal = -1;
            // set the new parity in the uart
            ioctlRetVal = ExternalIoCtl(serialPortFD, TCSETS, ref xfer);
            if (ioctlRetVal < 0)
            {
                throw new Exception ("Error writing terminal configuration on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the number of pending (unread) bytes in the read buffer of the 
        /// UART
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public int BytesInRxBuffer
        {
            get
            {
                int ioctlRetVal = -1;
                int numRxBytes = 0;

                // this is an external call
                ioctlRetVal = ExternalIoCtl(serialPortFD, FIONREAD, ref numRxBytes);
                // did the call succeed?
                if (ioctlRetVal < 0)
                {
                    throw new Exception ("Error reading count of Rx bytes on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
                }
                return (int)numRxBytes;
            }
        }
            
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Removes all data from the Rx and Tx queues. The equivalent of the termios.c 
        /// tcflush call
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void Flush()
        {
            // NOTE - the call below flushes both rx and tx queues
            //        you could just flush one by using...
            // 
            //   TCIFLUSH flushes data received but not read. 
            //   TCOFLUSH flushes data written but not transmitted. 
            //   TCIOFLUSH flushes both
             
            int ioctlRetVal = -1;
            // flush both queues. See Note above
            ioctlRetVal = ExternalIoCtl(serialPortFD, TCFLSH, TCIOFLUSH);
            if (ioctlRetVal < 0)
            {
                throw new Exception ("Error flushing queues on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
            }
        }
            
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends a BREAK (a stream of zero bits) on the serial line
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public void Break()
        {
            // NOTE - because the use of a non-zero argument is so nonstandard
            //        and the output is so variable we hard code the arg to 0 
            //        which should produce a stream of zero bits for between  
            //        0.25 and 0.5 seconds 
 
            int ioctlRetVal = -1;
            // fsend the break. See Note above
            ioctlRetVal = ExternalIoCtl(serialPortFD, TCSBRK, 0);
            if (ioctlRetVal < 0)
            {
                throw new Exception ("Error sending break on Serial device " + SerialPort.ToString() + ", ioctlRetVal=" + ioctlRetVal.ToString());
            }
        }
           
        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Opens the port. Throws an exception on failure
        /// 
        /// NOTE: the blocking mode refers to to the way the serial port code deals
        /// with a request to read more data than is present in the input queue. If
        /// the port is in OPEN_BLOCK mode and more data is requested than is present
        /// then the call will wait until the required amount of data arrives. In
        /// OPEN_NONBLOCK mode the port will just return what it has.
        /// 
        /// </summary>
        /// <param name="openMode">the read mode (BLOCK or NONBLOCK) in which we 
        /// open the serial port.</param>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        protected void OpenPort(SerialPortOpenModeEnum openMode)
        {
            string deviceFileName;
            // set up now
            deviceFileName = RPIDefinitions.TTYDEV_FILENAME;
            deviceFileName = deviceFileName.Replace("%tty%", "0");   

            // we open the file. We have to have an open file descriptor
            // note this is an external call. It has to be because the 
            // ioctl needs an open file descriptor it can use
            if (openMode == SerialPortOpenModeEnum.OPEN_NONBLOCK)
            {
                // open in non-block mode
                serialPortFD = ExternalFileOpen(deviceFileName, O_RDWR | O_NOCTTY | O_NONBLOCK);
            }
            else
            {
                // open in block mode (the default)
                serialPortFD = ExternalFileOpen(deviceFileName, O_RDWR | O_NOCTTY);
            }
            if(serialPortFD <= 0)
            {
                throw new Exception("Could not open serial device file:" + deviceFileName);
            }
            portIsOpen = true;

            // now clear any local flags out
            TermiosStruct xfer = GetTermiosState();
            xfer.c_lflag = 0x00000000;
            xfer.c_oflag = 0x00000000;
            xfer.c_iflag = 0x00000000;
            SetTermiosState(ref xfer);

            //    Console.WriteLine("SerialPort Port Device Enabled: "+ deviceFileName);
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
            //  Console.WriteLine("SerialPort Closing");
            if (serialPortFD >= 0)
            {
                // do an external close
                ExternalFileClose(serialPortFD);
            }
            portIsOpen = false;
            serialPortFD = -1;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the Serial Port. There is no Set accessor this is set in the constructor
        /// </summary>
        /// <history>
        ///    01 Dec 16  Cynic - Originally written
        /// </history>
        public SerialPortEnum SerialPort
        {
            get
            {
                return serialPort;
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
            return FSPortDirectionEnum.PORTDIR_INPUTOUTPUT;
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
                //Console.WriteLine("Disposing SerialPORT");
         
                // call the base to dispose there
                base.Dispose(disposing);

            }
        }
        #endregion

        // #########################################################################
        // ### External Library Calls
        // #########################################################################
        #region External Library Calls

        // these calls are in the libc.so.6 library. We can just say "libc" and mono
        // will figure out which libc.so is the latest version and use that.

        [DllImport("libc", EntryPoint = "write")]
        static extern int ExternalWrite(int fd, byte[] outBuf, int numBytes);

        [DllImport("libc", EntryPoint = "read")]
        static extern int ExternalRead(int fd, byte[] inBuf, int numBytes);

        [DllImport("libc", EntryPoint = "ioctl")]
        static extern int ExternalIoCtl(int fd, uint request, int intVal);

        [DllImport("libc", EntryPoint = "ioctl")]
        static extern int ExternalIoCtl(int fd, uint request, ref int intVal);

        [DllImport("libc", EntryPoint = "ioctl")]
        static extern int ExternalIoCtl(int fd, uint request, ref TermiosStruct xfer);

        [DllImport("libc", EntryPoint = "open")]
        static extern int ExternalFileOpen(string path, int flags);

        [DllImport("libc", EntryPoint = "close")]
        static extern int ExternalFileClose(int fd);

        #endregion
    }
}

