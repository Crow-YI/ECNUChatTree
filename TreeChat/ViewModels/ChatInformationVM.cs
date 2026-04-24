using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TreeChat.Commands;
using TreeChat.Models;
using TreeChat.Services;
using TreeChat.Views;

namespace TreeChat.ViewModels
{
    /// <summary>
    /// 节点信息VM
    /// </summary>
    public class ChatInformationVM : BaseViewModel
    {
        private string? _userMessage;
        private string? _aiReply;

        public string? UserMessage
        {
            get => _userMessage;
            set => SetProperty(ref _userMessage, value);
        }
        public string? AIReply
        {
            get => _aiReply;
            set => SetProperty(ref _aiReply, value);
        }

        private string _inputMessage;
        public string InputMessage
        {
            get => _inputMessage;
            set
            {
                SetProperty(ref _inputMessage, value);
                SendMessage.OnCanExecuteChanged();
            }
        }

        public AsyncRelayCommand SendMessage { get; }

        private TreeNodeVM? _selectedNode;
        public TreeNodeVM? SelectedNode
        {
            get => _selectedNode;
            set
            {
                _selectedNode = value;
                if(value != null)
                {
                    UserMessage = value.Node.UserMessage.Content;
                    AIReply = value.Node.ReplyMessage?.Content;
                }
                SendMessage.OnCanExecuteChanged();
            }
        }

        private ChatTree? _currentChatTree;
        public ChatTree? CurrentChatTree
        {
            get => _currentChatTree;
            set => SetProperty(ref _currentChatTree, value);
        }

        public event Action<TreeNodeVM, TreeNodeVM>? ChatTreeChanged;

        public ChatInformationVM()
        {
            UserMessage = string.Empty;
            AIReply = string.Empty;

            SendMessage = new AsyncRelayCommand(
                execute: ExecuteSendMessageAsync,
                canExecute: CanExecuteSendMessage);
        }

        private bool CanExecuteSendMessage(object? parameter)
        {
            return !string.IsNullOrEmpty(InputMessage) && SelectedNode != null && CurrentChatTree != null;
        }

        private async Task ExecuteSendMessageAsync(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(InputMessage) || SelectedNode == null || CurrentChatTree == null) return;

            try
            {
                // 检查API Key是否有效
                if (string.IsNullOrWhiteSpace(CurrentChatTree.ApiKey))
                {
                    MessageBox.Show("请更改有效的APIKey值！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 先创建新节点
                ChatTreeNode newNode = new ChatTreeNode(SelectedNode.Node, new ChatMessage("user", InputMessage));
                TreeNodeVM newNodeVM = SelectedNode.AddChild(newNode);
                ChatTreeChanged?.Invoke(SelectedNode, newNodeVM);
                SelectedNode = newNodeVM;

                InputMessage = string.Empty;

                // 调用API
                string aiReply = await OpenAIChat.Instance.CallAiApi(SelectedNode.Node.GetFullContext(), CurrentChatTree);

                // 设置AI回复
                SelectedNode.Node.SetAiReply(new ChatMessage("assistant", aiReply));
                AIReply = aiReply;
            }
            catch (Exception ex)
            {
                // 检查是否是API Key导致的失败
                if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized") || ex.Message.Contains("API Key"))
                {
                    // 如果是API Key错误，移除刚创建的节点
                    if (SelectedNode != null && SelectedNode.ParentNode != null)
                    {
                        var parentNode = SelectedNode.ParentNode;
                        parentNode.RemoveChild(SelectedNode);
                        SelectedNode = parentNode;
                        ChatTreeChanged?.Invoke(SelectedNode, null);
                    }
                    
                    // 显示错误信息
                    MessageBox.Show("请更改有效的APIKey值！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    AIReply = "";
                }
                else
                {
                    // 其他错误，显示错误信息但保留节点
                    AIReply = $"API调用失败：{ex.Message}";
                }
            }
        }
    }
}
