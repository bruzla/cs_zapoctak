using System;

namespace DplyrSharp.IO;

/// <summary>
/// Provides functionality to infer the most appropriate .NET type for a set of string values.
/// </summary>
/// <remarks>
/// Supported types for inference are: <see cref="bool"/>, <see cref="int"/>, <see cref="double"/>, <see cref="DateTime"/>, and <see cref="string"/>.
/// </remarks>
public static class TypeInferer
{
    /// <summary>
    /// The maximum number of non-null values to inspect when inferring a column's type.
    /// </summary>
    private const int MaxValueCountToInferFrom = 200;

    /// <summary>
    /// Infers the most suitable .NET type for a column based on a sample of its values.
    /// </summary>
    /// <param name="values">The sequence of string values to analyze.</param>
    /// <returns>The inferred <see cref="Type"/> (bool, int, double, DateTime, or string).</returns>
    public static Type InferColumnType(IEnumerable<string> values)
    {
        bool isBool = true;
        bool isInt = true;
        bool isDouble = true;
        bool isDateTime = true;

        int counter = 0;
        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value))
                continue;
            if (isBool && !bool.TryParse(value, out _))
                isBool = false;
            if (isInt && !int.TryParse(value, out _))
                isInt = false;
            if (isDouble && !double.TryParse(value, out _))
                isDouble = false;
            if (isDateTime && !DateTime.TryParse(value, out _))
                isDateTime = false;

            counter++;
            if (counter == MaxValueCountToInferFrom)
                break;
        }

        if (isBool)
            return typeof(bool);
        if (isInt)
            return typeof(int);
        if (isDouble)
            return typeof(double);
        if (isDateTime)
            return typeof(DateTime);
        return typeof(string);
    }
}
