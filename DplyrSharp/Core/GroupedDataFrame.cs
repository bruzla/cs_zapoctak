using System;

namespace DplyrSharp.Core;

/// <summary>
/// Represents a data frame grouped by one or more columns, enabling group-wise operations.
/// </summary>
public partial class GroupedDataFrame
{
    /// <summary>
    /// Gets the original data frame from which the groups were formed.
    /// </summary>
    internal DataFrame DataFrame { get; init; }


    /// <summary>
    /// Gets the names of the columns used for grouping.
    /// </summary>
    internal string[] GroupColumns { get; init; }

    /// <summary>
    /// Gets the grouped data as a list of key-value pairs, 
    /// where each key is an array of group column values and the value is a list of corresponding data rows.
    /// </summary>
    internal List<(object?[] Key, List<DataRow> Items)> GroupedRows { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupedDataFrame"/> class.
    /// </summary>
    /// <param name="dataFrame">The data frame to group.</param>
    /// <param name="groupColumns">The names of the columns to group by.</param>
    internal GroupedDataFrame(DataFrame dataFrame, params string[] groupColumns)
    {
        DataFrame = dataFrame;
        GroupColumns = groupColumns;
        GroupedRows = BuildGroups(DataFrame, GroupColumns);
    }

    /// <summary>
    /// Builds the grouped row structure by evaluating unique combinations of group column values.
    /// </summary>
    /// <param name="dataFrame">The input data frame.</param>
    /// <param name="groupColumns">The names of the columns to group by.</param>
    /// <returns>A list of groupings as key-value pairs.</returns>
    private static List<(object?[] Key, List<DataRow> Items)> BuildGroups(DataFrame dataFrame, string[] groupColumns)
    {
        var dict = new Dictionary<object?[], List<DataRow>>(new ObjectSequenceComparer());

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

/// <summary>
/// A comparer for arrays of objects that compares element-wise equality and computes hash codes accordingly.
/// Useful for grouping rows by composite keys.
/// </summary>
internal sealed class ObjectSequenceComparer : IEqualityComparer<object?[]>
{
    /// <summary>
    /// Determines whether two object arrays are equal by comparing each element.
    /// </summary>
    /// <param name="x">The first object array.</param>
    /// <param name="y">The second object array.</param>
    /// <returns><c>true</c> if the arrays are element-wise equal; otherwise, <c>false</c>.</returns>
    public bool Equals(object?[]? x, object?[]? y)
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

    /// <summary>
    /// Returns a hash code for the specified object array.
    /// </summary>
    /// <param name="obj">The object array for which to compute the hash code.</param>
    /// <returns>A hash code for the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    public int GetHashCode(object?[] obj)
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