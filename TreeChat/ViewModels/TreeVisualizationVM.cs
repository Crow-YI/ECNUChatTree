using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            }
        }

        public event Action? CanvasPropertyChanged;

        public event Action<TreeNodeVM>? SelectedNodeChanged;

        public TreeVisualizationVM()
        {
            RootNode = null;
            SelectedNode = null;
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
