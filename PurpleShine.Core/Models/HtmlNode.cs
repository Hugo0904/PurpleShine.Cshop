namespace PurpleShine.Core.Models
{
    public class HtmlNode
    {
        public HtmlNode(string tag)
        {
            Tag = tag;
        }

        public string Tag { get; set; }

        public string Get()
        {
            return Tag;
        }
    }
}
