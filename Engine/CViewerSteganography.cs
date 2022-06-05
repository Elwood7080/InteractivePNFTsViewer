using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PNFT_Viewer
{
    public class CViewerSteganography
    {
        private readonly ProgressBar hideProgressBar;

        private bool stop;

        private int startX = 0;
        private int startY = 0;
        private int secretPhraseNumber = 0;

        private UInt16 imageWidth;
        private UInt16 imageHeight;

        private UInt32 compresseMessageLength = 0;
        private UInt32 type;            // Future use
        private UInt32 versionMajor;    // Future use
        private UInt32 versionMinor;    // Future use
        private UInt32 width;           // Future use
        private UInt32 height;          // Future use

        // Buffer used to store temporary data
        private readonly List<byte> buffer = new List<byte>();

        // Message hidden in the image
        private List<byte> message = new List<byte>();

        public CViewerSteganography(ProgressBar progressBar = null)
        {
            hideProgressBar = progressBar;
        }

        /// <summary>
        /// Extracts hidden bytes from image
        /// </summary>
        /// <param name="bitmap">Image bitmap</param>
        /// <returns>The hidden bytes</returns>
        public byte[] ViewerGetMessage(Bitmap bitmap)
        {
            Initialize(bitmap);

            for (int y = 0, byteCounter = 0; (y < startY + imageHeight) && (y < bitmap.Height) && (!stop); y++)
            {
                for (int x = startX; (x < startX + imageWidth) && (x < bitmap.Width) && (!stop); x++)
                {
                    if (hideProgressBar != null)
                    {
                        hideProgressBar.Value++;
                        Application.DoEvents();
                    }

                    byte pixelHiddenValue = GetPixelHiddenValue(x, y, bitmap);

                    if (byteCounter < Constants.SecretPhrasesLength)
                    {
                        SecretPhrase1Decoder(x, y, pixelHiddenValue, ref byteCounter);
                    }
                    else
                    {
                        if (byteCounter < Constants.SecretPhrasesLength)
                        {
                            buffer.Clear();
                        }
                        
                        if (secretPhraseNumber == 1)
                        {
                            FirstVersionsDecoder(pixelHiddenValue, ref byteCounter);
                        }
                        else if (secretPhraseNumber == 2)
                        {
                            LatestVersionsDecoder(pixelHiddenValue, ref byteCounter);
                        }
                    }

                    byteCounter++;
                }
            }

            if (hideProgressBar != null) hideProgressBar.Value = 0;

            return message.ToArray();
        }

        /// <summary>
        /// Get the hidden value of the pixel (1 pixel = 4 bytes, hidden value = 1 byte)
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="bitmap">The bitmap of the image</param>
        /// <returns></returns>
        private byte GetPixelHiddenValue(int x, int y, Bitmap bitmap)
        {
            Color color = bitmap.GetPixel(x, y);
            byte pixelHiddenValue = ViewerUtils.ViewerGetPixelHiddenValue(color);
            return pixelHiddenValue;
        }

        /// <summary>
        /// Initialize some properties
        /// </summary>
        /// <param name="bitmap">The bitmap of the image</param>
        private void Initialize(Bitmap bitmap)
        {
            stop = false;

            buffer.Clear();
            message.Clear();

            if (hideProgressBar != null)
            {
                hideProgressBar.Minimum = 0;
                hideProgressBar.Maximum = bitmap.Width * bitmap.Height;
                hideProgressBar.Value = 0;
            }

            imageWidth = (UInt16)bitmap.Width;
            imageHeight = (UInt16)bitmap.Height;
            if (Constants.LogEnabled) File.AppendAllText(Constants.LogFilepath, "Starting] Image witdth: " + imageWidth.ToString() + Environment.NewLine);
            if (Constants.LogEnabled) File.AppendAllText(Constants.LogFilepath, "Starting] Image height: " + imageHeight.ToString() + Environment.NewLine);
        }

        /// <summary>
        /// Decode the first secret phrase (the one at the beggining of the byte array)
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="pixelHiddenValue">The hidden value of pixel x, y</param>
        /// <param name="byteCounter">The counter of the bytes</param>
        private void SecretPhrase1Decoder(int x, int y, byte pixelHiddenValue, ref int byteCounter)
        {
            if ((pixelHiddenValue != Constants.SecretPhrases[1][byteCounter]) && (pixelHiddenValue != Constants.SecretPhrases[2][byteCounter]))
            {
                byteCounter = -1;
                compresseMessageLength = UInt32.MaxValue;
                // if (Constants.LogEnabled) File.AppendAllText(Constants.LogFilepath, "Reading] Value: " + pixelHiddenValue.ToString("X2") + "; byte counter: " + byteCounter.ToString() + "; x: " + x.ToString() + "; y: " + y.ToString() + Environment.NewLine);
            }
            else
            {
                if (Constants.LogEnabled) File.AppendAllText(Constants.LogFilepath, "SecretPhrase] Byte counter: " + byteCounter.ToString() + "; x: " + x.ToString() + "; y: " + y.ToString() + Environment.NewLine);

                if (pixelHiddenValue != Constants.SecretPhrases[1][byteCounter]) secretPhraseNumber = 2;
                else if (pixelHiddenValue != Constants.SecretPhrases[2][byteCounter]) secretPhraseNumber = 1;

                if (byteCounter == 0)
                {
                    startX = x;
                    startY = y;                    
                }
            }
        }

        /// <summary>
        /// The decoder for the first versions
        /// </summary>
        /// <param name="pixelHiddenValue"></param>
        /// <param name="byteCounter">The counter of the bytes</param>
        private void FirstVersionsDecoder(byte pixelHiddenValue, ref int bytesCounter)
        {
            // The buffer is cleared after each extraction
            buffer.Add(pixelHiddenValue);

            switch (bytesCounter - Constants.SecretPhrasesLength + 1)
            {
                case 4: compresseMessageLength = ExtractUInt32(buffer, "Message length"); break;
                case 6: width = ExtractUInt16(buffer, "Image width"); break;
                case 8: versionMajor = ExtractUInt16(buffer, "Version major"); break;
                case 10: height = ExtractUInt16(buffer, "Image height"); break;                               
                case 12: versionMinor = ExtractUInt16(buffer, "Version minor"); break;                
            };

            if (bytesCounter - Constants.SecretPhrasesLength + 1 == 12 + compresseMessageLength) message = ExtractMessage(buffer, compresseMessageLength, "Message", false);
            if (bytesCounter - Constants.SecretPhrasesLength + 1 == 12 + compresseMessageLength + Constants.SecretPhrases[1].Count)
            {
                if (!buffer.SequenceEqual(Constants.SecretPhrases[1]))
                {
                    ViewerUtils.ShowError("SPR-2B");
                }
            }
        }

        /// <summary>
        /// The decoder for the latest versions
        /// </summary>
        /// <param name="pixelHiddenValue"></param>
        /// <param name="bytesCounter">The counter of the bytes</param>
        private void LatestVersionsDecoder(byte pixelHiddenValue, ref int bytesCounter)
        {
            // The buffer is cleared after each extraction
            buffer.Add(pixelHiddenValue);

            switch (bytesCounter - Constants.SecretPhrasesLength + 1)
            {
                case 8: width = ExtractUInt32WithCRC(buffer, "Image width"); break;
                case 16: height = ExtractUInt32WithCRC(buffer, "Image height"); break;
                case 24: compresseMessageLength = ExtractUInt32WithCRC(buffer, "Message length"); break;
                case 32: versionMajor = ExtractUInt32WithCRC(buffer, "Version major"); break;
                case 40: versionMinor = ExtractUInt32WithCRC(buffer, "Version minor"); break;
                case 48: type = ExtractUInt32WithCRC(buffer, "Type"); break;
                case 56: _ = ExtractUInt32WithCRC(buffer, "Res1"); break;
                case 64: _ = ExtractUInt32WithCRC(buffer, "Res2"); break;
                case 72: _ = ExtractUInt32WithCRC(buffer, "Res3"); break;
                case 80: _ = ExtractUInt32WithCRC(buffer, "Res4"); break;
                case 88: _ = ExtractUInt32WithCRC(buffer, "Res5"); break;
            };

            if (bytesCounter - Constants.SecretPhrasesLength + 1 == Constants.NumericFieldsLength + compresseMessageLength + 4) message = ExtractMessage(buffer, compresseMessageLength, "Message");
            if (bytesCounter - Constants.SecretPhrasesLength + 1 == Constants.NumericFieldsLength + compresseMessageLength + 4 + Constants.SecretPhrases[2].Count)
            {
                if (!buffer.SequenceEqual(Constants.SecretPhrases[2]))
                {
                    ViewerUtils.ShowError("SPR-2B");
                }
            }
        }

        /// <summary>
        /// Extracts 2 bytes as UInt16
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="operation">Type of operation (for log purposes)</param>
        /// <returns>The value</returns>
        private UInt16 ExtractUInt16(List<byte> buffer, string operation)
        {
            UInt16 result;

            if (buffer.Count >= 2)
            {
                byte[] value = buffer.GetRange(0, 2).ToArray();
                result = BitConverter.ToUInt16(value, 0);
            }
            else
            {
                ViewerUtils.ShowError(operation + ": error decoding message (EUI16)");
                result = 0;
            }

            buffer.Clear();
            return result;
        }

        /// <summary>
        /// Extracts 4 bytes as UInt32
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="operation">Type of operation (for log purposes)</param>
        /// <returns>The value</returns>
        private UInt32 ExtractUInt32(List<byte> buffer, string operation)
        {
            UInt32 result;

            if (buffer.Count >= 4)
            {
                byte[] value = buffer.GetRange(0, 4).ToArray();
                result = BitConverter.ToUInt32(value, 0);
            }
            else
            {
                ViewerUtils.ShowError(operation + ": error decoding message (EUI16)");
                result = 0;
            }

            buffer.Clear();
            return result;
        }

        /// <summary>
        /// Extracts 4 bytes as UInt32, checks the CRC and clear the buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="operation">Type of operation (for log purposes)</param>
        /// <returns>The value</returns>
        private UInt32 ExtractUInt32WithCRC(List<byte> buffer, string operation)
        {
            UInt32 result;

            if (buffer.Count >= 8)
            {
                byte[] value = buffer.GetRange(0, 4).ToArray();
                byte[] CRC = buffer.GetRange(4, 4).ToArray();

                if (BitConverter.GetBytes(ViewerUtils.CRC32(value)).SequenceEqual(CRC))
                {
                    result = BitConverter.ToUInt32(value, 0);
                }
                else
                {
                    ViewerUtils.ShowError(operation + ": CRC error");
                    result = 0;
                }
            }
            else
            {
                ViewerUtils.ShowError("Error decoding message (EUI32)");
                result = 0;
            }

            buffer.Clear();
            return result;
        }

        /// <summary>
        /// Extracts the message, checks the CRC and clear buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="compressedMessageLength">Length of compressed message</param>
        /// <param name="operation">Type of operation</param>
        /// <param name="checkCRC">True if CRC should be checked</param>
        /// <returns></returns>
        private List<byte> ExtractMessage(List<byte> buffer, UInt32 compressedMessageLength, string operation, bool checkCRC = true)
        {
            List<byte> result;

            if (compressedMessageLength + (checkCRC ? 4 : 0) <= buffer.Count)
            {
                byte[] value = buffer.GetRange(0, (int)compressedMessageLength).ToArray();
                byte[] CRC = checkCRC ? buffer.GetRange(0 + (int)compressedMessageLength, 4).ToArray() : null;

                if ((!checkCRC) || BitConverter.GetBytes(ViewerUtils.CRC32(value)).SequenceEqual(CRC))
                {
                    result = value.ToList();
                }
                else
                {
                    ViewerUtils.ShowError(operation + ": CRC error");
                    result = new List<byte>();
                }
            }
            else
            {
                ViewerUtils.ShowError("Error decoding message (EMSG)");
                result = new List<byte>();
            }

            buffer.Clear();
            return result;
        }
    }
}
