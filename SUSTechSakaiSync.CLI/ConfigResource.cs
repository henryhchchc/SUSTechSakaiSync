using System;
using System.Collections.Generic;
using System.Text;

namespace SUSTechSakaiSync.CLI
{
    public class ConfigResource
    {
        public string ServerRoot { get; set; }

        public string LocalRoot { get; set; }

        public List<string> Excludes { get; set; }

    }
}
