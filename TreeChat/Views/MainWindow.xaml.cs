using System.Windows;
using System.Windows.Controls;
using TreeChat.Models;
using TreeChat.ViewModels;
using TreeChat.Views;

namespace TreeChat.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowVM _vm = new();


        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;
        }
    }
}