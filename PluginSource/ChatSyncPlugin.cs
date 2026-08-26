using System;
using System.Collections.Generic;
using System.Linq;
using phonetolinux.Models;

namespace PhoneToLinux.Plugins
{
    /// <summary>
    /// Plugin responsible for selecting, filtering, and synchronizing 
    /// active conversations in the chat list.
    /// </summary>
    public class ChatSyncPlugin
    {
        /// <summary>
        /// Retrieves the default or first available conversation from the context,
        /// ensuring thread deduplication.
        /// </summary>
        /// <param name="context">The active chat context containing recent conversations.</param>
        /// <returns>The first valid conversation item, or null if empty.</returns>
        public ChatConversationItem? GetDefaultOrFirstConversation(ChatContext context)
        {
            if (context?.RecentConversations == null || context.RecentConversations.Count == 0)
                return null;

            // Filter out potential duplicate entries by phone number/address before selecting
            var uniqueConversations = DeduplicateConversations(context.RecentConversations);
            return uniqueConversations.FirstOrDefault();
        }

        /// <summary>
        /// Filters a collection of conversation items by unique phone number/address.
        /// </summary>
        /// <param name="conversations">Input list of conversations.</param>
        /// <returns>Deduplicated list of conversation items.</returns>
        public List<ChatConversationItem> DeduplicateConversations(IEnumerable<ChatConversationItem> conversations)
        {
            if (conversations == null)
                return new List<ChatConversationItem>();

            return conversations
                .Where(c => !string.IsNullOrWhiteSpace(c.PhoneNumber ?? c.Address))
                .DistinctBy(c => NormalizePhoneNumber(c.PhoneNumber ?? c.Address ?? ""))
                .ToList();
        }

        /// <summary>
        /// Normalizes phone numbers to standard 9-digit format for robust comparison.
        /// </summary>
        private static string NormalizePhoneNumber(string number)
        {
            var digits = new string(number.Where(char.IsDigit).ToArray());
            return digits.Length > 9 ? digits.Substring(digits.Length - 9) : digits;
        }
    }
}