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

            if (args == null || args.Length == 0) {
                Console.WriteLine("Please pass in the configuration file.");
                return;
            }

            // CHECK FILE
            var fileInfo = new FileInfo(args[0]);
            if (!fileInfo.Exists) {
                Console.WriteLine("File {0} does not exist.", fileInfo.FullName);
                return;
            }

            // LOAD FILE
            Console.WriteLine("Loading {0}", fileInfo.Name);
            var xml = File.ReadAllText(fileInfo.FullName);
            Console.WriteLine("File loaded at {0} ms.", stopWatch.ElapsedMilliseconds);

            // LOAD CONFIGURATION
            Console.WriteLine("Loading configuration.");
            var cfg = new TflRoot(xml, null);
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
