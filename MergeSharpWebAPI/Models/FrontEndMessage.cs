namespace MergeSharpWebAPI.Models;

public class FrontEndMessage
{
    public FrontEndMessage(object message)
    {
        this.Message = message;
    }
    public object Message { get; set; }
}
