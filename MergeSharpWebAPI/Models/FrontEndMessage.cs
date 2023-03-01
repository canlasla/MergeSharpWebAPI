namespace MergeSharpWebAPI.Models;

public class FrontEndMessage
{
    public FrontEndMessage(object message)
    {
        Message = message;
    }
    public object Message { get; set; }
}
