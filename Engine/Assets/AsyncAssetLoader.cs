using MyEngine.Threading;

namespace MyEngine.Assets;

/// <summary>
/// Обёртка над JobSystem: выполнить работу в фоне, а результат применить
/// на главном потоке, когда он будет готов.
/// </summary>
public sealed class AsyncAssetLoader<T> where T : class
{
    private readonly JobSystem _jobs;
    private T? _result;
    private Exception? _error;
    private volatile bool _done;

    public AsyncAssetLoader(JobSystem jobs) => _jobs = jobs;

    public bool IsDone => _done;
    public T? Result => _result;
    public Exception? Error => _error;

    public void Start(Func<T> work)
    {
        _done = false;
        _result = null;
        _error = null;

        _jobs.Enqueue(() =>
        {
            try { _result = work(); }
            catch (Exception ex) { _error = ex; }
            finally { _done = true; }
        });
    }
}