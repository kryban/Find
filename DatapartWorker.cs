using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Find
{
    public static class DatapartWorker
    {
        public static List<string> SearchDatapartCreation(List<string> list, string criteria)
        {
            return SearchDatapartActivity(list, criteria, false);
        }

        public static List<string> SearchDatapartUsage(List<string> list, string criteria)
        {
            return SearchDatapartActivity(list, criteria, true);
        }

        public static List<string> SearchDatapartActivity(List<string> list, string criteria, bool searchUsage)
        {
            string addStatement = Properties.Settings.Default.DatapartCreationCall.ToLower().Replace(" ", string.Empty);
            string useStatement_GET = Properties.Settings.Default.DatapartUsageCall_Get.ToLower().Replace(" ", string.Empty);
            string useStatement_CHECK = Properties.Settings.Default.DatapartUsageCall_Check.ToLower().Replace(" ", string.Empty);

            List<string> preMatches = FileHelper.FindFilesWithCriteria(list, criteria);

            List<string> specificMatch = new List<string>();

            //bij usage een subselectie op prematches. Bij creation selecteren op de volledige lijst.
            if (searchUsage)
            {
                List<string> specificMatchGet = FileHelper.FindFilesWithCriteria(preMatches, useStatement_GET);
                List<string> specificMatchCheck = FileHelper.FindFilesWithCriteria(preMatches, useStatement_CHECK);
                specificMatch = specificMatchGet.Concat(specificMatchCheck).ToList();
            }
            else
            {
                specificMatch = FileHelper.FindFilesWithCriteria(list, addStatement);
            }


            List<string> callableServicesWithAddDatapart = searchUsage ? new List<string>() : FileHelper.FindFilesWithCriteria(list, addStatement);
            List<Tuple<string, string>> callableServiceNames = ExtractServiceNames(callableServicesWithAddDatapart);
            List<string> result = new List<string>();

            string progress = searchUsage ? "\n> Filtering users" : "\n> Filtering creators";
            Console.Write(progress);

            if(searchUsage)
            {
                AnalyzeLine(criteria, useStatement_CHECK, specificMatch, callableServiceNames, result);
                AnalyzeLine(criteria, useStatement_GET, specificMatch, callableServiceNames, result);
            }
            else
            {
                AnalyzeLine(criteria, addStatement, specificMatch, callableServiceNames, result);
            }
            

            Console.Write("> Done");


            // TODO: eventuele duplicates worden veroorzaakt door CheckDatapart en GetDatapart calls
            // TODO: uitfilteren voldoet, maar netter is om Check en Get netjes te verwerken
            return result.Distinct().ToList();
        }

        private static void AnalyzeLine(string criteria, string datapartCall, List<string> specificMatch, List<Tuple<string, string>> callableServiceNames, List<string> result)
        {
            foreach (var path in specificMatch)
            {
                Console.Write(".");
                int lineNumber = 0;
                int lineNumber_WantedDatapartName = 0;
                int lineNumber_NotRelatedDatapartName = 0;
                int lineNumber_AddDatapartCall = 0;

                string fileOriginal = File.ReadAllText(path);
                string onlyCode = fileOriginal
                                    .Substring(fileOriginal.IndexOf("#endif // __DESIGNER_DATA") + 1)
                                    .Replace(" ", string.Empty).ToLower();

                // elk bestand in voorlopige matches nalopen op werkelijke create van datapart
                //foreach (var line in File.ReadLines(path))
                foreach (var line in onlyCode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    var foo = 0;

                    // eerst line vinden waarin datapartName gedeclareerd wordt
                    if (line.Contains(criteria.ToLower()) && line.Contains("datapartname="))
                    {
                        lineNumber_WantedDatapartName = lineNumber;
                    }

                    // vinden we nog een tweede declaratie, dan wordt de match kennelijk overschreven
                    if (!line.Contains(criteria.ToLower()) && line.Contains("datapartname="))
                    {
                        lineNumber_NotRelatedDatapartName = lineNumber;
                    }

                    // dan line vinden waarin datapart aanmaak call plaatsvindt, maar pas nadat de relevante datapartname gedeclareerd is
                    if (line.ToLower().Contains(datapartCall) && lineNumber_WantedDatapartName > 0)
                    {
                        lineNumber_AddDatapartCall = lineNumber;

                        // als we deze vinden, dan stoppen we met lines zoeken, 
                        // want dan hebben we een declaratie en een AddDatapart call
                        break;
                    }
                    else
                    {
                        // dan kan datapart nog in een called service aangemaakt worden
                        // dus als volgend op aanmaak datapartname een service wordt gecalled, dan is het aannemelijk dat 
                        // daarin ook datapart aangemaakt wordt.
                        // die proberen we te vinden indien niet in het huidig document een addDatapart gecalled wordt
                        foreach (var element in callableServiceNames)
                        {
                            if (line.Contains(element.Item1))
                            {
                                lineNumber_AddDatapartCall = lineNumber;
                            }
                        }
                    }

                    lineNumber++;
                }

                // als beoogde datapartname gebruikt wordt in de AddDatapartCall (al dan niet indirect), dan toevoegen
                if (lineNumber_WantedDatapartName > lineNumber_NotRelatedDatapartName
                     && lineNumber_WantedDatapartName < lineNumber_AddDatapartCall)
                {
                    result.Add(path);
                }
            }
        }

        private static List<Tuple<string, string>> ExtractServiceNames(List<string> callableServices)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xpath_ServiceNamespace = "/om:MetaModel/om:Element[@Type=\"Module\"]/om:Property[@Name=\"Name\"]";
            string xpath_ServiceName = "/om:MetaModel/om:Element[@Type=\"Module\"]/om:Element[@Type=\"ServiceDeclaration\"]/om:Property[@Name=\"Name\"]";

            var fullServiceNames = new List<Tuple<string, string>>();

            foreach (var path in callableServices)
            {
                string document = File.ReadAllText(path);
                document = BeautifyStringForXml(document);

                xmlDoc.LoadXml(document);

                XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
                manager.AddNamespace("om", "http://schemas.microsoft.com/BizTalk/2003/DesignerData");

                string serviceNamespace = xmlDoc.SelectSingleNode(xpath_ServiceNamespace, manager).Attributes["Value"].Value;
                string serviceName = xmlDoc.SelectSingleNode(xpath_ServiceName, manager).Attributes["Value"].Value;
                string fullName = serviceNamespace + "." + serviceName;

                fullServiceNames.Add(Tuple.Create(fullName, path));
            }

            Console.Write("\n> Extracted serviceNames");
            return fullServiceNames;
        }

        private static string BeautifyStringForXml(string document)
        {
            document = document.Remove(document.IndexOf("\r\n#endif // __DESIGNER_DATA"));
            document = document.Replace("#if __DESIGNER_DATA\r\n", "");
            document = document.Replace("#error Do not define __DESIGNER_DATA.\r\n", "");
            File.WriteAllText(@"C:\Div\BizTalk\GALO_en_ESB\testje_GestriptFinal.txt", document);
            return document;
        }
    }
}
