using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PNFT_Viewer
{
    public class CViewerTreeInterface
    {
        public byte[] ExtractMessageFromHiddenBytes(byte[] hiddenBytes)
        {
            byte[] result = hiddenBytes.ToList().Skip(Constants.SecretPhrasesLength + Constants.NumericFieldsLength).ToArray();
            result = result.Take(result.Length - (4 + Constants.SecretPhrasesLength)).ToArray();
            return result;
        }

        public void ViewerMessageToTree(byte[] byteArray, TreeView treeView)
        {          
            string text = ViewerCompression.ViewerDecompress(byteArray);
            if (text == "")
            {
                ViewerUtils.ShowError("Empty message or decompression error! This image may not be a PNFTs, or you may have downloaded the preview instead of the real image!");
                return;
            }

            if (text.Contains(Constants.FileSeparator) && text.Contains(Constants.RecordSeparator))
            {
                List<string> files = text.Split(new string[] { Constants.FileSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (files.Count == 2)
                {
                    // Parent-child nodes
                    Dictionary<string, string> childParentDictionary = new Dictionary<string, string>();
                    foreach (string parentChild in files[0].Split(new string[] { Constants.GroupSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList())
                    {
                        List<string> parentChildList = parentChild.Split(new string[] { Constants.RecordSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (parentChildList.Count != 2)
                        {
                            ViewerUtils.ShowError("Wrong number of elements in parent-child tuple!");
                            return;
                        }

                        if (!childParentDictionary.ContainsKey(parentChildList[1]))
                        {
                            childParentDictionary.Add(parentChildList[1], parentChildList[0]);
                        }
                        else
                        {
                            ViewerUtils.ShowError("Node added multiple times to parent-child tuples!");
                            return;
                        }
                    }

                    // Nodes list
                    Dictionary<string, string> nodePartsDictionary = new Dictionary<string, string>();
                    foreach (string nodeParts in files[1].Split(new string[] { Constants.GroupSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList())
                    {
                        List<string> nodePartsList = nodeParts.Split(new string[] { Constants.RecordSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (nodePartsList.Count != 2)
                        {
                            ViewerUtils.ShowError("Wrong number of elements in node parts tuple!");
                            return;
                        }

                        if (!nodePartsDictionary.ContainsKey(nodePartsList[0]))
                        {
                            nodePartsDictionary.Add(nodePartsList[0], nodePartsList[1]);
                        }
                        else
                        {
                            ViewerUtils.ShowError("Node added multiple times to node parts tuples!");
                            return;
                        }
                    }

                    do
                    {
                        foreach (string childNodeName in nodePartsDictionary.Keys)
                        {
                            string parentNodeName = "";
                            if (childParentDictionary.ContainsKey(childNodeName))
                            {
                                parentNodeName = childParentDictionary[childNodeName];
                            }

                            if (!treeView.Nodes.ContainsKey(childNodeName))
                            {
                                if (parentNodeName == Constants.RootNode)
                                {
                                    treeView.Nodes.Add(childNodeName, nodePartsDictionary[childNodeName]);
                                    childParentDictionary.Remove(childNodeName);
                                }
                                else
                                {
                                    TreeNode[] nodes = treeView.Nodes.Find(parentNodeName, true);
                                    if (nodes.Length > 0)
                                    {
                                        nodes[0].Nodes.Add(childNodeName, nodePartsDictionary[childNodeName]);
                                        childParentDictionary.Remove(childNodeName);
                                    }
                                }
                            }
                        }
                    } while (childParentDictionary.Count > 0);                    
                }
                else
                {
                    ViewerUtils.ShowError("Wrong number of lines!");
                }
            }
            else
            {
                ViewerUtils.ShowError("Characters not found!");
            }
        }
    }
}
