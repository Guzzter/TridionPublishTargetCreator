using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.CoreService.Client;
using System.Xml;
using TridionPublishTargetCreator.CoreService;
using System.IO;
using System.Xml.Linq;

namespace TridionPublishTargetCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Please specify a valid CSV file as the 1st argument.");
            }
            else {
                string csvFile = args[0];
                var ch = new CsvTridionReader(csvFile);
                if (ch.IsValid())
                {
                    List<PublicationTarget> listWithTargets = ch.Read();
                    Console.WriteLine(Environment.NewLine + String.Format("Found {0} targets in {1}", listWithTargets.Count, Path.GetFileName(csvFile)));

                    var ptc = new PubTargetCreator(listWithTargets);
                    Console.WriteLine(Environment.NewLine + "Available target types:");
                    foreach (var types in ptc.GetTargetTypes()) 
                    {
                        Console.WriteLine(string.Format("{0} ({1})", types.Title, types.Id));
                    }

                    Console.WriteLine(Environment.NewLine + "Available publication targets:");
                    foreach (var targets in ptc.GetPublicationTargets())
                    {
                        Console.WriteLine(string.Format("{0} ({1})", targets.Title, targets.Id));
                    }

                    Console.Write(Environment.NewLine + Environment.NewLine + "=====================================================");
                    Console.Write(Environment.NewLine + "First delete all Pub Targets and Types? Answer [Y/N]: ");
                    
                    var key = Console.ReadKey();
                    bool deleteAll = (key.KeyChar == 'y' || key.KeyChar == 'Y');
                    bool deleteCsvFirst = false;
                    if (!deleteAll) { 
                        Console.Write(Environment.NewLine + "First delete Pub Targets and Types that are in the CSV? Answer [Y/N]: ");
                        var keyCsv = Console.ReadKey();
                        deleteCsvFirst = (keyCsv.KeyChar == 'y' || keyCsv.KeyChar == 'Y');
                    }

                    Console.Write(Environment.NewLine);
                    if (deleteAll || deleteCsvFirst) {
                        Console.WriteLine(Environment.NewLine + "Removing unwanted Pub Targets");
                        ptc.DeletePubTargets(deleteAll);
                        Console.WriteLine(Environment.NewLine + "Removing unwanted Target Types");
                        ptc.DeleteTargetTypes(deleteAll);
                    }
                    
                    Console.WriteLine(Environment.NewLine + "Creating new Target Types");
                    ptc.CreateNewTargetTypes();
                    Console.WriteLine(Environment.NewLine + "Creating new Pub Targets");
                    ptc.CreateNewPubTargets();
                }
                else
                {
                    Console.WriteLine(Environment.NewLine + "Error: could not find CSV file. Used " + csvFile);
                }
            }
            Console.WriteLine(Environment.NewLine + "Done..");
            Console.ReadLine();
        }
        

    }


}
