using System.Linq;
using phonetolinux.Models;

namespace PhoneToLinux.Plugins
{
    /// <summary>
    /// Wtyczka odpowiedzialna za wybór domyślnej lub pierwszej konwersacji na liście czatów.
    /// </summary>
    public class ChatSyncPlugin
    {
        public ChatConversationItem? GetDefaultOrFirstConversation(ChatContext context)
        {
            if (context.RecentConversations == null || context.RecentConversations.Count == 0)
                return null;

            // Zwraca pierwszą dostępną konwersację jako domyślnie zaznaczoną
            return context.RecentConversations.FirstOrDefault();
        }
    }
}