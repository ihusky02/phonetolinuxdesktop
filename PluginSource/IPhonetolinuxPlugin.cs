using System;
using System.Threading.Tasks;

namespace phonetolinux.Plugins;

public interface IPhonetolinuxPlugin
{
    string Name { get; }
    string Version { get; }
    
    void Initialize(string phoneIp, int port = 5000);
    void Shutdown();

    event Action<string, string>? OnCallReceived; // number, callerName
    event Action? OnCallEnded;
    event Action<string, string>? OnSmsReceived;  // sender, message

    Task<bool> AnswerCallAsync();
    Task<bool> RejectCallAsync();
}