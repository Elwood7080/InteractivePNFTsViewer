﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PNFT_Viewer
{
    public static class Constants
    {
        public enum ContentType
        {
            None = 0,
            Tree = 1
        }

        public const int SecretPhrasesLength = 41;
        public const int NumericFieldsLength = 88;

        public static readonly string RootNode = ((char)27).ToString();
        public static readonly string FileSeparator = ((char)28).ToString();
        public static readonly string GroupSeparator = ((char)29).ToString();
        public static readonly string RecordSeparator = ((char)30).ToString();
        
        public static readonly Dictionary<int, List<byte>> SecretPhrases = new Dictionary<int, List<byte>>();        

        public static string SaveFolder = Path.GetDirectoryName(Application.ExecutablePath);
        public static readonly bool LogEnabled;
        public static readonly string LogFilepath;

        public static readonly string Eula =

            "Boost Software License - Version 1.0 - August 17th, 2003                     " +
            Environment.NewLine + Environment.NewLine +
            "Permission is hereby granted, free of charge, to any person or organization " +
            "obtaining a copy of the software and accompanying documentation covered by " +
            "this license (the \"Software\") to use, reproduce, display, distribute, " +
            "execute, and transmit the Software, and to prepare derivative works of the " +
            "Software, and to permit third-parties to whom the Software is furnished to " +
            "do so, all subject to the following:" +
            Environment.NewLine + Environment.NewLine +
            "The copyright notices in the Software and this entire statement, including " +
            "the above license grant, this restriction and the following disclaimer, " +
            "must be included in all copies of the Software, in whole or in part, and " +
            "all derivative works of the Software, unless such copies or derivative " +
            "works are solely in the form of machine-executable object code generated by " +
            "a source language processor. " +
            Environment.NewLine + Environment.NewLine +
            "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR " +
            "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, " +
            "FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT.IN NO EVENT " +
            "SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE " +
            "FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE, " +
            "ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER " +
            "DEALINGS IN THE SOFTWARE.";

        public static readonly string Note =
            "How to use the software" +
            Environment.NewLine + Environment.NewLine +
            "This software is just an example of how to view the content of PNFTs. " +
            "You can start from these sources and the file with the description of the " +
            "data format and develop a viewer or creator of PNFTs according to your needs.";

        static Constants()
        {            
            SecretPhrases.Add(1, Encoding.ASCII.GetBytes("Hey guys! I'm talking! Do you believe it?").ToList());    // Secret phrase 1 for the first versions (v.0.0, v.0.1, v.0.2)
            SecretPhrases.Add(2, Encoding.ASCII.GetBytes("Hey guys I'm talking! Isn't that amazing?").ToList());    // Secret phrase 2 for the latest versions (v.0.3+)
            for (int i = 1; i <= 2; i++) if (SecretPhrases[i].Count != SecretPhrasesLength) ViewerUtils.ShowError("Secret phrase n." + i.ToString() + ": length error");

            LogEnabled = File.Exists(Path.Combine(Constants.SaveFolder, "log-enabled"));
            if (LogEnabled)
            {
                string logsFolder = Path.Combine(Constants.SaveFolder, "Logs");
                if (!Directory.Exists(logsFolder)) Directory.CreateDirectory(logsFolder);
                LogFilepath = Path.Combine(logsFolder, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log");
                File.AppendAllText(LogFilepath, "Secret phrase 1: " + BitConverter.ToString(SecretPhrases[1].ToArray()) + Environment.NewLine);
                File.AppendAllText(LogFilepath, "Secret phrase 2: " + BitConverter.ToString(SecretPhrases[2].ToArray()) + Environment.NewLine);
            }
        }
    }
}
