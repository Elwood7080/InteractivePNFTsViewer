using System;
using System.Drawing;
using System.Windows.Forms;

namespace PNFT_Viewer
{
    public partial class Viewer : Form
    {
        private readonly CViewerSteganography Steganography = new CViewerSteganography();
        private readonly CViewerTreeInterface TreeInterface = new CViewerTreeInterface();
        private readonly TreeView VirtualTree = new TreeView();

        private TreeNode CurrentNode;

        public Viewer()
        {
            InitializeComponent();
        }

        private void BtnLoadPicture_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = Constants.SaveFolder,
                Title = "Browse PNFTs",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "png",
                Filter = "png files (*.png)|*.png",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pnlBottom.Height = pnlMain.Height / 2;
                tabControl1.SelectedIndex = 1;
                txtMessage.Text = "";
                checkedListBox1.Items.Clear();
                VirtualTree.Nodes.Clear();
                picture.Image = Image.FromFile(openFileDialog1.FileName);
                byte[] bytes = Steganography.ViewerGetMessage((Bitmap)picture.Image);                
                ViewMessage(bytes);
            }
        }

        private void ViewMessage(byte[] bytes)
        {
            TreeInterface.ViewerMessageToTree(bytes, VirtualTree);

            if (VirtualTree.Nodes.Count > 0)
            {
                if (VirtualTree.Nodes[0].Nodes.Count > 0)
                {
                    CurrentNode = VirtualTree.Nodes[0];
                    txtMessage.Text = CurrentNode.Text;
                    foreach (TreeNode node in CurrentNode.Nodes)
                    {
                        _ = checkedListBox1.Items.Add(node.Text);
                    }                    
                }
                else
                {
                    CurrentNode = VirtualTree.Nodes[0];
                    pnlBottom.Height = 0;
                    txtMessage.Text = CurrentNode.Text;
                }
            }
        }

        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (CurrentNode.Nodes.Count == 1)
            {
                CurrentNode = CurrentNode.Nodes[0];
            }

            foreach (TreeNode node in CurrentNode.Nodes)
            {
                if (node.Text == checkedListBox1.Items[e.Index].ToString())
                {
                    CurrentNode = node;
                    ExploreCurrentNode();
                    break;
                }
            }
        }

        private void ExploreCurrentNode()
        {
            _ = checkedListBox1.BeginInvoke(new Action(() =>
            {
                if (checkedListBox1.CheckedItems.Count > 0)
                {
                    checkedListBox1.Items.Clear();

                    if (CurrentNode.Nodes.Count > 0)
                    {
                        txtMessage.Text = CurrentNode.Nodes[0].Text;

                        if (CurrentNode.Nodes[0].Nodes.Count > 0)
                        {
                            foreach (TreeNode node in CurrentNode.Nodes[0].Nodes)
                            {
                                _ = checkedListBox1.Items.Add(node.Text);
                            }
                        }
                        else
                        {
                            pnlBottom.Height = 0;
                        }
                    }
                }
            }));
        }
    }
}
