namespace Dan.Plugin.Pseudo.Config;

public class Settings
{
    public int DefaultCircuitBreakerOpenCircuitTimeSeconds { get; init; }
    public int DefaultCircuitBreakerFailureBeforeTripping { get; init; }
    public int SafeHttpClientTimeout { get; init; }

    public int KeyCacheTimeToLiveDays { get; init; } = 30;
}
