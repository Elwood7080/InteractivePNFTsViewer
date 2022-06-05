using PNFT_Viewer;
using System;
using System.Windows.Forms;

namespace Interactive_PNFT_Viewer
{
    public partial class Eula : Form
    {
        public Eula()
        {
            InitializeComponent();

            textBox1.Text =
                Constants.Eula +
                Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                Constants.Note;
        }

        private void Button1_Click(object sender, System.EventArgs e)
        {
            Application.Exit(); 
        }

        private void Button2_Click(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.ShowEula = false;
            Properties.Settings.Default.Save();

            this.Hide();
            var viewer = new Viewer();
            viewer.Closed += (s, args) => this.Close();
            viewer.Show();
        }
    }
}
