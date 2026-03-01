using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeChat.Models
{
    public class ChatTree
    {
        public ChatTreeNode RootNode { get; }
        public ChatTreeNode CurrentNode { get; private set; }
        public string TreeTitle { get; set; } = "新对话";

        public ChatTree(string? systemPrompt = null)
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
