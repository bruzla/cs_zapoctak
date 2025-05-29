using System;

namespace DplyrSharp.Core;

/// <summary>
/// Provides statistical aggregation methods over a single column of values derived from <see cref="DataRow"/>s.
/// </summary>
public static class SingleColumnExtensions
{
    /// <summary>
    /// Computes the population variance of a numeric column derived from the specified selector.
    /// </summary>
    /// <param name="source">The sequence of <see cref="DataRow"/> objects.</param>
    /// <param name="selector">A function that extracts a numeric value from each row.</param>
    /// <returns>The population variance of the selected values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the sequence is empty.</exception>
    public static double Variance(this IEnumerable<DataRow> source, Func<DataRow, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        // compute mean
        double sum = 0;
        long count = 0;
        foreach (var row in source)
        {
            sum += selector(row);
            count++;
        }

        if (count == 0)
            throw new InvalidOperationException("Sequence contains no elements");

        double mean = sum / count;

        // sum of squared deviations
        double sumSq = 0;
        foreach (var row in source)
        {
            var diff = selector(row) - mean;
            sumSq += diff * diff;
        }

        return sumSq / count;
    }

    /// <summary>
    /// Computes the population standard deviation of a numeric column derived from the specified selector.
    /// </summary>
    /// <param name="source">The sequence of <see cref="DataRow"/> objects.</param>
    /// <param name="selector">A function that extracts a numeric value from each row.</param>
    /// <returns>The population standard deviation.</returns>
    public static double StdDev(this IEnumerable<DataRow> source, Func<DataRow, double> selector)
    {
        return Math.Sqrt(source.Variance(selector));
    }

    /// <summary>
    /// Computes the median of a numeric column derived from the specified selector.
    /// </summary>
    /// <param name="source">The sequence of <see cref="DataRow"/> objects.</param>
    /// <param name="selector">A function that extracts a numeric value from each row.</param>
    /// <returns>The median value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the sequence is empty.</exception>
    public static double Median(this IEnumerable<DataRow> source, Func<DataRow, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        // collect & sort
        var list = source.Select(selector).ToList();
        int n = list.Count;
        if (n == 0)
            throw new InvalidOperationException("Sequence contains no elements");

        list.Sort();
        int mid = n / 2;
        return (n % 2 == 1)
            ? list[mid]
            : (list[mid - 1] + list[mid]) / 2.0;
    }

    /// <summary>
    /// Computes the p-th percentile (0 ≤ p ≤ 1) of a numeric column derived from the selector.
    /// </summary>
    /// <param name="source">The sequence of <see cref="DataRow"/> objects.</param>
    /// <param name="selector">A function that extracts a numeric value from each row.</param>
    /// <param name="p">A percentile value between 0 and 1.</param>
    /// <returns>The percentile value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="p"/> is not between 0 and 1.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the sequence is empty.</exception>
    public static double Percentile(this IEnumerable<DataRow> source, Func<DataRow, double> selector, double p)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (p < 0 || p > 1)
            throw new ArgumentOutOfRangeException(nameof(p), "p must be in [0,1]");

        var list = source.Select(selector).OrderBy(x => x).ToList();
        int n = list.Count;
        if (n == 0)
            throw new InvalidOperationException("Sequence contains no elements");

        int rank = (int)Math.Ceiling(p * n);
        rank = Math.Max(rank, 1);
        rank = Math.Min(rank, n);
        return list[rank - 1];
    }

    /// <summary>
    /// Returns the most frequently occurring value in a column derived from the selector.
    /// </summary>
    /// <typeparam name="T">The type of the values to count. Must be non-nullable.</typeparam>
    /// <param name="source">The sequence of <see cref="DataRow"/> objects.</param>
    /// <param name="selector">A function that extracts the value to count from each row.</param>
    /// <returns>The value that appears most frequently.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the sequence is empty.</exception>
    public static T Mode<T>(this IEnumerable<DataRow> source, Func<DataRow, T> selector) where T : notnull
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var counts = new Dictionary<T, int>();
        foreach (var row in source)
        {
            var v = selector(row)!;
            counts[v] = counts.TryGetValue(v, out var c) ? c + 1 : 1;
        }
        if (counts.Count == 0)
            throw new InvalidOperationException("Sequence contains no elements");

        // pick key with max count
        T best = default!;
        int bestCount = -1;
        foreach (var kv in counts)
        {
            if (kv.Value > bestCount)
            {
                bestCount = kv.Value;
                best = kv.Key!;
            }
        }
        return best!;
    }

    /// <summary>
    /// Counts the number of distinct values in a column derived from the selector.
    /// </summary>
    /// <typeparam name="T">The type of the values to count.</typeparam>
    /// <param name="source">The sequence of <see cref="DataRow"/> objects.</param>
    /// <param name="selector">A function that extracts the value from each row.</param>
    /// <returns>The number of unique values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    public static int CountDistinct<T>(this IEnumerable<DataRow> source, Func<DataRow, T> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var set = new HashSet<T>();
        foreach (var row in source)
            set.Add(selector(row)!);
        return set.Count;
    }


    /// <summary>
    /// Computes the range (maximum - minimum) of a numeric column derived from the selector.
    /// </summary>
    /// <param name="source">The sequence of <see cref="DataRow"/> objects.</param>
    /// <param name="selector">A function that extracts a numeric value from each row.</param>
    /// <returns>The range of the values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the sequence is empty.</exception>
    public static double Range(this IEnumerable<DataRow> source, Func<DataRow, double> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        bool first = true;
        double min = 0;
        double max = 0;
        foreach (var row in source)
        {
            double v = selector(row);
            if (first)
            {
                min = max = v;
                first = false;
            }
            else
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }
        }

        if (first)
            throw new InvalidOperationException("Sequence contains no elements");

        return max - min;
    }
}
