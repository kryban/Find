using System;
using System.Collections.Generic;

namespace Find
{
    class Program
    {
        static void Main(string[] args)
        {
            string goOn = "y";

            while (goOn.Equals("y"))
            {
                Console.Write("Path (optional): ");
                string startPath = Console.ReadLine();
                startPath = startPath.Length > 0 ? startPath : Properties.Settings.Default.DefaultPath;

                Console.Write("Search for datapart(d) or property (p): ");
                string searchFor = Console.ReadLine().ToLower() ;
                if(searchFor != "d" && searchFor != "p")
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

                List<string> resulSetCreation = new List<string>();
                List<string> resulSetUsage = new List<string>();

                if (searchFor.Equals("d"))
                {
                    
                    resulSetCreation = DatapartWorker.SearchDatapartCreation(fileList, criteria);                  
                    resulSetUsage = DatapartWorker.SearchDatapartUsage(fileList, criteria);
                }
                else
                {
                    resulSetCreation = PropertyWorker.SearchPropertyCreation(fileList, criteria);
                    resulSetUsage = PropertyWorker.SearchPropertyUsage(fileList, criteria);
                }

                Console.WriteLine("\n\nRESULT");

                Console.WriteLine("\nCreation:");

                foreach (string path in resulSetCreation)
                {
                    Console.WriteLine(path);
                }

                Console.WriteLine("\nUsage:");
                foreach (string path in resulSetUsage)
                {
                    Console.WriteLine(path);
                }

                Console.WriteLine("\nDone.");

                Console.Write("Continue (y/n): ");
                goOn= Console.ReadLine().ToString();
            }
        }
    }
}
