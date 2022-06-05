using System.IO;
using System.IO.Compression;
using System.Text;

namespace PNFT_Viewer
{
    public static class ViewerCompression
    {
        public static string ViewerDecompress(byte[] bytes)
        {
            string result;

            try
            {
                using (var msi = new MemoryStream(bytes))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        gs.CopyTo(mso);
                    }
                    result = Encoding.Unicode.GetString(mso.ToArray());
                }
            }
            catch
            {
                result = "";
            }

            return result;
        }
    }
}
