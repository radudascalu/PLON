using Newtonsoft.Json;
using Plon.Core.PerformanceTests.TestObjects;
using System;
using System.Diagnostics;

namespace Plon.Core.PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            TestFewProperties();
            Console.WriteLine();
            TestNumerousPropertiesAndNestedObjects();
            Console.ReadLine();
        }

        private static void TestFewProperties()
        {
            var testObject = new FewProperties { Id = 42, Name = "PLON" };
            var noOfIterations = 10000;
            Console.WriteLine("Few properties, {0} iterations:", noOfIterations);

            var plonSerialized = PlonConvert.Serialize(testObject);
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            for (var idx = 0; idx < noOfIterations; idx++)
            {
                var plonDeserialized = PlonConvert.Deserialize<FewProperties>(plonSerialized);
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalSeconds);

            var jsonSerialized = JsonConvert.SerializeObject(testObject);
            stopwatch.Reset();
            stopwatch.Start();
            for (var idx = 0; idx < noOfIterations; idx++)
            {
                var jsonDeserialized = JsonConvert.DeserializeObject<FewProperties>(jsonSerialized);
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalSeconds);
        }

        private static void TestNumerousPropertiesAndNestedObjects()
        {
            var testObject = NumerousPropertiesAndNestedObjects.New();
            var noOfIterations = 1000;
            Console.WriteLine("Numerous properties with nested objects, {0} iterations:", noOfIterations);

            var plonSerialized = PlonConvert.Serialize(testObject);
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            for (var idx = 0; idx < noOfIterations; idx++)
            {
                var plonDeserialized = PlonConvert.Deserialize<NumerousPropertiesAndNestedObjects>(plonSerialized);
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalSeconds);

            var jsonSerialized = JsonConvert.SerializeObject(testObject);
            stopwatch.Reset();
            stopwatch.Start();
            for (var idx = 0; idx < noOfIterations; idx++)
            {
                var jsonDeserialized = JsonConvert.DeserializeObject<NumerousPropertiesAndNestedObjects>(jsonSerialized);
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed.TotalSeconds);
        }
    }
}
