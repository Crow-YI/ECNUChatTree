using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeChat.Models;
using TreeChat.Services;

namespace TreeChat.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private OpenAIChat _openaiChat = new OpenAIChat();

        private ObservableCollection<ChatTree> _chatTreeList = new();
        private ChatTree? _selectedChatTree;

        public MainWindowViewModel()
        {
            CreateNewTree = new RelayCommand(ExecuteCreateNewTree);
            SendMessage = new AsyncRelayCommand(ExecuteSendMessageAsync);
        }

        public ObservableCollection<ChatTree> ChatTreeList
        {
            get { return _chatTreeList; }
        }

        public ChatTree? SelectedChatTree
        {
            get { return _selectedChatTree; }
            set { SetProperty(ref _selectedChatTree, value); }
        }

        public RelayCommand CreateNewTree { get; }

        private void ExecuteCreateNewTree(object? parameter)
        {
            ChatTree newTree = new ChatTree();
            ChatTreeList.Add(newTree);
            SelectedChatTree = newTree;
        }

        private TreeNodeViewModel _selectedNode;
        public TreeNodeViewModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                SetProperty(ref _selectedNode, value);
                UserInformation = value.Node.UserMessage.Content;
                if(value.Node.ReplyMessage == null)
                    AIReply = string.Empty;
                else
                    AIReply = value.Node.ReplyMessage.Content;
            }
        }

        private string? _userInformation;
        private string? _aiReply;
        public string UserInformation
        {
            get 
            { 
                if (_userInformation == null)
                    return string.Empty;
                else
                    return _userInformation; 
            }
            set { SetProperty(ref _userInformation, value); }
        }
        public string AIReply
        {
            get 
            { 
                if(_aiReply == null)
                    return string.Empty;
                else
                    return _aiReply; 
            }
            set { SetProperty(ref _aiReply, value); }
        }

        private string _inputMessage;
        public string InputMessage
        {
            get { return _inputMessage; }
            set { SetProperty(ref _inputMessage, value); }
        }

        public AsyncRelayCommand SendMessage { get; }

        private bool _isSending;
        public bool IsSending
        {
            get => _isSending;
            set => SetProperty(ref _isSending, value);
        }

        public event Action OnChatTreeChanged;

        private async Task ExecuteSendMessageAsync(object? parameter)
        {
            // 边界校验：空输入/未选中节点直接返回
            if (string.IsNullOrWhiteSpace(InputMessage) || SelectedNode == null) return;

            try
            {
                // 1. 设置加载状态（UI会自动禁用按钮）
                IsSending = true;

                // 2. 创建新节点并更新UI（同步操作，快速反馈）
                ChatTreeNode newNode = new ChatTreeNode(SelectedNode.Node, new ChatMessage("user", InputMessage));
                TreeNodeViewModel newNodeViewModel = SelectedNode.AddChild(newNode);
                SelectedNode = newNodeViewModel;
                OnChatTreeChanged?.Invoke();

                // 3. 清空输入框
                InputMessage = string.Empty;

                // 4. 异步调用AI接口（核心：用await替代.Result，不阻塞UI）
                string aiReply = await _openaiChat.CallAiApi(SelectedNode.Node.GetFullContext());

                // 5. 更新AI回复到UI
                SelectedNode.Node.SetAiReply(new ChatMessage("assistant", aiReply));
                AIReply = aiReply;
            }
            catch (Exception ex)
            {
                // 异常处理：给用户友好提示
                AIReply = $"请求失败：{ex.Message}";
            }
            finally
            {
                // 无论成功/失败，都取消加载状态
                IsSending = false;
            }
        }
    }

}

