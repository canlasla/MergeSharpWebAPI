using Microsoft.AspNetCore.SignalR.Client;

public class RetryPolicy : IRetryPolicy
{
    private const int ReconnectionWaitSeconds = 5;
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return TimeSpan.FromSeconds(ReconnectionWaitSeconds);
    }
}