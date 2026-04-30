using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TreeChat.Commands;
using TreeChat.Models;
using TreeChat.Services;

namespace TreeChat.ViewModels
{
    public class TreeVisualizationVM : BaseViewModel
    {
        // 根节点VM
        public TreeNodeVM? RootNode { get; private set; }

        // 选中的节点（与View的依赖属性双向绑定）
        private TreeNodeVM? _selectedNode;
        public TreeNodeVM? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if(value != null && _selectedNode != value)
                    SelectedNodeChanged?.Invoke(value);
                SetProperty(ref _selectedNode, value);
                RenameNodeCommand?.OnCanExecuteChanged();
            }
        }

        private ChatTree? _currentChatTree;
        public ChatTree? CurrentChatTree
        {
            get => _currentChatTree;
            set => SetProperty(ref _currentChatTree, value);
        }

        public RelayCommand ShowConfigCommand { get; }
        public RelayCommand RenameNodeCommand { get; }

        public event Action? CanvasPropertyChanged;

        public event Action<TreeNodeVM>? SelectedNodeChanged;

        public TreeVisualizationVM()
        {
            RootNode = null;
            // 先初始化命令
            ShowConfigCommand = new RelayCommand(ExecuteShowConfig);
            RenameNodeCommand = new RelayCommand(ExecuteRenameNode, CanExecuteRenameNode);
            // 再设置SelectedNode
            SelectedNode = null;
        }

        private bool CanExecuteRenameNode(object? parameter)
        {
            return SelectedNode != null;
        }

        private void ExecuteRenameNode(object? parameter)
        {
            if (SelectedNode == null)
                return;

            string currentName = SelectedNode.Node.Name ?? SelectedNode.Node.NodeID.ToString();
            var dialog = new Views.RenameDialog(currentName);
            if (dialog.ShowDialog() == true)
            {
                string newName = dialog.NewName;
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    SelectedNode.Node.Name = newName;
                    // 重新计算整个树的布局
                    if (RootNode != null)
                    {
                        TreeLayoutService.LayoutTree(RootNode);
                    }
                    // 通知UI更新
                    OnPropertyChanged(nameof(SelectedNode));
                    CanvasPropertyChanged?.Invoke();
                }
            }
        }

        private void ExecuteShowConfig(object? parameter)
        {
            if (CurrentChatTree == null)
            {
                MessageBox.Show("当前没有选中的对话。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Views.ConfigDialog();
            // 设置当前配置值
            dialog.ApiKey = CurrentChatTree.ApiKey;
            dialog.ApiEndpoint = CurrentChatTree.ApiEndpoint;
            dialog.ModelName = CurrentChatTree.ModelName;
            dialog.Temperature = CurrentChatTree.Temperature;
            dialog.TopP = CurrentChatTree.TopP;
            dialog.TopK = CurrentChatTree.TopK;

            if (dialog.ShowDialog() == true)
            {
                // 保存新的配置值
                CurrentChatTree.ApiKey = dialog.ApiKey;
                CurrentChatTree.ApiEndpoint = dialog.ApiEndpoint;
                CurrentChatTree.ModelName = dialog.ModelName;
                CurrentChatTree.Temperature = dialog.Temperature;
                CurrentChatTree.TopP = dialog.TopP;
                CurrentChatTree.TopK = dialog.TopK;
            }
        }

        public void SetTree(TreeNodeVM rootNode)
        {
            RootNode = rootNode;
            TreeLayoutService.LayoutTree(RootNode);
            CanvasPropertyChanged?.Invoke();
            SelectedNode = rootNode;
        }

        public void UpdateTree(TreeNodeVM updateNode, TreeNodeVM selectedNode)
        {
            if(RootNode == null)
                return;
            TreeLayoutService.UpdateLayoutTree(updateNode);
            CanvasPropertyChanged?.Invoke();
        }
    }
}