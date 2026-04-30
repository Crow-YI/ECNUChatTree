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

            // 记录发送前的状态，便于失败后回滚
            // 这里 SelectedNode 在上方已判空，因此使用 null-forgiving 以消除静态分析警告
            TreeNodeVM previousNode = SelectedNode!;
            string messageToSend = InputMessage;
            int? createdNodeId = null;

            try
            {
                // 检查API Key是否有效
                if (string.IsNullOrWhiteSpace(CurrentChatTree.ApiKey))
                {
                    MessageBox.Show("请更改有效的APIKey值！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 先创建新节点
                ChatTreeNode newNode = new ChatTreeNode(previousNode.Node, new ChatMessage("user", messageToSend));
                createdNodeId = newNode.NodeID;
                TreeNodeVM newNodeVM = previousNode.AddChild(newNode);
                ChatTreeChanged?.Invoke(previousNode, newNodeVM);
                SelectedNode = newNodeVM;

                InputMessage = string.Empty;

                // 调用API
                AiCallResult result = await OpenAIChat.Instance.CallAiApi(SelectedNode.Node.GetFullContext(), CurrentChatTree);

                if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Content))
                {
                    // 设置 AI 回复（仅成功时写入树节点，避免错误信息污染上下文）
                    SelectedNode.Node.SetAiReply(new ChatMessage("assistant", result.Content));
                    AIReply = result.Content;
                    return;
                }

                // 失败：弹窗提示，不写入节点回复
                string userPrompt = GetUserFriendlyErrorPrompt(result);
                MessageBox.Show(userPrompt, "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                // 点“确定”后回滚：移除本次新增节点，退回上一个节点，并恢复输入框内容
                if (createdNodeId.HasValue)
                {
                    ChatTreeNode.RollbackNextNodeIdIfLastAllocated(createdNodeId.Value);
                }
                previousNode.RemoveChild(newNodeVM);
                SelectedNode = previousNode;
                InputMessage = messageToSend;
                AIReply = previousNode.Node.ReplyMessage?.Content;
                ChatTreeChanged?.Invoke(previousNode, previousNode);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"请求过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);

                // 异常同样回滚到发送前状态（不污染树节点）
                if (SelectedNode != null && SelectedNode != previousNode)
                {
                    if (createdNodeId.HasValue)
                    {
                        ChatTreeNode.RollbackNextNodeIdIfLastAllocated(createdNodeId.Value);
                    }
                    previousNode.RemoveChild(SelectedNode);
                    SelectedNode = previousNode;
                }
                InputMessage = messageToSend;
                AIReply = previousNode.Node.ReplyMessage?.Content;
                ChatTreeChanged?.Invoke(previousNode, previousNode);
            }
        }

        /// <summary>
        /// 将接口错误结果映射为用户可读中文提示
        /// </summary>
        /// <param name="result">AI 调用结果</param>
        /// <returns>用于弹窗展示的中文提示</returns>
        public static string GetUserFriendlyErrorPrompt(AiCallResult result)
        {
            // HTTP 错误码（文档）：401 / 403 / 422 / 429 / Other
            // 客户端/网络类（非文档 HTTP 码）：Timeout / NetworkError / InvalidResponse / EmptyModelReply / ClientException
            string header = result.ErrorKey switch
            {
                "401" => "令牌无效。",
                "403" => "禁止访问。",
                "422" => "请求体验证失败。",
                "429" => "请求过于频繁。",
                "Other" => "服务器返回了非文档约定的错误状态码。",
                "Timeout" => "请求超时：长时间未收到服务器响应，请检查网络或稍后重试。",
                "NetworkError" => "网络连接失败：无法访问服务器，请检查网络、VPN 或接口地址。",
                "InvalidResponse" => "服务器返回了无法解析的响应（可能不是预期的 JSON 格式）。",
                "EmptyModelReply" => "服务器已响应，但模型返回内容为空。",
                "ClientException" => "客户端处理响应时出错。",
                _ => "发生未知错误。"
            };

            // 将 detail 作为补充信息展示，方便定位问题
            if (!string.IsNullOrWhiteSpace(result.ErrorDetail))
                return $"{header}\n\n详情：{result.ErrorDetail}";

            if (result.StatusCode != null)
                return $"{header}\n\nHTTP 状态码：{(int)result.StatusCode}";

            return header;
        }
    }
}
