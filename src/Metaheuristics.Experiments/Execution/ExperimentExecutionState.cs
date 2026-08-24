namespace Anastasya.Metaheuristics.Experiments.Execution;

/// <summary>保存整个 Experiment 的并发执行状态。</summary>
internal sealed class ExperimentExecutionState
{
    private int _startedGroupCount;

    public int StartedGroupCount => Volatile.Read(ref _startedGroupCount);

    public void MarkGroupStarted() => Interlocked.Increment(ref _startedGroupCount);
}
