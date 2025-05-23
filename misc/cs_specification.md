#  NPRG035 + NPRG038: Final Project Specification

## Introduction

The goal of this project is to develop a C# library that provides a domain-specific-language (DSL) for tabular data manipulation. This library is inspired by R’s [dplyr package](https://dplyr.tidyverse.org/) which allows users to manipulate tabular data in a clear and fluent manner.

## Key Features

### Data Frame Object

At the core of the library will be a custom `DataFrame` object. This object will serve as a container for tabular data, similar to a table in a database or a spreadsheet.

The library will include features for:

- **Loading Data**: Methods to import data from CSV files, databases, or other common data sources.
- **Saving Data**: Functionality to export the `DataFrame` to various file formats or persist it in a database.

### Core Functionalities

The library will support the following operations on a `DataFrame`:

- **Select**: Extract specific columns from the table.
- **Filter**: Subset rows based on conditions.
- **Mutate**: Add or transform columns via expressions.
- **Arrange**: Sort rows by one or more columns.
- **Group_By**: Group data by one or more columns.
- **Summarise**: Perform aggregations (e.g., count, average) on groups specified by the **Group_By** verb.
- **Joins**: Merge multiple data frames on key columns.

### Fluent Chaining of Operations

A key focus of this project is ensuring that users can fluently chain multiple operations together in a readable and intuitive manner, similar to how dplyr achieves this in R.

## Desired Example 

Consider an existing `DataFrame` object named `data`, which contains the columns `Name`, `Age`, `Country`.

We aim to perform the following sequence of operations on `data`:

1. Filter rows where `Age > 30`.
1. Select only the `Name` and `Country` columns.
1. Create a new boolean column `IsSenior`, which is `true` if `Age ≥ 65`.
1. Group data by `Country` and `IsSenior`.
1. For each group, compute:
    - The number of rows in the group.
    - The average age in the group.
1. Sort the resulting `DataFrame` by the `Country`.

### Equivalent Code in R’s dplyr

An example of dplyr code in R that executes said sequence of operations:

```R
result = data %>%
  filter(Age > 30) %>%
  select(Name, Country) %>%
  mutate(IsSenior = Age >= 65) %>%
  group_by(Country, IsSenior) %>%
  summarise(Count = n(), AvgAge = mean(Age)) %>%
  arrange(Country)
```

### Equivalent DSL in Our C# Library

Our objective is to achieve a similar DSL in C#, allowing users to chain these operations fluently. The syntax could resemble:

```cs
var result = data
    .Filter(p => p.Age > 30)
    .Select(p => new { p.Name, p.Country })
    .Mutate(p => new { IsSenior = p.Age >= 65 })
    .GroupBy(p => new { p.Country, p.IsSenior })
    .Summarise(g => new { 
        Count = g.Count(), 
        AvgAge = g.Average(p => p.Age) 
    })
    .Arrange(p => p.Country);
```
