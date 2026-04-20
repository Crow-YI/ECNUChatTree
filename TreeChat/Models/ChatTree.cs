using System.ComponentModel;
using System.Runtime.CompilerServices;
using TreeChat.Services;

namespace TreeChat.Models
{
    /// <summary>
    /// 聊天树结构，包含根节点和当前节点等信息
    /// </summary>
    public class ChatTree : INotifyPropertyChanged
    {
        private string _treeTitle = "新对话";

        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性并触发通知
        /// </summary>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否发生了改变</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public ChatTreeNode RootNode { get; private set; }
        public ChatTreeNode CurrentNode { get; private set; }

        /// <summary>
        /// 对话树标题
        /// </summary>
        public string TreeTitle
        {
            get => _treeTitle;
            set => SetProperty(ref _treeTitle, value);
        }

        /// <summary>
        /// API Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// API 端点
        /// </summary>
        public string ApiEndpoint { get; set; }

        /// <summary>
        /// 模型名称
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// 温度参数
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Top P 参数
        /// </summary>
        public double TopP { get; set; }

        /// <summary>
        /// Top K 参数
        /// </summary>
        public int TopK { get; set; }

        /// <summary>
        /// 无参构造函数，用于JSON反序列化
        /// </summary>
        public ChatTree()
        {
            RootNode = new ChatTreeNode(null, new ChatMessage("system", "你是一个有帮助的AI助手。"));
            CurrentNode = RootNode;
            
            // 设置默认配置
            ApiKey = ApiConfig.ApiKey;
            ApiEndpoint = ApiConfig.ApiEndpoint;
            ModelName = ApiConfig.ModelName;
            Temperature = ApiConfig.Temperature;
            TopP = ApiConfig.TopP;
            TopK = ApiConfig.TopK;
        }

        public ChatTree(string? systemPrompt = null, string? apiKey = null, string? apiEndpoint = null, string? modelName = null, double? temperature = null, double? topP = null, int? topK = null)
        {
            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                RootNode = new ChatTreeNode(null, new ChatMessage("system", systemPrompt));
            }
            else
            {
                RootNode = new ChatTreeNode(null, new ChatMessage("system", "你是一个有帮助的AI助手。"));
            }
            CurrentNode = RootNode;
            
            // 设置配置，使用传入的值或默认值
            ApiKey = apiKey ?? ApiConfig.ApiKey;
            ApiEndpoint = apiEndpoint ?? ApiConfig.ApiEndpoint;
            ModelName = modelName ?? ApiConfig.ModelName;
            Temperature = temperature ?? ApiConfig.Temperature;
            TopP = topP ?? ApiConfig.TopP;
            TopK = topK ?? ApiConfig.TopK;
        }

        /// <summary>
        /// 设置根节点（用于从文件加载）
        /// </summary>
        /// <param name="rootNode">新的根节点</param>
        public void SetRootNode(ChatTreeNode rootNode)
        {
            RootNode = rootNode;
            CurrentNode = rootNode;
        }

        private ChatTreeNode? FindNodeById(ChatTreeNode startNode, int nodeID)
        {
            if (startNode.NodeID == nodeID) return startNode;
            foreach (var child in startNode.ChildNodes)
            {
                var found = FindNodeById(child, nodeID);
                if (found != null) return found;
            }
            return null;
        }
    }
}
