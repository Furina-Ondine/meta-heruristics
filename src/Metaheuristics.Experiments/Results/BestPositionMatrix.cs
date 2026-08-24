namespace Anastasya.Metaheuristics.Experiments.Results;

/// <summary>
/// 提供 Case 中各成功 run 最佳位置的只读二维视图。
/// </summary>
/// <remarks>矩阵只由执行器在构造结果期间写入；公开实例完成构造后仅供并发读取。</remarks>
public sealed class BestPositionMatrix
{
    private readonly double[,] _positions;
    private readonly bool[] _hasPosition;

    internal BestPositionMatrix(int runCount, int dimension)
    {
        _positions = new double[runCount, dimension];
        _hasPosition = new bool[runCount];
    }

    /// <summary>
    /// 获取矩阵中的 run 行数。
    /// </summary>
    public int RunCount => _positions.GetLength(0);

    /// <summary>
    /// 获取每个最佳位置的维度。
    /// </summary>
    public int Dimension => _positions.GetLength(1);

    /// <summary>
    /// 获取指定成功 run 的一个位置分量。
    /// </summary>
    /// <param name="repetitionIndex">从零开始的 Repetition 下标。</param>
    /// <param name="dimensionIndex">从零开始的位置维度下标。</param>
    /// <returns>指定位置分量。</returns>
    /// <exception cref="ArgumentOutOfRangeException">任一下标越界。</exception>
    /// <exception cref="InvalidOperationException">指定 run 没有成功结果。</exception>
    public double this[int repetitionIndex, int dimensionIndex]
    {
        get
        {
            ValidateRepetitionIndex(repetitionIndex);
            ArgumentOutOfRangeException.ThrowIfNegative(dimensionIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(dimensionIndex, Dimension);
            EnsurePositionExists(repetitionIndex);
            return _positions[repetitionIndex, dimensionIndex];
        }
    }

    /// <summary>
    /// 判断指定 run 是否保存了成功结果的最佳位置。
    /// </summary>
    /// <param name="repetitionIndex">从零开始的 Repetition 下标。</param>
    /// <returns>存在有效最佳位置时返回 <see langword="true"/>。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="repetitionIndex"/> 越界。</exception>
    public bool HasPosition(int repetitionIndex)
    {
        ValidateRepetitionIndex(repetitionIndex);
        return _hasPosition[repetitionIndex];
    }

    /// <summary>
    /// 将指定成功 run 的完整最佳位置复制到调用方缓冲区。
    /// </summary>
    /// <param name="repetitionIndex">从零开始的 Repetition 下标。</param>
    /// <param name="destination">长度必须等于 <see cref="Dimension"/> 的目标缓冲区。</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="repetitionIndex"/> 越界。</exception>
    /// <exception cref="ArgumentException">目标缓冲区长度不匹配。</exception>
    /// <exception cref="InvalidOperationException">指定 run 没有成功结果。</exception>
    public void CopyPositionTo(int repetitionIndex, Span<double> destination)
    {
        ValidateRepetitionIndex(repetitionIndex);
        if (destination.Length != Dimension)
        {
            throw new ArgumentException("The destination length must match the position dimension.", nameof(destination));
        }

        EnsurePositionExists(repetitionIndex);
        for (var dimensionIndex = 0; dimensionIndex < Dimension; dimensionIndex++)
        {
            destination[dimensionIndex] = _positions[repetitionIndex, dimensionIndex];
        }
    }

    internal void SetPosition(int repetitionIndex, ReadOnlySpan<double> position)
    {
        if (position.Length != Dimension)
        {
            throw new ArgumentException("The position length must match the matrix dimension.", nameof(position));
        }

        for (var dimensionIndex = 0; dimensionIndex < Dimension; dimensionIndex++)
        {
            _positions[repetitionIndex, dimensionIndex] = position[dimensionIndex];
        }

        _hasPosition[repetitionIndex] = true;
    }

    private void ValidateRepetitionIndex(int repetitionIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(repetitionIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(repetitionIndex, RunCount);
    }

    private void EnsurePositionExists(int repetitionIndex)
    {
        if (!_hasPosition[repetitionIndex])
        {
            throw new InvalidOperationException("The requested run does not have a successful best position.");
        }
    }
}
