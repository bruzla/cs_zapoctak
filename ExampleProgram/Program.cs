using System;
using DplyrSharp.Core;
using DplyrSharp.IO;
using System.Globalization;

namespace ExampleProgram
{
    class Program
    {
        static bool IsStandardGender(string gender)
        {
            return gender == "male" || gender == "female";
        }
        static double CalculateBMI(double height, double mass)
        {
            double adjustedHeight = height / 100.0;
            return mass / (adjustedHeight * adjustedHeight);
        }

        static string BracketizeAge(int age)
        {
            if (age < 30)
                return "Young";
            else if (age < 60)
                return "Adult";
            else
                return "Senior";
        }

        static double AbsDiff(double x, double y)
        {
            return Math.Abs(x - y);
        }

        static void Main()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

            var people = CsvReader.ReadCsv("C:/Users/bruzl/Desktop/cs_zapoctak/misc/data/people.csv");
            var planets = CsvReader.ReadCsv("C:/Users/bruzl/Desktop/cs_zapoctak/misc/data/planets.csv");
            var species = CsvReader.ReadCsv("C:/Users/bruzl/Desktop/cs_zapoctak/misc/data/species.csv");

            // People statistics

            var peopleBmiStatistics = people
                .Filter(row => row.Get<int>("age") > 18 && IsStandardGender(row.Get<string>("gender")))
                .Mutate(("bmi", row => CalculateBMI(row.Get<int>("height"), row.Get<double>("mass"))),
                        ("age_bracket", row => BracketizeAge(row.Get<int>("age"))))
                .GroupBy("gender", "age_bracket")
                .Summarise(("variance_bmi", rows => rows.Variance(row => row.Get<double>("bmi"))),
                           ("avg_bmi", rows => rows.Average(row => row.Get<double>("bmi")))
                )
                .Arrange(row => row.Get<string>("gender"), row => row.Get<string>("age_bracket"), descending: true);

            peopleBmiStatistics.Print();

            // Inhabited planets

            var inhabitedPlanets = planets
                .InnerJoin(people, (planet, person) => planet.Get<int>("id") == person.Get<int>("homeworld_id"))
                .GroupBy("name")
                .Summarise("count", rows => rows.Count())
                .Filter(row => row.Get<int>("count") > 1)
                .Arrange(row => row.Get<int>("count"), descending: true);

            inhabitedPlanets.Print();

            // Species height analysis

            var speciesHeightAnalysis = people
                .GroupBy("species_id")
                .Summarise(("average_height_calculated", rows => rows.Average(row => row.Get<int>("height"))),
                           ("count", rows => rows.Count()))
                .LeftJoin(species, (l, r) => l.Get<int>("species_id") == r.Get<int>("id"))
                .Mutate("abs_diff", row => AbsDiff(row.Get<double>("average_height_calculated"), row.Get<int>("average_height")))
                .Select("name", "abs_diff", "count")
                .Arrange(row => row.Get<double>("abs_diff"), descending: true);

            speciesHeightAnalysis.Print(5);
        }
    }
}
