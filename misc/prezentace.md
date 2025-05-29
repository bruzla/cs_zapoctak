# Presentation

## Slide 1

Title: DplyrSharp
Subtitle: NPRG035 + NPRG038 Final Project

## SLide 2 (Inspiration)

A lil smth about dplyr

example:

```R
result = data %>%
  select(Name, Country, Age) %>%
  filter(Age > 30) %>%
  mutate(IsSenior = Age >= 65) %>%
  group_by(Country, IsSenior) %>%
  summarise(Count = n(), AvgAge = mean(Age)) %>%
  arrange(IsSenior, Country)
```

## Slide 3 (Main Features)

* **Core types**: `DataFrame`, `IDataColumn`, `DataRow`
* **CSV I/O**: type inference, culture options
* **Verbs** :

  * `Filter`, `Select`, `Mutate`, `Arrange` by Func<T, R>
  * `GroupBy` + `Summarise` (built-in: Count, Sum, Avg, Min, Max, Median, Mode, Variance, StdDev, Correlation, WeightedAverage) by IEnumerable<DataRow> aggergator and property selector
  * `InnerJoin`, `LeftJoin`, `RightJoin`, `FullJoin` by row predicate

## Slide 4 (Demonstration)

TO DO

- sample code
- its output

## Slide 5 (High-Level Architecture)

TO DO

## Slide 6 (Hlavní řešené problémy)

TO DO

- algoritmicky: joiny a groupovani prislusnych radku
- strongly typed vs object / boxing vs unboxing
- pridavani vice overloadu kvuli poctu