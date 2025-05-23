using System.Globalization;
using DplyrSharp.IO;

namespace tests;

// supported types: bool, int, double, datetime, string

public class TypeInfererTests
{
    public TypeInfererTests()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
    }

    [Theory]
    [InlineData("True")]
    [InlineData("False")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("True", "true", "False", "false")]
    [InlineData("False", "false", "True", "true")]
    public void InfersBool(params string[] values)
    {
        Type result = TypeInferer.InferColumnType(values);
        Assert.Equal(typeof(bool), result);
    }

    [Theory]
    [InlineData("2147483647")]
    [InlineData("-2147483648")]
    [InlineData("+13230")]
    [InlineData("-0")]
    [InlineData("55", "20", "78", "2147483647")]
    [InlineData("1", "0", "-42", "999")]
    public void InfersInt(params string[] values)
    {
        Type result = TypeInferer.InferColumnType(values);
        Assert.Equal(typeof(int), result);
    }

    [Theory]
    [InlineData("2.5")]
    [InlineData("-15635.1")]
    [InlineData("0.0")]
    [InlineData("-35.7")]
    [InlineData("55", "20", "78", "2147483647", "2.5", "1")]
    [InlineData("1", "0", "-42", "999", "1.2", "NaN")]
    public void InfersDouble(params string[] values)
    {
        Type result = TypeInferer.InferColumnType(values);
        Assert.Equal(typeof(double), result);
    }

    [Theory]
    [InlineData("2023-05-21")]
    [InlineData("01/01/2000")]
    [InlineData("12/31/1999", "01/01/2000", "06/15/2021")]
    [InlineData("2020-01-01T00:00:00", "2020-12-31T23:59:59")]
    [InlineData("2024-06-01 14:30:00")]
    [InlineData("2022-10-12", "2022-10-13", "2022-10-14")]
    [InlineData("15 Aug 2022", "16 Aug 2022", "17 Aug 2022")]
    [InlineData("March 5, 2020", "March 6, 2020")]
    public void InfersDateTime(params string[] values)
    {
        Type result = TypeInferer.InferColumnType(values);
        Assert.Equal(typeof(DateTime), result);
    }

    [Theory]
    [InlineData("True", "true", "False", "false", "hello world")]
    [InlineData("55", "20", "78", "2147483647", "hello world")]
    [InlineData("1", "0", "-42", "999", "1.2", "NaN", "hello world")]
    [InlineData("15 Aug 2022", "16 Aug 2022", "17 Aug 2022", "hello world")]
    public void FallsBackToString(params string[] values)
    {
        Type result = TypeInferer.InferColumnType(values);
        Assert.Equal(typeof(string), result);
    }
}
