namespace Anastasya.Metaheuristics.Experiments.Results;

/// <summary>
/// 提供一组数值样本的基本描述统计。
/// </summary>
public sealed class NumericStatistics
{
    internal NumericStatistics(double mean, double median, double minimum, double maximum, double standardDeviation)
    {
        Mean = mean;
        Median = median;
        Minimum = minimum;
        Maximum = maximum;
        StandardDeviation = standardDeviation;
    }

    /// <summary>获取算术平均值。</summary>
    public double Mean { get; }

    /// <summary>获取中位数。</summary>
    public double Median { get; }

    /// <summary>获取最小值。</summary>
    public double Minimum { get; }

    /// <summary>获取最大值。</summary>
    public double Maximum { get; }

    /// <summary>获取以 <c>n - 1</c> 为分母的样本标准差；单样本时为零。</summary>
    public double StandardDeviation { get; }
}

/// <summary>
/// 提供一组持续时间样本的基本描述统计。
/// </summary>
public sealed class DurationStatistics
{
    internal DurationStatistics(
        TimeSpan mean,
        TimeSpan median,
        TimeSpan minimum,
        TimeSpan maximum,
        TimeSpan standardDeviation)
    {
        Mean = mean;
        Median = median;
        Minimum = minimum;
        Maximum = maximum;
        StandardDeviation = standardDeviation;
    }

    /// <summary>获取平均持续时间。</summary>
    public TimeSpan Mean { get; }

    /// <summary>获取持续时间中位数。</summary>
    public TimeSpan Median { get; }

    /// <summary>获取最短持续时间。</summary>
    public TimeSpan Minimum { get; }

    /// <summary>获取最长持续时间。</summary>
    public TimeSpan Maximum { get; }

    /// <summary>获取持续时间的样本标准差；单样本时为零。</summary>
    public TimeSpan StandardDeviation { get; }
}

/// <summary>
/// 汇总 Case 或 Experiment 中各执行状态的 run 数量。
/// </summary>
public sealed class ExperimentRunCounts
{
    internal ExperimentRunCounts(int succeeded, int failed, int canceled, int notStarted)
    {
        Succeeded = succeeded;
        Failed = failed;
        Canceled = canceled;
        NotStarted = notStarted;
    }

    /// <summary>获取计划的 run 总数。</summary>
    public int Total => checked(Succeeded + Failed + Canceled + NotStarted);

    /// <summary>获取成功数量。</summary>
    public int Succeeded { get; }

    /// <summary>获取失败数量。</summary>
    public int Failed { get; }

    /// <summary>获取运行中被取消的数量。</summary>
    public int Canceled { get; }

    /// <summary>获取取消发生时尚未开始的数量。</summary>
    public int NotStarted { get; }
}

/// <summary>
/// 提供一个 Case 中成功运行的指标统计和全部状态计数。
/// </summary>
public sealed class ExperimentStatistics
{
    internal ExperimentStatistics(
        ExperimentRunCounts counts,
        NumericStatistics? bestObjective,
        NumericStatistics? iterations,
        NumericStatistics? evaluations,
        DurationStatistics? duration)
    {
        Counts = counts;
        BestObjective = bestObjective;
        Iterations = iterations;
        Evaluations = evaluations;
        Duration = duration;
    }

    /// <summary>获取各执行状态的 run 数量。</summary>
    public ExperimentRunCounts Counts { get; }

    /// <summary>获取成功 run 的最佳目标值统计；没有成功 run 时为 <see langword="null"/>。</summary>
    public NumericStatistics? BestObjective { get; }

    /// <summary>获取成功 run 的迭代次数统计；没有成功 run 时为 <see langword="null"/>。</summary>
    public NumericStatistics? Iterations { get; }

    /// <summary>获取成功 run 的评估次数统计；没有成功 run 时为 <see langword="null"/>。</summary>
    public NumericStatistics? Evaluations { get; }

    /// <summary>获取成功 run 的持续时间统计；没有成功 run 时为 <see langword="null"/>。</summary>
    public DurationStatistics? Duration { get; }
}

internal static class ExperimentStatisticsCalculator
{
    public static ExperimentStatistics Create(IReadOnlyList<ExperimentRunResult> runs)
    {
        var succeeded = runs.Where(static run => run.Status == ExperimentExecutionStatus.Succeeded).ToArray();
        var counts = CreateCounts(runs);
        if (succeeded.Length == 0)
        {
            return new ExperimentStatistics(counts, null, null, null, null);
        }

        return new ExperimentStatistics(
            counts,
            CreateNumeric(succeeded.Select(static run => run.Summary!.BestEvaluation.Objective)),
            CreateNumeric(succeeded.Select(static run => (double)run.Summary!.Iterations)),
            CreateNumeric(succeeded.Select(static run => (double)run.Summary!.Evaluations)),
            CreateDuration(succeeded.Select(static run => run.Summary!.Duration)));
    }

    public static ExperimentRunCounts CreateCounts(IEnumerable<ExperimentRunResult> runs)
    {
        var succeeded = 0;
        var failed = 0;
        var canceled = 0;
        var notStarted = 0;
        foreach (var run in runs)
        {
            switch (run.Status)
            {
                case ExperimentExecutionStatus.Succeeded:
                    succeeded++;
                    break;
                case ExperimentExecutionStatus.Failed:
                    failed++;
                    break;
                case ExperimentExecutionStatus.Canceled:
                    canceled++;
                    break;
                case ExperimentExecutionStatus.NotStarted:
                    notStarted++;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported experiment execution status.");
            }
        }

        return new ExperimentRunCounts(succeeded, failed, canceled, notStarted);
    }

    private static NumericStatistics CreateNumeric(IEnumerable<double> source)
    {
        var values = source.ToArray();
        Array.Sort(values);
        var mean = values.Average();
        var median = Median(values);
        var variance = values.Length == 1
            ? 0
            : values.Sum(value => Math.Pow(value - mean, 2)) / (values.Length - 1);
        return new NumericStatistics(mean, median, values[0], values[^1], Math.Sqrt(variance));
    }

    private static DurationStatistics CreateDuration(IEnumerable<TimeSpan> source)
    {
        var ticks = source.Select(static duration => (double)duration.Ticks).ToArray();
        var statistics = CreateNumeric(ticks);
        return new DurationStatistics(
            FromTicks(statistics.Mean),
            FromTicks(statistics.Median),
            FromTicks(statistics.Minimum),
            FromTicks(statistics.Maximum),
            FromTicks(statistics.StandardDeviation));
    }

    private static double Median(double[] sortedValues)
    {
        var middle = sortedValues.Length / 2;
        return sortedValues.Length % 2 == 0
            ? (sortedValues[middle - 1] + sortedValues[middle]) / 2
            : sortedValues[middle];
    }

    private static TimeSpan FromTicks(double ticks)
    {
        return TimeSpan.FromTicks(checked((long)Math.Round(ticks, MidpointRounding.AwayFromZero)));
    }
}
