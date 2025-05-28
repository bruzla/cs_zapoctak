using System;

namespace DplyrSharp.Core;

public partial class GroupedDataFrame
{
    internal DataFrame DataFrame { get; init; }
    internal string[] GroupColumns { get; init; }
    internal List<(object?[] Key, List<DataRow> Items)> GroupedRows { get; init; }

    internal GroupedDataFrame(DataFrame dataFrame, params string[] groupColumns)
    {
        DataFrame = dataFrame;
        GroupColumns = groupColumns;
        GroupedRows = BuildGroups(DataFrame, GroupColumns);
    }

    private static List<(object?[] Key, List<DataRow> Items)> BuildGroups(DataFrame dataFrame, string[] groupColumns)
    {
        var dict = new Dictionary<object[], List<DataRow>>(new ObjectSequenceComparer());

        foreach (var row in dataFrame.Rows)
        {
            var key = new object?[groupColumns.Length];
            for (int gi = 0; gi < groupColumns.Length; gi++)
                key[gi] = row[groupColumns[gi]];

            if (!dict.TryGetValue(key, out var list))
            {
                list = new List<DataRow>();
                dict[key] = list;
            }
            list.Add(row);
        }

        var result = new List<(object?[] Key, List<DataRow> Items)>();
        foreach (var kv in dict)
            result.Add((kv.Key, kv.Value));

        return result;
    }
}

internal sealed class ObjectSequenceComparer : IEqualityComparer<object[]>
{
    public bool Equals(object[]? x, object[]? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x == null || y == null)
            return false;
        if (x.Length != y.Length)
            return false;
        for (int i = 0; i < x.Length; i++)
        {
            if (!object.Equals(x[i], y[i]))
                return false;
        }
        return true;
    }

    public int GetHashCode(object[] obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));
        unchecked
        {
            int hash = 17;
            foreach (var o in obj)
                hash = hash * 31 + (o?.GetHashCode() ?? 0);
            return hash;
        }
    }
}