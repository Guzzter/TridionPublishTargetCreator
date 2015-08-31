using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.CoreService.Client;

namespace TridionPublishTargetCreator
{
    public class PubTargetCreator
    {
        private const string configFilename = "CoreServiceHandler.config";
        private CoreServiceHandler _coreServiceHandler;
        private CoreServiceClient _core;
        private List<PublicationTarget> _targets;

        private static Tridion.ContentManager.CoreService.Client.ReadOptions _readOpts = new Tridion.ContentManager.CoreService.Client.ReadOptions();

        public PubTargetCreator(List<PublicationTarget> targets)
        {
            _coreServiceHandler = new CoreServiceHandler(configFilename);
            _core = _coreServiceHandler.GetNewNetTcpClient();
            _targets = targets;
        }

        public List<PublicationTargetData> GetPublicationTargets()
        {
            var list = _core.GetSystemWideList(new PublicationTargetsFilterData()).Cast<PublicationTargetData>().ToList();
            return list;
        }

        public List<TargetTypeData> GetTargetTypes()
        {
            var list = _core.GetSystemWideList(new TargetTypesFilterData()).Cast<TargetTypeData>().ToList();
            return list;
        }

        public PublicationData GetPublication(string titlePrefix)
        {
            PublicationData pd = _core.GetSystemWideList(new PublicationsFilterData()).Cast<PublicationData>().FirstOrDefault(x => x.Title.StartsWith(titlePrefix));
            return pd;
        }


        public void DeletePubTargets(bool deleteAll = false)
        {
            var pubTargets = GetPublicationTargets();
            if (deleteAll)
            {
                foreach (var pt in pubTargets)
                {
                    _core.Delete(pt.Id);
                }
            }
            else
            {
                foreach (var target in _targets)
                {
                    var pubTarget = pubTargets.SingleOrDefault(pt => pt.Title == target.Name);
                    // Do not add when publish target is existing
                    if (pubTarget != null)
                    {
                        //Store
                        _core.Delete(pubTarget.Id);
                    }
                }
            }
        }

        public void DeleteTargetTypes(bool deleteAll = false)
        {
            var list = GetTargetTypes();
            if (deleteAll) {
                foreach (var targetTp in list)
                {
                    _core.Delete(targetTp.Id);
                }
            }else {
                // Lookup the unique names for target types in CSV
                foreach (string targetTypeName in _targets.Select(p => p.TargetType).Distinct())
                {
                    var targetTp = list.FirstOrDefault(t => t.Title == targetTypeName);
                    // Do not add when target type is existing
                    if (targetTp != null)
                    {
                        _core.Delete(targetTp.Id);
                    }
                }
            }
        }

        // LIVE or Staging
        public void CreateNewTargetTypes()
        {
            var list = GetTargetTypes();

            // Lookup the unique names for target types in CSV
            foreach (string targetTypeName in _targets.Select(p => p.TargetType).Distinct())
            {
                // Do not add when target type is existing
                if (!list.Exists(t => t.Title == targetTypeName))
                {
                    var tt = _core.GetDefaultData(ItemType.TargetType, null) as TargetTypeData;
                    tt.Title = targetTypeName;
                    tt.Description = targetTypeName;
                    _core.Create(tt, _readOpts);
                }
            }
        }



        public void CreateNewPubTargets()
        {
            var targetTypeList = GetTargetTypes();
            var pubTargets = GetPublicationTargets();
            foreach (var target in _targets)
            {
                // Do not add when publish target is existing
                if (!pubTargets.Exists(pt => pt.Title == target.Name))
                {
                    // Lookup wanted target type and publication
                    var wantedTargetType = targetTypeList.First(tt => tt.Title == target.TargetType);
                    var wantedPub = GetPublication(target.LinkedPublicationPrefix);
                    if (wantedPub == null)
                    {
                        throw new Exception("Could not find a publication starting with " + target.LinkedPublicationPrefix);
                    }
                    
                    // Setup new Pub Target object
                    var pt = _core.GetDefaultData(ItemType.PublicationTarget, null) as PublicationTargetData;
                    pt.Title = target.Name;
                    pt.Description = target.Description;
                    pt.Publications = new[] { new LinkToPublicationData() { IdRef = wantedPub.Id } };
                    pt.TargetTypes = new[] { new LinkToTargetTypeData() { IdRef = wantedTargetType.Id } };
                    pt.TargetLanguage = "None";
                    TargetDestinationData destination = new TargetDestinationData
                    {
                        ProtocolSchema = new LinkToSchemaData { IdRef = "tcm:0-6-8" },
                        Title = target.DestinationName,
                        ProtocolData =
                            "<HTTPS xmlns=\"http://www.tridion.com/ContentManager/5.0/Protocol/HTTPS\"><UserName>n/a</UserName><Password>n/a</Password><URL>" +
                            target.Url + "</URL></HTTPS>"
                    };
                    pt.Destinations = new[] { destination };

                    //Store
                    _core.Create(pt, _readOpts);
                }
            }
        }
        
        /*
        internal void CreatePublicationTarget(XElement site, WebsiteHelper.TargetLanguage language)
        {
            string targetName = site.Element("Name").Value;
            string targetTypeId = TcmUri.UriNull;
            string url = string.Empty;
            if (language == WebsiteHelper.TargetLanguage.Aspnet)
                url = "http://localhost:" + site.Element("Port").Value + "/httpupload.aspx";
            else
                url = "http://localhost:" + site.Element("Port").Value + "/" + site.Element("ContextRoot").Value + "/httpupload";

            TargetTypesFilterData filter = new TargetTypesFilterData();
            //filter.ForRepository.IdRef = Configuration.WebsitePublicationId;
            foreach (XElement node in _client.GetSystemWideListXml(filter).Nodes())
            {
                if (node.Attribute("Title").Value.Equals(targetName))
                {
                    targetTypeId = node.Attribute("ID").Value;
                    break;
                }
            }
            if (targetTypeId.Equals(TcmUri.UriNull))
            {
                TargetTypeData targetType =
                    (TargetTypeData)_client.GetDefaultData(ItemType.TargetType, null, _readOptions);
                targetType.Title = targetName;
                targetType.Description = targetName;
                targetType = (TargetTypeData)_client.Save(targetType, _readOptions);
                targetTypeId = targetType.Id;
            }

            PublicationTargetsFilterData pFilter = new PublicationTargetsFilterData();
            string pTargetId = TcmUri.UriNull;
            foreach (XElement node in _client.GetSystemWideListXml(pFilter).Nodes())
            {
                if (node.Attribute("Title").Value.Equals(targetName))
                {
                    pTargetId = node.Attribute("ID").Value;
                    break;
                }
            }
            if (pTargetId.Equals(TcmUri.UriNull))
            {
                PublicationTargetData pt =
                    (PublicationTargetData)_client.GetDefaultData(ItemType.PublicationTarget, null, _readOptions);
                pt.Title = targetName;
                pt.Description = targetName;
                pt.Publications = new[] { new LinkToPublicationData { IdRef = Configuration.WebsitePublicationId } };
                pt.TargetTypes = new[] { new LinkToTargetTypeData { IdRef = targetTypeId } };
                TargetDestinationData destination = new TargetDestinationData
                {
                    ProtocolSchema = new LinkToSchemaData { IdRef = "tcm:0-6-8" },
                    Title = targetName,
                    ProtocolData =
                        "<HTTPS xmlns=\"http://www.tridion.com/ContentManager/5.0/Protocol/HTTPS\"><UserName>notused</UserName><Password>notused</Password><URL>" +
                        url + "</URL></HTTPS>"
                };
                pt.Destinations = new[] { destination };
                if (language == WebsiteHelper.TargetLanguage.Aspnet)
                    pt.TargetLanguage = "ASP.NET";
                else
                    pt.TargetLanguage = "JSP";

                _client.Save(pt, null);
            }


        }*/

    }
}
