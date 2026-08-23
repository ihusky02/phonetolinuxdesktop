using System.Linq;
using phonetolinux.Models;

namespace PhoneToLinux.Plugins
{
    /// <summary>
    /// Plugin responsible for selecting the default or first conversation in the chat list.
    /// </summary>
    public class ChatSyncPlugin
    {
        public ChatConversationItem? GetDefaultOrFirstConversation(ChatContext context)
        {
            if (context.RecentConversations == null || context.RecentConversations.Count == 0)
                return null;

            // Returns the first available conversation as selected by default
            return context.RecentConversations.FirstOrDefault();
        }
    }
}