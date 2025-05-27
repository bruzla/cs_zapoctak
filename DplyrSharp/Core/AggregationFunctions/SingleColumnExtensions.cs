using System;

namespace DplyrSharp.Core;

public static class SingleColumnExtensions
{
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

    public static double StdDev(this IEnumerable<DataRow> source, Func<DataRow, double> selector)
    {
        return Math.Sqrt(source.Variance(selector));
    }

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

    public static T Mode<T>(this IEnumerable<DataRow> source, Func<DataRow, T> selector)
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

    public static int CountDistinct<T>(this IEnumerable<DataRow> source, Func<DataRow, T> selector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var set = new HashSet<T>();
        foreach (var row in source)
            set.Add(selector(row)!);
        return set.Count;
    }


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
