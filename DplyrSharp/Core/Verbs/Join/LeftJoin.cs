using System;

namespace DplyrSharp.Core;

public partial class DataFrame
{
    public DataFrame LeftJoin(DataFrame right, Func<DataRow, DataRow, bool> predicate)
    {
        return JoinByInternal(this, right, predicate, includeLeftUnmatched: true, includeRightUnmatched: false);
    }
}