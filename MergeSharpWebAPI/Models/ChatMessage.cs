namespace MergeSharpWebAPI.Models;

public class ChatMessage
{
    public ChatMessage(string user, string message)
    {
        this.User = user;
        this.Message = message;
    }
    public string User { get; set; }

    public string Message { get; set; }
}
