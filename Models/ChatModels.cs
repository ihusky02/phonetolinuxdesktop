using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace phonetolinux.Models;

/// <summary>
/// Represents an emoji item in the emoji picker with automatic Twemoji graphic loading and caching.
/// </summary>
public partial class EmojiItemModel : ObservableObject
{
    public string Name { get; set; } = "";

    [ObservableProperty]
    private string _symbol = "";

    public string Shortcode { get; set; } = "";
    public string AssetPath { get; set; } = "";
    public FluentIcons.Common.Symbol FluentSymbol { get; set; } = FluentIcons.Common.Symbol.Emoji;

    private static readonly string CacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".phonetolinux", "cache", "emojis");
    private static readonly HttpClient HttpClient = new();
    private static readonly HashSet<string> Downloading = new();

    public IImage? ImageSource
    {
        get
        {
            try
            {
                var fileName = "";
                if (!string.IsNullOrEmpty(AssetPath))
                {
                    fileName = Path.GetFileName(AssetPath);
                }
                else if (!string.IsNullOrEmpty(Name))
                {
                    fileName = $"{Name}.png";
                }

                if (!string.IsNullOrEmpty(fileName))
                {
                    var diskPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Emojis", fileName);
                    if (File.Exists(diskPath)) return new Bitmap(diskPath);

                    var relativePath = Path.Combine("Assets", "Emojis", fileName);
                    if (File.Exists(relativePath)) return new Bitmap(relativePath);

                    try
                    {
                        return new Bitmap(AssetLoader.Open(new Uri($"avares://phonetolinux/Assets/Emojis/{fileName}")));
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(Symbol))
                {
                    string hexCode = GetEmojiHexCode(Symbol);
                    if (!string.IsNullOrEmpty(hexCode))
                    {
                        Directory.CreateDirectory(CacheDir);
                        string cacheFilePath = Path.Combine(CacheDir, $"{hexCode}.png");
                        if (File.Exists(cacheFilePath))
                        {
                            return new Bitmap(cacheFilePath);
                        }

                        lock (Downloading)
                        {
                            if (!Downloading.Contains(hexCode))
                            {
                                Downloading.Add(hexCode);
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        string url = $"https://cdn.jsdelivr.net/gh/twitter/twemoji@14.0.2/assets/72x72/{hexCode}.png";
                                        var bytes = await HttpClient.GetByteArrayAsync(url);
                                        await File.WriteAllBytesAsync(cacheFilePath, bytes);
                                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                                        {
                                            OnPropertyChanged(nameof(ImageSource));
                                        });
                                    }
                                    catch
                                    {
                                    }
                                    finally
                                    {
                                        lock (Downloading)
                                        {
                                            Downloading.Remove(hexCode);
                                        }
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImageSource Error for {Name}] {ex.Message}");
            }
            return null;
        }
    }

    private static string GetEmojiHexCode(string emoji)
    {
        var codePoints = new List<string>();
        foreach (var rune in emoji.EnumerateRunes())
        {
            int value = rune.Value;
            if (value != 0xFE0F && value != 0x200D)
            {
                codePoints.Add(value.ToString("x"));
            }
            else if (value == 0x200D)
            {
                codePoints.Add("200d");
            }
        }
        return string.Join("-", codePoints);
    }
}

/// <summary>
/// Represents a single chat message within a conversation thread.
/// </summary>
public partial class ChatMessageItem : ObservableObject
{
    /// <summary>
    /// The actual text content of the message.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("text")]
    private string _text = "";

    /// <summary>
    /// Indicates whether the message was sent by the user (true) or received (false).
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("isOutgoing")]
    private bool _isOutgoing = true;

    /// <summary>
    /// Fallback property to handle Android's native SMS database column "body".
    /// Maps the incoming JSON "body" to the "Text" property if it wasn't provided directly.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("body")]
    public string ServerBody 
    { 
        set 
        { 
            if (string.IsNullOrEmpty(Text)) 
            {
                Text = value ?? ""; 
            }
        } 
    }

    /// <summary>
    /// Fallback property to handle Android's native SMS database column "type".
    /// Maps the incoming JSON "type" (1 = Received/Inbox, 2 = Sent/Outbox) to the "IsOutgoing" boolean flag.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("type")]
    public int ServerType 
    { 
        set 
        { 
            IsOutgoing = (value == 2); 
        } 
    }
}

/// <summary>
/// Represents a summarized conversation thread in the recent chats list (left panel).
/// </summary>
public partial class ChatConversationItem : ObservableObject
{
    /// <summary>
    /// Display name of the contact, or the raw phone number if the name is not in the address book.
    /// </summary>
    [ObservableProperty]
    private string _contactName = "";

    /// <summary>
    /// The phone number or alphanumeric sender ID associated with the conversation.
    /// </summary>
    [ObservableProperty]
    private string _phoneNumber = "";

    /// <summary>
    /// Preview snippet of the most recent message in the thread.
    /// </summary>
    [ObservableProperty]
    private string _lastMessage = "";
}
