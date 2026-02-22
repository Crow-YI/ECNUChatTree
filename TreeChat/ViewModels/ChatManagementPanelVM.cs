using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeChat.Commands;
using TreeChat.Models;

namespace TreeChat.ViewModels
{
    public class ChatManagementPanelVM : BaseViewModel
    {
        private ObservableCollection<ChatTree> _chatList;
        private ChatTree? _selectedChat;

        public ObservableCollection<ChatTree> ChatList
        {
            get => _chatList;
        }

        public ChatTree? SelectedChat
        {
            get => _selectedChat;
            set
            {
                SetProperty(ref _selectedChat, value);
                if (value != null)
                    SelectedChatChanged?.Invoke(value);
            }
        }

        public RelayCommand CreateNewChat { get; }

        public event Action<ChatTree>? SelectedChatChanged;

        public ChatManagementPanelVM()
        {
            _chatList = new ObservableCollection<ChatTree>();

            CreateNewChat = new RelayCommand(ExecuteCreateNewChat);
        }

        private void ExecuteCreateNewChat(object? parameter)
        {
            ChatTree newTree = new ChatTree();
            ChatList.Add(newTree);
            SelectedChat = newTree;
        }
    }
}
