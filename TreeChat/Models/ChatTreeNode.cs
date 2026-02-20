namespace TreeChat.Models
{
    public class ChatTreeNode
    {
        public ChatTreeNode? ParentNode { get; }
        public List<ChatTreeNode> ChildNodes { get; } = new List<ChatTreeNode>();
        public ChatMessage UserMessage { get; }
        public ChatMessage? ReplyMessage { get; private set; }
        public int NodeID { get; }

        private static int _nextNodeID = 1;

        public ChatTreeNode(ChatTreeNode? parentNode, ChatMessage userMessage)
        {
            ParentNode = parentNode;
            UserMessage = userMessage;
            NodeID = _nextNodeID++;
        }

        public List<ChatMessage> GetFullContext()
        {
            var context = new List<ChatMessage>();
            var currentNode = this;

            while (currentNode != null)
            {
                if (currentNode.ReplyMessage != null && !string.IsNullOrEmpty(currentNode.ReplyMessage.Content))
                    context.Add(currentNode.ReplyMessage);

                if (!string.IsNullOrEmpty(currentNode.UserMessage.Content))
                    context.Add(currentNode.UserMessage);

                currentNode = currentNode.ParentNode;
            }

            context.Reverse();
            return context;
        }

        public ChatTreeNode AddChildNode(ChatMessage userMessage)
        {
            var childNode = new ChatTreeNode(this, userMessage);
            ChildNodes.Add(childNode);
            return childNode;
        }

        public void SetAiReply(ChatMessage replyMessage)
        {
            ReplyMessage = replyMessage;
        }
    }
}