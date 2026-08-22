using System.Collections.ObjectModel;

namespace Anastasya.Metaheuristics.Experiments.Configuration;

/// <summary>
/// 表示按声明顺序保存的完整实验案例集合。
/// </summary>
public sealed class ExperimentDefinition
{
    private readonly ReadOnlyCollection<ExperimentCase> _cases;

    /// <summary>
    /// 创建实验定义并复制案例集合。
    /// </summary>
    /// <param name="cases">至少包含一个案例且 ID 互不重复的集合。</param>
    /// <exception cref="ArgumentNullException"><paramref name="cases"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">集合为空、包含空案例或包含重复 ID。</exception>
    public ExperimentDefinition(IEnumerable<ExperimentCase> cases)
    {
        ArgumentNullException.ThrowIfNull(cases);
        var copy = cases.ToArray();
        if (copy.Length == 0)
        {
            throw new ArgumentException("An experiment must contain at least one case.", nameof(cases));
        }

        if (copy.Any(static experimentCase => experimentCase is null))
        {
            throw new ArgumentException("An experiment case cannot be null.", nameof(cases));
        }

        var duplicate = copy
            .GroupBy(static experimentCase => experimentCase.Id, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException(
                $"Experiment case identifiers must be unique; duplicate identifier '{duplicate.Key}'.",
                nameof(cases));
        }

        _cases = Array.AsReadOnly(copy);
    }

    /// <summary>
    /// 获取按声明顺序排列的案例集合。
    /// </summary>
    public IReadOnlyList<ExperimentCase> Cases => _cases;
}
