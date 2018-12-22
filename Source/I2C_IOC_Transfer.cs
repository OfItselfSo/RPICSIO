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
	// Information taken from i2c-dev.h and i2c.h linux

	/// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// Defines the Ioctl transfer structures used by the I2C_RDWR version of
    /// the I2CPort (I2CPortFS)
    /// 
    /// </summary>
    /// <history>
    ///    12 Dec 18  Ridler - Originally written
    /// </history>
    [StructLayout(LayoutKind.Explicit,Size=12)]
    public struct i2c_msg
    {
        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(0)]
        public UInt16 addr;			// 2 bytes - slave address

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(2)]
        public UInt16 flags;		// 2 bytes

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(4)]
        public UInt16 len;			// 2 bytes = msg length

		[MarshalAs(UnmanagedType.U2)]
        [FieldOffset(6)]
		public UInt16 pad;			// 2 bytes - Padding for alignment

        [MarshalAs(UnmanagedType.LPStr)]
        [FieldOffset(8)]
        public IntPtr  buf;			// 4 bytes - pointer to msg data
	}

	// This is the structure as used in the I2C_RDWR ioctl call
    [StructLayout(LayoutKind.Explicit,Size=8)]
    public struct i2c_rdwr_ioctl_data
	{
        [MarshalAs(UnmanagedType.LPStr)]
        [FieldOffset(0)]
        public IntPtr msgs;        // 4 bytes - pointers to i2c_msgs

		[MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public UInt32 nmsgs;       // 4 bytes - number of i2c_msgs
	}
}

