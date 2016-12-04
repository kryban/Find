using System;
using System.Collections.Generic;
using System.IO;

namespace Find
{
    public static class PropertyWorker
    {

        public static List<string> SearchPropertyCreation(List<string> list, string criteria)
        {
            return SearchPropertyActivity(list, criteria, false);
        }

        public static List<string> SearchPropertyUsage(List<string> list, string criteria)
        {
            return SearchPropertyActivity(list, criteria, true);
        }

        private static List<string> SearchPropertyActivity(List<string> list, string criteria, bool searchUsage)
        {
            string addStatement = Properties.Settings.Default.PropertyCreationCall.ToLower().Replace(" ", string.Empty);
            string useStatement = Properties.Settings.Default.PropertyUsageCall.ToLower().Replace(" ", string.Empty);

            // creation of usage zoeken
            string propertyCall = searchUsage ? useStatement : addStatement;

            List<string> preMatches = FileHelper.FindFilesWithCriteria(list, criteria);
            List<string> specificMatch = FileHelper.FindFilesWithCriteria(preMatches, propertyCall);
            List<string> result = new List<string>();

            string progress = searchUsage ? "\n> Filtering property users" : "\n> Filtering property creators";
            Console.Write(progress);

            AnalyzeLineForProperty(criteria, propertyCall, specificMatch, result);

            Console.Write("> Done");
            return result;
        }

        private static void AnalyzeLineForProperty(string criteria, string propertyCall, List<string> specificMatch, List<string> result)
        {
            foreach (var path in specificMatch)
            {
                Console.Write(".");
                int lineNumber = 0;
                int lineNumber_WantedPropertyName = 0;
                int lineNumber_NotRelatedPropertyName = 0;
                int lineNumber_PropertyCall = 0;

                string fileOriginal = File.ReadAllText(path);
                string onlyCode = fileOriginal
                                    .Substring(fileOriginal.IndexOf("#endif // __DESIGNER_DATA") + 1)
                                    .Replace(" ", string.Empty).ToLower();

                // elk bestand in voorlopige matches nalopen op werkelijke create o aanroep van property
                foreach (var line in onlyCode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                {

                    // eerst line vinden waarin propertyName gedeclareerd wordt
                    if (line.Contains(criteria.ToLower()) && line.Contains("propertyname="))
                    {
                        lineNumber_WantedPropertyName = lineNumber;
                    }

                    // vinden we nog een tweede declaratie, dan wordt de match kennelijk overschreven
                    if (!line.Contains(criteria.ToLower()) && line.Contains("propertyname="))
                    {
                        lineNumber_NotRelatedPropertyName = lineNumber;
                    }

                    // dan line vinden waarin property aanmaak/aanroep call plaatsvindt, 
                    // maar pas nadat de relevante propertyname gedeclareerd is
                    if (line.ToLower().Contains(propertyCall) && lineNumber_WantedPropertyName > 0)
                    {
                        lineNumber_PropertyCall = lineNumber;

                        // als we deze vinden, dan stoppen we met lines zoeken, 
                        // want dan hebben we een declaratie en een get/setproperty call
                        // een eventuele tweede aanroep is niet interessant voor bepalen van een match
                        break;
                    }
                    
                    lineNumber++;
                }

                // als volgens op de propertyName declaratie een Get of Set propertyCall komt, dan is het van die dataName
                // waardoor we het als een match kunnen zien
                if (lineNumber_WantedPropertyName > lineNumber_NotRelatedPropertyName
                     && lineNumber_WantedPropertyName < lineNumber_PropertyCall)
                {
                    result.Add(path);
                }
            }
        }
    }
}
