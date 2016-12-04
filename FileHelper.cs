using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Find
{
    public static class FileHelper
    {

        public static List<string> CollectFiles(string path, string fileType)
        {
            List<string> retval;
            string searchPattern = "*." + fileType;
            try {
                retval = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories).ToList<string>();
                Console.Write("> Collected Files ");
            }
            catch(Exception)
            {
                Console.WriteLine("No such directory found.");
                retval = new List<string>();
            }

            return retval;
        }

        public static List<string> FindFilesWithCriteria(List<string> list, string criteria)
        {
            List<string> matches = new List<string>();
            criteria = criteria.Replace(" ", string.Empty).ToLower();

            foreach (string path in list)
            {
                string fileOriginal = File.ReadAllText(path);
                string formatted = fileOriginal
                                    .Substring(fileOriginal.IndexOf("#endif // __DESIGNER_DATA") + 1)
                                    .Replace(" ", string.Empty).ToLower();

                foreach (var line in formatted.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    if (line.Contains(criteria.ToLower()))
                    {
                        matches.Add(path);
                        break;
                    }
                }
            }

            return matches;
        }
    }
}
