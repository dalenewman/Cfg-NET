using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Cfg.Demo.Cfg;

namespace Cfg.Demo {

    /// <summary>
    /// A demo app for Cfg.NET
    /// </summary>
    class Program {
        static void Main(string[] args) {

            Console.WriteLine("Program is running.");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            // PREPARE PARAMETER
            var parameters = new Dictionary<string, string>();
            if (args != null && args.Length > 0) {
                parameters["SqlServer"] = args[0];
            }

            // LOAD FILE
            Console.WriteLine("Loading NorthWind.xml");
            var xml = File.ReadAllText("NorthWind.xml");
            Console.WriteLine("File loaded at {0} ms.", stopWatch.ElapsedMilliseconds);

            // LOAD CONFIGURATION
            Console.WriteLine("Loading configuration.");
            var cfg = new TflRoot(xml, parameters);
            Console.WriteLine("Loaded Configuration at {0} ms.", stopWatch.ElapsedMilliseconds);

            // REPORT PROBLEMS
            Console.WriteLine("Checking for problems.");
            var problems = cfg.Problems();
            Console.WriteLine("Found {0} problem{1}.", problems.Count, problems.Count == 1 ? "" : "s");
            foreach (var problem in problems) {
                Console.WriteLine(problem);
            }
            Console.WriteLine("Problems reported at {0} ms.", stopWatch.ElapsedMilliseconds);

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
