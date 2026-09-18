using MyEngine.Diagnostics;

namespace MyEngine.Threading;

public readonly struct JobHandle
{
    internal readonly JobNode? Node;
    internal JobHandle(JobNode node) => Node = node;
    public bool IsValid => Node != null;
}

public sealed class JobNode
{
    public Action Work = null!;
    internal int PendingDeps;
    internal readonly List<JobNode> Dependents = new();
    internal readonly ManualResetEventSlim Done = new(false);
}

public sealed class JobGraph
{
    private readonly JobSystem _jobs;
    private readonly List<JobNode> _nodes = new();
    private int _pending;

    public JobGraph(JobSystem jobs) => _jobs = jobs;

    public JobHandle Add(Action work, params JobHandle[] dependsOn)
    {
        var node = new JobNode { Work = work };
        node.PendingDeps = dependsOn.Length;

        foreach (var dep in dependsOn)
        {
            if (dep.Node == null) continue;
            dep.Node.Dependents.Add(node);
        }

        _nodes.Add(node);

        if (node.PendingDeps == 0)
            Kick(node);

        return new JobHandle(node);
    }

    public void WaitAll()
    {
        foreach (var n in _nodes) n.Done.Wait();
    }

    private void Kick(JobNode node)
    {
        Interlocked.Increment(ref _pending);
        _jobs.Enqueue(() =>
        {
            try { node.Work(); }
            catch (Exception ex) { Log.Error("[JobGraph]",ex.Message); }
            finally
            {
                node.Done.Set();
                foreach (var dep in node.Dependents)
                {
                    if (Interlocked.Decrement(ref dep.PendingDeps) == 0)
                        Kick(dep);
                }
            }
        });
    }
}