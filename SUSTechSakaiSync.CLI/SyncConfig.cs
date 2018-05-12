using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SUSTechSakaiSync.CLI
{
    public class SyncConfig
    {
        public string UserName { get; private set; }

        public string Password { get; private set; }

        public List<ConfigResource> Resources { get; private set; }

        private SyncConfig()
        {
        }

        public static SyncConfig FromConfigXml(string xml)
        {
            var root = XElement.Parse(xml);
            return new SyncConfig
            {
                UserName = root.Attribute("UserName").Value,
                Password = root.Attribute("Password").Value,
                Resources = root.Nodes()
                    .OfType<XElement>()
                    .Select(e => new ConfigResource
                    {
                        ServerRoot = e.Attribute("ServerRoot").Value,
                        LocalRoot = e.Attribute("LocalRoot").Value,
                        Excludes = e.Nodes()
                                    .OfType<XElement>()
                                    .FirstOrDefault()?
                                    .Nodes()
                                    .OfType<XElement>()
                                    .Select(i => i.Value).ToList() ?? new List<string>()
                    }).ToList()
            };
        }

        public ICredentials GetCredentials()
            => new NetworkCredential(UserName, Password);
    }
}
