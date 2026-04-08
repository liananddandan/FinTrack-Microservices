using DotNetCore.CAP;

namespace IdentityService.Tests.Common;

public class FakeCapPublisher : ICapPublisher
{
    public List<(string Topic, object? Message)> PublishedMessages { get; } = new();

    public IServiceProvider ServiceProvider { get; } = new EmptyServiceProvider();

    public ICapTransaction? Transaction { get; set; }

    public Task PublishAsync<T>(
        string name,
        T? value,
        string? callbackName = null,
        CancellationToken cancellationToken = default)
    {
        PublishedMessages.Add((name, value));
        return Task.CompletedTask;
    }

    public Task PublishAsync<T>(
        string name,
        T? value,
        IDictionary<string, string?> headers,
        CancellationToken cancellationToken = default)
    {
        PublishedMessages.Add((name, value));
        return Task.CompletedTask;
    }

    public void Publish<T>(string name, T? contentObj, string? callbackName = null)
    {
        PublishedMessages.Add((name, contentObj));
    }

    public void Publish<T>(string name, T? contentObj, IDictionary<string, string?> headers)
    {
        PublishedMessages.Add((name, contentObj));
    }

    public Task PublishDelayAsync<T>(
        TimeSpan delayTime,
        string name,
        T? contentObj,
        IDictionary<string, string?> headers,
        CancellationToken cancellationToken = default)
    {
        PublishedMessages.Add((name, contentObj));
        return Task.CompletedTask;
    }

    public Task PublishDelayAsync<T>(
        TimeSpan delayTime,
        string name,
        T? contentObj,
        string? callbackName = null,
        CancellationToken cancellationToken = default)
    {
        PublishedMessages.Add((name, contentObj));
        return Task.CompletedTask;
    }

    public void PublishDelay<T>(
        TimeSpan delayTime,
        string name,
        T? contentObj,
        IDictionary<string, string?> headers)
    {
        PublishedMessages.Add((name, contentObj));
    }

    public void PublishDelay<T>(
        TimeSpan delayTime,
        string name,
        T? contentObj,
        string? callbackName = null)
    {
        PublishedMessages.Add((name, contentObj));
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}