using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FalseApple.Net.Http
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
