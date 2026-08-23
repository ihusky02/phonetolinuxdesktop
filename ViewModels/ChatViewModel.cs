public async Task LoadConversationsAndSyncAsync()
    {
        try
        {
            var phoneConversations = await _conversationsService.GetConversationsFromServerAsync();
            var list = new List<ChatConversationItem>();

            if (phoneConversations != null && phoneConversations.Count > 0)
            {
                var uniqueConversations = phoneConversations
                    .GroupBy(c => (c.contactName ?? c.phoneNumber)?.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First());

                foreach (var conv in uniqueConversations)
                {
                    list.Add(new ChatConversationItem 
                    { 
                        ContactName = string.IsNullOrEmpty(conv.contactName) ? conv.phoneNumber : conv.contactName, 
                        PhoneNumber = conv.phoneNumber ?? "", 
                        LastMessage = conv.lastMessage 
                    });
                }
            }
            else
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string storageDir = Path.Combine(homeDir, ".phonetolinux", "chats");
                
                if (Directory.Exists(storageDir))
                {
                    var files = Directory.GetFiles(storageDir, "*.json");
                    foreach (var file in files)
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        list.Add(new ChatConversationItem { ContactName = name, PhoneNumber = name, LastMessage = "Saved history" });
                    }
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                RecentConversations.Clear();
                foreach (var item in list.Take(6))
                {
                    RecentConversations.Add(item);
                }

                if (SelectedConversation == null && RecentConversations.Count > 0)
                {
                    SelectedConversation = RecentConversations[0];
                }
            });
        }
        catch 
        {
            // Silently handle exception
        }
    }