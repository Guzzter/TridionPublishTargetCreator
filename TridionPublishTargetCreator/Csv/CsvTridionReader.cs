using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TridionPublishTargetCreator
{
    public class CsvTridionReader
    {
        private string _filename;

        public CsvTridionReader(string filename)
        {
            _filename = filename;
        }

        public bool IsValid()
        {
            if (!File.Exists(_filename))
            {
                return false;
            }
            return true;
        }

        public List<PublicationTarget> Read()
        {
            
            using (var tr = new StreamReader(_filename))
            {
                var csv = new CsvHelper.CsvReader(tr);
                csv.Configuration.Delimiter = ",";
                csv.Configuration.IsHeaderCaseSensitive = false;
                csv.Configuration.SkipEmptyRecords = true;
                csv.Configuration.IgnoreBlankLines = true;
                csv.Configuration.RegisterClassMap<PublicationTargetCsvMapping>();
                //csv.Configuration.TrimHeaders = true;

                var records = csv.GetRecords<PublicationTarget>().ToList();
                return records;
            }
        }
    }
}
