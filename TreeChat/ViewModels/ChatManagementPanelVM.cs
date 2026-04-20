using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TreeChat.Commands;
using TreeChat.Models;
using TreeChat.Services;

namespace TreeChat.ViewModels
{
    /// <summary>
    /// 聊天管理面板ViewModel，管理多个对话的创建、保存、读取和重命名
    /// </summary>
    public class ChatManagementPanelVM : BaseViewModel
    {
        private ObservableCollection<ChatTree> _chatList;
        private ChatTree? _selectedChat;
        private readonly IFileService _fileService;

        /// <summary>
        /// 对话列表
        /// </summary>
        public ObservableCollection<ChatTree> ChatList
        {
            get => _chatList;
        }

        /// <summary>
        /// 当前选中的对话
        /// </summary>
        public ChatTree? SelectedChat
        {
            get => _selectedChat;
            set
            {
                SetProperty(ref _selectedChat, value);
                if (value != null)
                    SelectedChatChanged?.Invoke(value);
                
                SaveChat.OnCanExecuteChanged();
                RenameChat.OnCanExecuteChanged();
            }
        }

        /// <summary>
        /// 创建新对话命令
        /// </summary>
        public RelayCommand CreateNewChat { get; }

        /// <summary>
        /// 保存对话命令
        /// </summary>
        public RelayCommand SaveChat { get; }

        /// <summary>
        /// 读取对话命令
        /// </summary>
        public RelayCommand LoadChat { get; }

        /// <summary>
        /// 重命名对话命令
        /// </summary>
        public RelayCommand RenameChat { get; }

        /// <summary>
        /// 选中对话变更事件
        /// </summary>
        public event Action<ChatTree>? SelectedChatChanged;

        /// <summary>
        /// 构造函数，初始化命令和服务
        /// </summary>
        public ChatManagementPanelVM()
        {
            _chatList = new ObservableCollection<ChatTree>();
            _fileService = new FileService();

            CreateNewChat = new RelayCommand(ExecuteCreateNewChat);
            SaveChat = new RelayCommand(ExecuteSaveChat, CanExecuteSaveChat);
            LoadChat = new RelayCommand(ExecuteLoadChat);
            RenameChat = new RelayCommand(ExecuteRenameChat, CanExecuteRenameChat);
        }

        /// <summary>
        /// 执行创建新对话
        /// </summary>
        private void ExecuteCreateNewChat(object? parameter)
        {
            // 显示配置窗口
            var configDialog = new Views.ConfigDialog();
            if (configDialog.ShowDialog() == true)
            {
                // 使用用户配置创建新对话
                ChatTree newTree = new ChatTree(
                    apiKey: configDialog.ApiKey,
                    apiEndpoint: configDialog.ApiEndpoint,
                    modelName: configDialog.ModelName,
                    temperature: configDialog.Temperature,
                    topP: configDialog.TopP,
                    topK: configDialog.TopK
                );
                ChatList.Add(newTree);
                SelectedChat = newTree;
            }
        }

        /// <summary>
        /// 判断是否可以保存对话（必须选中一个对话）
        /// </summary>
        private bool CanExecuteSaveChat(object? parameter)
        {
            return SelectedChat != null;
        }

        /// <summary>
        /// 执行保存对话到文件
        /// </summary>
        private void ExecuteSaveChat(object? parameter)
        {
            if (SelectedChat == null) return;

            bool success = _fileService.SaveChatTree(SelectedChat);
            if (success)
            {
                MessageBox.Show("保存成功！", "提示", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 执行从文件读取对话
        /// </summary>
        private void ExecuteLoadChat(object? parameter)
        {
            var loadedTree = _fileService.LoadChatTree();
            if (loadedTree != null)
            {
                ChatList.Add(loadedTree);
                SelectedChat = loadedTree;
            }
        }

        /// <summary>
        /// 从指定文件路径加载对话
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void LoadChatFromPath(string filePath)
        {
            var loadedTree = _fileService.LoadChatTree(filePath);
            if (loadedTree != null)
            {
                ChatList.Add(loadedTree);
                SelectedChat = loadedTree;
            }
        }

        /// <summary>
        /// 判断是否可以重命名对话（必须选中一个对话）
        /// </summary>
        private bool CanExecuteRenameChat(object? parameter)
        {
            return SelectedChat != null;
        }

        /// <summary>
        /// 执行重命名对话
        /// </summary>
        private void ExecuteRenameChat(object? parameter)
        {
            if (SelectedChat == null) return;

            var dialog = new Views.RenameDialog(SelectedChat.TreeTitle);
            if (dialog.ShowDialog() == true)
            {
                string newName = dialog.NewName;
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    SelectedChat.TreeTitle = newName;
                    
                    // 刷新列表显示
                    int index = ChatList.IndexOf(SelectedChat);
                    if (index >= 0)
                    {
                        ChatList[index] = SelectedChat;
                    }
                }
            }
        }
    }
}
