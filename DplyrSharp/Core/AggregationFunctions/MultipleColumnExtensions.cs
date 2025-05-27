using System;

namespace DplyrSharp.Core.AggregationFunctions;

public static class MultipleColumnExtensions
{
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
