using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace PNFT_Viewer
{
    public static class ViewerUtils
    {
        public static uint CRC32(byte[] bytes)
        {
            uint[] crcTable = MakeCrcTable();
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; i++)
            {
                crc = (crc >> 8) ^ crcTable[(crc ^ bytes[i]) & 0xFF];
            }

            return ~crc; //(crc ^ (-1)) >> 0;
        }

        internal static uint[] MakeCrcTable()
        {
            uint c;
            uint[] crcTable = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                c = n;
                for (int k = 0; k < 8; k++)
                {
                    var res = c & 1;
                    c = (res == 1) ? (0xEDB88320 ^ (c >> 1)) : (c >> 1);
                }
                crcTable[n] = c;
            }

            return crcTable;
        }

        public static byte ViewerGetPixelHiddenValue(Color clr_combined)
        {
            byte a = (byte)((clr_combined.R) & 7);
            byte b = (byte)((clr_combined.G) & 7);
            byte c = (byte)((clr_combined.B) & 3);

            byte value = (byte)(a + (b << 3) + (c << 6));

            return value;
        }

        public static void ShowError(string errorText)
        {
            if (Constants.LogEnabled) File.AppendAllText(Constants.LogFilepath, errorText + Environment.NewLine);
            MessageBox.Show(errorText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
