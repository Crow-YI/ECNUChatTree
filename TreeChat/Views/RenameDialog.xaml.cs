using System.Windows;

namespace TreeChat.Views
{
    /// <summary>
    /// 重命名对话框，用于修改对话名称
    /// </summary>
    public partial class RenameDialog : Window
    {
        /// <summary>
        /// 获取或设置新名称
        /// </summary>
        public string NewName { get; private set; }

        /// <summary>
        /// 构造函数，初始化对话框
        /// </summary>
        /// <param name="currentName">当前名称</param>
        public RenameDialog(string currentName)
        {
            InitializeComponent();
            NameTextBox.Text = currentName;
            NewName = currentName;
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        }

        /// <summary>
        /// 确定按钮点击事件处理
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            NewName = NameTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 取消按钮点击事件处理
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
