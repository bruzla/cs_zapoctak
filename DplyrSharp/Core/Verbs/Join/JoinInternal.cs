using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    /// <summary>
    /// Performs a general-purpose join between two <see cref="DataFrame"/>s using a row-level predicate.
    /// Supports inner, left outer, right outer, and full outer joins based on flags.
    /// </summary>
    /// <param name="left">The left data frame.</param>
    /// <param name="right">The right data frame.</param>
    /// <param name="predicate">A function that determines whether a pair of rows should be joined.</param>
    /// <param name="includeLeftUnmatched">If <c>true</c>, includes unmatched rows from the left frame (LEFT JOIN).</param>
    /// <param name="includeRightUnmatched">If <c>true</c>, includes unmatched rows from the right frame (RIGHT JOIN).</param>
    /// <returns>A new <see cref="DataFrame"/> containing the join result.</returns>
    private static DataFrame JoinByInternal(DataFrame left, DataFrame right, Func<DataRow, DataRow, bool> predicate, bool includeLeftUnmatched, bool includeRightUnmatched)
    {
        var schema = BuildSchema(left, right);
        var matches = FindMatches(left, right, predicate, includeLeftUnmatched, includeRightUnmatched);
        var resultCols = Materialize(matches, schema);

        return new DataFrame(resultCols);
    }

    /// <summary>
    /// Constructs the schema for the resulting joined data frame by merging column metadata from both inputs.
    /// Adds a <c>_R</c> suffix to right-hand column names if they conflict with left-hand names.
    /// </summary>
    /// <param name="left">The left input data frame.</param>
    /// <param name="right">The right input data frame.</param>
    /// <returns>A list of column definitions representing the output schema.</returns>
    private static List<IDataColumn> BuildSchema(DataFrame left, DataFrame right)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var schema = new List<IDataColumn>();
        foreach (var c in left.Columns)
        {
            seen.Add(c.Name);
            schema.Add(c);
        }
        foreach (var c in right.Columns)
        {
            var alias = seen.Contains(c.Name)
                      ? c.Name + "_R"
                      : c.Name;
            seen.Add(alias);
            schema.Add(new ColumnPlaceholder(c.ClrType, alias, c.Name));
        }

        return schema;
    }

    /// <summary>
    /// Identifies matching row pairs between the left and right data frames based on the join predicate.
    /// Also handles unmatched rows depending on inclusion flags.
    /// </summary>
    /// <param name="left">The left data frame.</param>
    /// <param name="right">The right data frame.</param>
    /// <param name="predicate">A function that returns <c>true</c> if two rows match.</param>
    /// <param name="includeLeftUnmatched">Whether to include unmatched left rows.</param>
    /// <param name="includeRightUnmatched">Whether to include unmatched right rows.</param>
    /// <returns>A list of matched row pairs with optional <c>null</c> entries.</returns>
    private static List<(DataRow L, DataRow? R)> FindMatches(DataFrame left, DataFrame right, Func<DataRow, DataRow, bool> predicate, bool includeLeftUnmatched, bool includeRightUnmatched)
    {
        var matches = new List<(DataRow L, DataRow? R)>();
        var rightMatched = new bool[right.Rows.Count()];
        foreach (var rowL in left.Rows)
        {
            bool any = false;
            foreach (var rowR in right.Rows)
            {
                if (predicate(rowL, rowR))
                {
                    any = true;
                    rightMatched[rowR.RowIndex] = true;
                    matches.Add((rowL, rowR));
                }
            }
            if (!any && includeLeftUnmatched)
                matches.Add((rowL, null));
        }
        if (includeRightUnmatched)
        {
            // add right‐only rows
            foreach (var rowR in right.Rows)
            {
                if (!rightMatched[rowR.RowIndex])
                    matches.Add((new DataRow(left, -1), rowR));
            }
        }

        return matches;
    }

    /// <summary>
    /// Materializes the result of the join operation into concrete <see cref="IDataColumn"/> instances.
    /// Each row in the output corresponds to a matched or unmatched pair.
    /// </summary>
    /// <param name="matches">The list of matched row pairs.</param>
    /// <param name="schema">The schema to use for output column construction.</param>
    /// <returns>A list of populated data columns for the final <see cref="DataFrame"/>.</returns>
    private static List<IDataColumn> Materialize(List<(DataRow L, DataRow? R)> matches, List<IDataColumn> schema)
    {
        int N = matches.Count;
        var resultCols = new List<IDataColumn>(schema.Count);
        foreach (var col in schema)
        {
            if (col is ColumnPlaceholder ph)
            {
                // right‐side
                var arr = Array.CreateInstance(ph.ClrType, N);
                var nulls = new BitArray(N);
                for (int i = 0; i < N; i++)
                {
                    var r = matches[i].R;
                    if (r == null)
                        nulls[i] = true;
                    else
                    {
                        var v = ((DataRow)r)[ph.NameOriginal];
                        if (v == null) nulls[i] = true;
                        else arr.SetValue(v, i);
                    }
                }
                resultCols.Add((IDataColumn)Activator
                    .CreateInstance(
                      typeof(DataColumn<>).MakeGenericType(ph.ClrType),
                      ph.Name, arr, nulls
                    )!);
            }
            else
            {
                // left‐side
                var clrT = col.ClrType;
                var arr = Array.CreateInstance(clrT, N);
                var nulls = new BitArray(N);
                for (int i = 0; i < N; i++)
                {
                    var l = matches[i].L;
                    if (l.RowIndex < 0)  // dummy row for right‐only
                        nulls[i] = true;
                    else
                    {
                        var v = l[col.Name];
                        if (v == null) nulls[i] = true;
                        else arr.SetValue(v, i);
                    }
                }
                resultCols.Add((IDataColumn)Activator
                    .CreateInstance(
                      typeof(DataColumn<>).MakeGenericType(clrT),
                      col.Name, arr, nulls
                    )!);
            }
        }

        return resultCols;
    }

    /// <summary>
    /// Represents a placeholder for a right-side column during join schema construction.
    /// Used to track aliasing and original column names before final materialization.
    /// </summary>
    private class ColumnPlaceholder : IDataColumn
    {
        /// <summary>
        /// Gets the CLR type of the column.
        /// </summary>
        public Type ClrType { get; }

        /// <summary>
        /// Gets the alias used for the column in the join result.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Gets the original name of the column before aliasing.
        /// </summary>
        public string NameOriginal { get; }
        public int RowCount => throw new NotSupportedException();
        public BitArray NullBitmap => throw new NotSupportedException();
        public object? GetValue(int rowIndex) => throw new NotSupportedException();

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnPlaceholder"/> class.
        /// </summary>
        /// <param name="clr">The column's CLR type.</param>
        /// <param name="alias">The name to use in the result.</param>
        /// <param name="original">The original name of the column.</param>
        public ColumnPlaceholder(Type clr, string alias, string original)
        {
            ClrType = clr;
            Name = alias;
            NameOriginal = original;
        }
    }
}
