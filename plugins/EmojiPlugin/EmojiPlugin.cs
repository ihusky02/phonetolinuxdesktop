using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PhoneToLinux.Core;

namespace phonetolinux.Plugins
{
    /// <summary>
    /// Android-compatible Emoji Processing and Rendering Engine plugin (.dll).
    /// Dynamically discovers and loads emoji graphic assets from Assets/Emojis/ directory,
    /// mapping file shortcodes to corresponding Unicode symbols and avares asset paths.
    /// </summary>
    public class EmojiPlugin : IPhonePlugin
    {
        private static readonly string UserConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux");
        private static readonly string CustomEmojiFilePath = Path.Combine(UserConfigDir, "emoji.json");

        public string Endpoint => "/emoji";

        private Dictionary<string, string> _emojiMap = new();
        private List<EmojiAssetItem> _emojiAssets = new();

        public class EmojiAssetItem
        {
            public string Name { get; set; } = "";
            public string Shortcode { get; set; } = "";
            public string Unicode { get; set; } = "";
            public string AssetPath { get; set; } = "";
        }

        public class EmojiJsonItem
        {
            [System.Text.Json.Serialization.JsonPropertyName("emoji")]
            public string Emoji { get; set; } = "";

            [System.Text.Json.Serialization.JsonPropertyName("description")]
            public string Description { get; set; } = "";

            [System.Text.Json.Serialization.JsonPropertyName("category")]
            public string Category { get; set; } = "";

            [System.Text.Json.Serialization.JsonPropertyName("aliases")]
            public List<string> Aliases { get; set; } = new();

            [System.Text.Json.Serialization.JsonPropertyName("tags")]
            public List<string> Tags { get; set; } = new();
        }

        public EmojiPlugin()
        {
            InitializeDefaultEmojis();
            LoadEmojiJson();
            LoadCustomEmojis();
        }

        private void InitializeDefaultEmojis()
        {
            var defaults = new Dictionary<string, (string unicode, string filename)>
            {
                { ":smile:", ("😄", "smile.png") },
                { ":blush:", ("😊", "blush.png") },
                { ":laugh:", ("😂", "laugh.png") },
                { ":heart:", ("❤️", "heart.png") },
                { ":thumbsup:", ("👍", "thumbsup.png") },
                { ":fire:", ("🔥", "fire.png") },
                { ":rocket:", ("🚀", "rocket.png") },
                { ":dog:", ("🐶", "dog.png") },
                { ":sparkles:", ("✨", "sparkles.png") },
                { ":shiba_happy:", ("🐕😊", "dog.png") },
                { ":shiba_love:", ("🐶💖", "heart.png") },
                { ":shiba_cool:", ("😎🐕", "dog.png") },
                { ":shiba_sleep:", ("💤🐶", "dog.png") },
                { ":shiba_wow:", ("😲🐕", "dog.png") },
                { ":shiba_paws:", ("🐾🐾", "sparkles.png") }
            };

            foreach (var kvp in defaults)
            {
                _emojiMap[kvp.Key] = kvp.Value.unicode;
                string assetPath = $"avares://phonetolinux/Assets/Emojis/{kvp.Value.filename}";
                _emojiAssets.Add(new EmojiAssetItem
                {
                    Name = kvp.Key.Trim(':'),
                    Shortcode = kvp.Key,
                    Unicode = kvp.Value.unicode,
                    AssetPath = assetPath
                });
            }
        }

        private void LoadEmojiJson()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emoji.json");
                if (!File.Exists(jsonPath))
                {
                    jsonPath = Path.Combine(Environment.CurrentDirectory, "emoji.json");
                }

                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    var list = JsonSerializer.Deserialize<List<EmojiJsonItem>>(json);
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            if (!string.IsNullOrEmpty(item.Emoji))
                            {
                                if (item.Aliases != null)
                                {
                                    foreach (var alias in item.Aliases)
                                    {
                                        string shortcode = $":{alias}:";
                                        _emojiMap[shortcode] = item.Emoji;
                                    }
                                }

                                if (!string.IsNullOrEmpty(item.Description))
                                {
                                    string descShortcode = $":{item.Description.Replace(" ", "_").Replace("-", "_")}:";
                                    _emojiMap[descShortcode] = item.Emoji;
                                }

                                string primaryName = (item.Aliases != null && item.Aliases.Count > 0) ? item.Aliases[0] : item.Description;
                                string primaryShortcode = (item.Aliases != null && item.Aliases.Count > 0) ? $":{item.Aliases[0]}:" : "";

                                string filename = "smile.png";
                                if (primaryName == "smile" || primaryName == "grinning") filename = "smile.png";
                                else if (primaryName == "blush" || primaryName == "smiling_face_with_smiling_eyes") filename = "blush.png";
                                else if (primaryName == "laugh" || primaryName == "joy" || primaryName == "face_with_tears_of_joy") filename = "laugh.png";
                                else if (primaryName == "heart" || primaryName == "red_heart") filename = "heart.png";
                                else if (primaryName == "thumbsup") filename = "thumbsup.png";
                                else if (primaryName == "fire") filename = "fire.png";
                                else if (primaryName == "rocket") filename = "rocket.png";
                                else if (primaryName == "dog") filename = "dog.png";
                                else if (primaryName == "sparkles") filename = "sparkles.png";

                                string assetPath = $"avares://phonetolinux/Assets/Emojis/{filename}";

                                if (!string.IsNullOrEmpty(primaryShortcode) && !_emojiAssets.Exists(a => a.Shortcode == primaryShortcode))
                                {
                                    _emojiAssets.Add(new EmojiAssetItem
                                    {
                                        Name = primaryName,
                                        Shortcode = primaryShortcode,
                                        Unicode = item.Emoji,
                                        AssetPath = assetPath
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmojiPlugin Warning] Failed to load emoji.json: {ex.Message}");
            }
        }

        private void LoadCustomEmojis()
        {
            try
            {
                if (File.Exists(CustomEmojiFilePath))
                {
                    string json = File.ReadAllText(CustomEmojiFilePath);
                    var customMap = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (customMap != null)
                    {
                        foreach (var kvp in customMap)
                        {
                            _emojiMap[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmojiPlugin Warning] Failed to load custom emojis: {ex.Message}");
            }
        }

        public string Execute(string queryParams)
        {
            if (string.IsNullOrEmpty(queryParams))
            {
                return JsonSerializer.Serialize(new
                {
                    engine = "Android Emoji Asset-Based Engine (PhoneToLinux .DLL)",
                    version = "1.0.4",
                    assets = _emojiAssets,
                    emojis = _emojiMap
                });
            }

            if (queryParams.Equals("reload", StringComparison.OrdinalIgnoreCase) || queryParams.Equals("action=reload", StringComparison.OrdinalIgnoreCase))
            {
                LoadCustomEmojis();
                return JsonSerializer.Serialize(new { status = "reloaded", totalEmojis = _emojiMap.Count });
            }

            if (queryParams.Equals("list", StringComparison.OrdinalIgnoreCase) || queryParams.Equals("action=list", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Serialize(_emojiMap);
            }

            if (queryParams.Equals("assets", StringComparison.OrdinalIgnoreCase) || queryParams.Equals("action=assets", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Serialize(_emojiAssets);
            }

            // Android-style emoji processing: replace text shortcodes with authentic Unicode emojis
            string processedText = queryParams;
            foreach (var kvp in _emojiMap)
            {
                processedText = processedText.Replace(kvp.Key, kvp.Value);
            }

            return processedText;
        }
    }
}
