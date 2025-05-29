# DplyrSharp User Manual

**DplyrSharp** is a C# library for manipulating tabular data using a fluent and readable syntax.

## API Overview

### 1. Read Csv File

To load data from a CSV file, use:

```csharp
var df = CsvReader.ReadCsv("path/to/file.csv");
```

This returns a `DataFrame` object.

You can optionally pass a `CsvOptions` instance to customize the behavior:

```csharp
var options = new CsvOptions
{
    Delimiter = ';',
    HasHeader = true,
    Encoding = Encoding.UTF8
};

var df = CsvReader.ReadCsv("path/to/file.csv", options);
```

**Options include:**

- `Delimiter`: character that separates fields (default: `,`)
- `HasHeader`: set to `false` if the file has no header row (default: `true`)
- `Encoding`: file encoding (default: UTF-8)

If omitted, default options will be applied.

### 2. Working with DataFrames

#### Accessing Columns and Rows

A `DataFrame` behaves like a table with named columns and typed values. You can inspect its structure or loop through its contents:

```csharp
Console.WriteLine(df.RowCount);        // total rows
Console.WriteLine(df.ColCount);        // total columns
```

To iterate over rows and access values:

```csharp
foreach (var row in df.Rows)
{
    Console.WriteLine(row.Get<string>("name"));
}
```

If you need to examine column metadata or raw access:

```csharp
foreach (var col in df.Columns)
{
    Console.WriteLine($"{col.Name}: {col.ClrType.Name}");
}
```

#### Accessing Values in a Row

Use the `DataRow` struct to retrieve values by column name or index:

```csharp
var row = df.Rows.First();
string name = row.Get<string>("name");          // access by name
int height = row.Get<int>(1);                   // access by index
object? rawValue = row["species_id"];
```

The `Get<T>` method ensures type safety and throws an exception if the column's type doesn't match.

#### Printing a DataFrame

To inspect the contents of a `DataFrame`, use:

```csharp
df.Print();      // Print all rows
df.Print(10);    // Print first 10 rows
```

You can also get a string representation of a given DataFrame object:

```csharp
string preview = df.ToString();
Console.WriteLine(preview);
```

### 3. Verbs

The core functionality of DplyrSharp is exposed through a set of composable verbs that are defined as methods on `DataFrame` objects. These enable chainable data transformations:

#### `Filter(Func<DataRow, bool>)`

Keeps rows that match a predicate.

```csharp
df.Filter(row => row.Get<int>("age") > 30);
```

#### `Select(params string[])`

Keeps only the specified columns.

```csharp
df.Select("name", "country");
```

#### `Mutate(string newCol, Func<DataRow, object>)`

Adds or modifies columns.

```csharp
df.Mutate("is_senior", row => row.Get<int>("age") >= 65);
```

#### `Arrange(Func<DataRow, T>, bool descending = false)`

Sorts rows by a selector.

```csharp
df.Arrange(row => row.Get<string>("country"));
```

#### `GroupBy(params string[])`

Groups data by one or more columns.

```csharp
df.GroupBy("country", "is_senior");
```

#### `Summarise(string name, Func<IEnumerable<DataRow>, T> aggregator)`

Applies aggregation functions to each group.

```csharp
grouped.Summarise(
    ("count", rows => rows.Count()),
    ("avg_age", rows => rows.Average(r => r.Get<int>("age")))
);
```

#### `LeftJoin(DataFrame right, Func<DataRow, DataRow, bool> predicate)`

*Analogously for(`InnerJoin`, `RightJoin`, `FullJoin`)*

Combines two data frames by row-matching predicate.

```csharp
df.LeftJoin(other, (l, r) => l.Get<int>("id") == r.Get<int>("user_id"));
```

All verbs return new `DataFrame` instances, allowing fluent chaining like:

```csharp
df.Filter(...)
  .Mutate(...)
  .GroupBy(...)
  .Summarise(...)
  .Arrange(...);
```

## Example Program

This example shows how to load CSV files, perform grouped aggregation, join data, compute new columns, and sort the results.

```cs
var csvOptions = new CsvOptions { Delimiter = ',', HasHeader = true};                           // read input files

var people = CsvReader.ReadCsv(".../data/people.csv", csvOptions);
var species = CsvReader.ReadCsv(".../data/species.csv", csvOptions);

people
.GroupBy("species_id")                                                                          // group people by "species_id"
.Summarise(
    ("average_height_calculated", rows => rows.Average(r => r.Get<int>("height"))),             // calculate average height
    ("count", rows => rows.Count())                                                             // and count of individuals in each species
)
.LeftJoin(species, (l, r) => l.Get<int>("species_id") == r.Get<int>("id"))                      // attach species info via a "LeftJoin" on "species_id == id"
.Mutate(
    "abs_diff", 
    row => 
        AbsDiff(row.Get<double>("average_height_calculated"), row.Get<int>("average_height"))   // calculate absolute difference between calculated and reported average height
)
.Select("name", "abs_diff", "count")                                                            // keep only selected columns
.Arrange(row => row.Get<double>("abs_diff"), descending: true)                                  // sort by largest absolute difference
.Print(5);                                                                                      // print top 5 rows to standard output
```