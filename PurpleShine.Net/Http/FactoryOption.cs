using System.Collections.Generic;

namespace PurpleShine.Net.Http
{
    public class FactoryOption
    {
        public FactoryOption(string uri)
        {
            Uri = uri;
        }

        public Dictionary<string, string> Cookies { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public int Timeout { get; set; }

        public string Uri { get; private set; }

        public bool AllowAutoRedirect { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public bool JsonUse { get; set; }
    }
}
