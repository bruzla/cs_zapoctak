using System;

namespace DplyrSharp.IO;

// supported types: bool, int, double, datetime, string

public static class TypeInferer
{
    private const int MaxValueCountToInferFrom = 200;

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
