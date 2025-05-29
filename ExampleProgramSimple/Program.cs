using System.Globalization;
using DplyrSharp.IO;
using DplyrSharp.Core;

namespace ExampleProgramSimple
{
    class Program
    {
        static void Main()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

            var csvOptions = new CsvOptions() { Delimiter = ';' };
            var people = CsvReader.ReadCsv("C:/Users/bruzl/Desktop/cs_zapoctak/misc/data/sample_people_data.csv", csvOptions);

            var result = people
                .Select("Name", "Country", "Age")
                .Filter(row => row.Get<int>("Age") > 30)
                .Mutate("IsSenior", row => row.Get<int>("Age") >= 65)
                .GroupBy("Country", "IsSenior")
                .Summarise(("Count", rows => rows.Count()),
                           ("AvgAge", rows => rows.Average(row => row.Get<int>("Age"))))
                .Arrange(row => row.Get<bool>("IsSenior"), row => row.Get<string>("Country"));

            result.Print();
        }        
    }
}
