using System;
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
    /// Defines the ioctl transfer structure used by the /dev/ttyO? version of
    /// the SerialPort
    /// 
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    [StructLayout(LayoutKind.Explicit)]
    public struct TermiosStruct
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public UInt32  c_iflag;           // input mode flags 

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public UInt32  c_oflag;           // output mode flags

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(8)]
        public UInt32  c_cflag;           // control mode flags

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(12)]
        public UInt32  c_lflag;           // local mode flags

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(16)]
        public UInt32  c_line;            // line discipline

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        [FieldOffset(20)]
        public string c_cc;
 
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(52)]
        public UInt32  c_ispeed;          // input speed

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(56)]
        public UInt32  c_ospeed;          // output speed
    }
}

