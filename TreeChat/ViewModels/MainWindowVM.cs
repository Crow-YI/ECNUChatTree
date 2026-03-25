using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeChat.Commands;
using TreeChat.Models;
using TreeChat.Services;

namespace TreeChat.ViewModels
{
    /// <summary>
    /// 主窗口VM
    /// </summary>
    public class MainWindowVM : BaseViewModel
    {
        public ChatManagementPanelVM ChatManagementPanelVM { get; set; }
        public TreeVisualizationVM TreeVisualizationVM { get; set; }
        public ChatInformationVM ChatInformationVM { get; set; }

        public MainWindowVM()
        {
            ChatManagementPanelVM = new ChatManagementPanelVM();
            TreeVisualizationVM = new TreeVisualizationVM();
            ChatInformationVM = new ChatInformationVM();

            //事件绑定
            ChatManagementPanelVM.SelectedChatChanged += ChangeNodeVMTree;
            TreeVisualizationVM.SelectedNodeChanged += (nodeVM) => { ChatInformationVM.SelectedNode = nodeVM; };
            ChatInformationVM.ChatTreeChanged += TreeVisualizationVM.UpdateTree;
        }

        private void ChangeNodeVMTree(ChatTree tree)
        {
            TreeNodeVM rootNodeVM = InitializeNodeVMTree(tree.RootNode, null);
            TreeVisualizationVM.SetTree(rootNodeVM);
        }

        private TreeNodeVM InitializeNodeVMTree(ChatTreeNode currentNode, TreeNodeVM? parentVM)
        {
            TreeNodeVM currentVM = new TreeNodeVM(currentNode, parentVM);
            foreach(ChatTreeNode node in currentNode.ChildNodes)
            {
                InitializeNodeVMTree(node, currentVM);
            }
            return currentVM;
        }
    }
}

