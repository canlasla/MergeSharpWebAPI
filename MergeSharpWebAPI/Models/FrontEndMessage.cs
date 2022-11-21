namespace MergeSharpWebAPI.Models;

public class FrontEndMessage
{
    public FrontEndMessage(string message)
    {
        this.Message = message;
    }
    public string Message { get; set; }
}
