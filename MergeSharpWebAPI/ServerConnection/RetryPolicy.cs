using Microsoft.AspNetCore.SignalR.Client;

namespace MergeSharpWebAPI.ServerConnection;

public class RetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        const int ReconnectionWaitSeconds = 5;
        return TimeSpan.FromSeconds(ReconnectionWaitSeconds);
    }
}
