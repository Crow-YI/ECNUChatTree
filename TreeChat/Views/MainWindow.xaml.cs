using System.Windows;
using System.Windows.Controls;
using TreeChat.Models;
using TreeChat.ViewModels;
using TreeChat.Views;

namespace TreeChat.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm = new();


        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            _vm.OnChatTreeChanged += treeView.RefreshView;

            // 假设你已经有一个ChatTree实例和对应的树形ViewModel
            var chatTree = new ChatTree();
            var rootViewModel = new TreeNodeViewModel(chatTree.RootNode, null);

            treeView.SetTree(chatTree, rootViewModel);
        }

       
    }
}