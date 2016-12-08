using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Find
{
    class Program
    {
        static void Main(string[] args)
        {
            string goOn = "y";

            while (goOn.Equals("y"))
            {
                Console.Write("Path (optional) (default "+ Properties.Settings.Default.DefaultPath +"): ");
                string startPath = Console.ReadLine();
                startPath = startPath.Length > 0 ? startPath : Properties.Settings.Default.DefaultPath;

                Console.Write("Search for datapart(d) or property (p): ");
                string searchFor = Console.ReadLine().ToLower();
                if (searchFor != "d" && searchFor != "p")
                {
                    Console.WriteLine(searchFor + " not recognized.");
                    continue;
                }

                Console.Write("Search criteria  (optional): ");
                string criteria = Console.ReadLine();
                criteria = criteria.Length > 0 ? criteria : Properties.Settings.Default.DefaultCriteria_Test;

                string extension = Properties.Settings.Default.DefaultExtension;


                Console.WriteLine("\nProgress: ");

                List<string> fileList = new List<string>();
                fileList = FileHelper.CollectFiles(startPath, extension);

                List<string> resultSetCreation = new List<string>();
                List<string> resultSetUsage = new List<string>();

                //TODO: de gevallen worden niet gevonden indien property of datapartname gedeclareerd en daarna in een if de datapartname overschreven wordt
                if (searchFor.Equals("d"))
                {

                    resultSetCreation = DatapartWorker.SearchDatapartCreation(fileList, criteria);
                    resultSetUsage = DatapartWorker.SearchDatapartUsage(fileList, criteria);
                }
                else
                {
                    resultSetCreation = PropertyWorker.SearchPropertyCreation(fileList, criteria);
                    resultSetUsage = PropertyWorker.SearchPropertyUsage(fileList, criteria);
                }

                Console.WriteLine("\n\nRESULT");

                Console.WriteLine("\nCreation of \"" + criteria + " :");

                foreach (string path in resultSetCreation)
                {
                    string subPath = path.Replace(startPath, "");
                    Console.WriteLine(subPath);
                }

                Console.WriteLine("\nUsage of \"" + criteria + " :");
                foreach (string path in resultSetUsage)
                {
                    string subPath = path.Replace(startPath, "");
                    Console.WriteLine(subPath);
                }

                Console.WriteLine("\nDone.");

                Console.Write("Export results (y/n)"  );
                string exportResults = Console.ReadLine().ToLower();
                
                if (exportResults.Equals("y"))
                {
                    FileHelper.ExportResults(criteria, resultSetCreation, resultSetUsage);
                }

                Console.Write("Continue (y/n): ");
                goOn = Console.ReadLine().ToString();
            }
        }

        public static List<string> resultSetCreation { get; set; }
    }
}
