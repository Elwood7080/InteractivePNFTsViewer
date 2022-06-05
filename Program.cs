using Interactive_PNFT_Viewer;
using System;
using System.Windows.Forms;

namespace PNFT_Viewer
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Interactive_PNFT_Viewer.Properties.Settings.Default.ShowEula)
            {
                Application.Run(new Eula());
            }
            else
            {
                Application.Run(new Viewer());
            }            
        }
    }
}
