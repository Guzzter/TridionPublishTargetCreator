using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TridionPublishTargetCreator
{

    public class PublicationTarget
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DestinationName { get; set; }

        public string TargetType { get; set; }
        public string LinkedPublicationPrefix { get; set; }

        private string _url;
        public string Url
        {
            get { return string.Format("http://{0}/httpupload", _url); }
            set { _url = value; }
        }

    }

    public sealed class PublicationTargetCsvMapping : CsvClassMap<PublicationTarget>
    {
        public PublicationTargetCsvMapping()
        {
            Map(m => m.Name).Index(0);
            Map(m => m.Description).Index(0);
            Map(m => m.DestinationName).Index(0);
            Map(m => m.Url).Index(1);
            Map(m => m.TargetType).Index(2);
            Map(m => m.LinkedPublicationPrefix).Index(3);
        }
    }
}
