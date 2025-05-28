using System.Collections;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    private const int DummyValue = -1;
    private static DataFrame JoinByInternal(DataFrame left, DataFrame right, Func<DataRow, DataRow, bool> predicate, bool includeLeftUnmatched, bool includeRightUnmatched)
    {
        var schema = BuildSchema(left, right);
        var matches = FindMatches(left, right, predicate, includeLeftUnmatched, includeRightUnmatched);
        var resultCols = Materialize(matches, schema);

        return new DataFrame(resultCols);
    }

    // build the result‐schema: left cols, then right cols - _R on collisions
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

    // gather all matching (L,R) pairs and track them
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
                    matches.Add((new DataRow(left, DummyValue), rowR));
            }
        }

        return matches;
    }

    // materialise each column in schema order
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

    // placeholder for right‐side metadata
    private class ColumnPlaceholder : IDataColumn
    {
        public Type ClrType { get; }
        public string Name { get; }
        public string NameOriginal { get; }
        public int RowCount => throw new NotSupportedException();
        public BitArray NullBitmap => throw new NotSupportedException();
        public object? GetValue(int rowIndex) => throw new NotSupportedException();
        public ColumnPlaceholder(Type clr, string alias, string original)
        {
            ClrType = clr;
            Name = alias;
            NameOriginal = original;
        }
    }
}
