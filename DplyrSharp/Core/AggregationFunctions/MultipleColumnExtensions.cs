using System;

namespace DplyrSharp.Core.AggregationFunctions;

/// <summary>
/// Provides aggregation functions over multiple columns derived from <see cref="DataRow"/>s.
/// </summary>
public static class MultipleColumnExtensions
{
    /// <summary>
    /// Computes the weighted average of values in a data frame, where both the value and weight are provided by selectors.
    /// </summary>
    /// <param name="source">The sequence of <see cref="DataRow"/> objects.</param>
    /// <param name="valueSelector">A function to extract the value from each row.</param>
    /// <param name="weightSelector">A function to extract the weight for each value.</param>
    /// <returns>The weighted average of the selected values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any argument is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the sequence is empty or the total weight is zero.</exception>
    public static double WeightedAverage(this IEnumerable<DataRow> source, Func<DataRow, double> valueSelector, Func<DataRow, double> weightSelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));
        if (weightSelector == null) throw new ArgumentNullException(nameof(weightSelector));

        double sumWeighted = 0;
        double sumWeights = 0;
        long count = 0;

        foreach (var row in source)
        {
            double x = valueSelector(row);
            double w = weightSelector(row);
            sumWeighted += x * w;
            sumWeights += w;
            count++;
        }

        if (count == 0)
            throw new InvalidOperationException("Sequence contains no elements");
        if (sumWeights == 0)
            throw new InvalidOperationException("Total weight is zero");

        return sumWeighted / sumWeights;
    }

    /// <summary>
    /// Computes the Pearson correlation coefficient between two columns derived from a data frame.
    /// </summary>
    /// <param name="source">The sequence of <see cref="DataRow"/> objects.</param>
    /// <param name="selectorX">A function to extract the first value (X) from each row.</param>
    /// <param name="selectorY">A function to extract the second value (Y) from each row.</param>
    /// <returns>The Pearson correlation coefficient between X and Y.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any argument is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the sequence is empty or either column has zero variance (making correlation undefined).
    /// </exception>
    public static double Correlation(this IEnumerable<DataRow> source, Func<DataRow, double> selectorX, Func<DataRow, double> selectorY)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selectorX == null) throw new ArgumentNullException(nameof(selectorX));
        if (selectorY == null) throw new ArgumentNullException(nameof(selectorY));

        // First pass: compute means
        double sumX = 0, sumY = 0;
        long count = 0;
        foreach (var row in source)
        {
            sumX += selectorX(row);
            sumY += selectorY(row);
            count++;
        }
        if (count == 0)
            throw new InvalidOperationException("Sequence contains no elements");

        double meanX = sumX / count;
        double meanY = sumY / count;

        // Second pass: compute covariance numerator and variances
        double cov = 0, varX = 0, varY = 0;
        foreach (var row in source)
        {
            double dx = selectorX(row) - meanX;
            double dy = selectorY(row) - meanY;
            cov += dx * dy;
            varX += dx * dx;
            varY += dy * dy;
        }

        if (varX == 0 || varY == 0)
            throw new InvalidOperationException("Variance is zero; correlation undefined");

        return cov / Math.Sqrt(varX * varY);
    }
}
