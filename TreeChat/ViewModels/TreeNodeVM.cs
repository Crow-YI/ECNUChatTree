using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using TreeChat.Models;

namespace TreeChat.ViewModels
{
    public class TreeNodeVM : BaseViewModel
    {
        //绘图属性
        public const double WIDTH = 40;
        public const double HEIGHT = 30;

        public double X {  get; set; }
        public double Y { get; set; }

        public List<double> SubtreeWidth { get; set; } = new List<double>();

        public string DisplayContent => Node.NodeID.ToString();

        public ChatTreeNode Node { get; }

        public int ID => Node.NodeID;

        public TreeNodeVM? ParentNode { get; }

        private readonly ObservableCollection<TreeNodeVM> _children;
        public ReadOnlyObservableCollection<TreeNodeVM> Children { get; }

        public TreeNodeVM(ChatTreeNode node, TreeNodeVM? parentNode)
        {
            Node = node;
            _children = new ObservableCollection<TreeNodeVM>();
            Children = new ReadOnlyObservableCollection<TreeNodeVM>(_children);

            foreach (var child in Node.ChildNodes)
            {
                _children.Add(new TreeNodeVM(child, this));
            }

            ParentNode = parentNode;
        }

        public TreeNodeVM AddChild(ChatTreeNode childNode)
        {
            Node.ChildNodes.Add(childNode);
            var childViewModel = new TreeNodeVM(childNode, this);
            _children.Add(childViewModel);
            return childViewModel;
        }

    }
}
