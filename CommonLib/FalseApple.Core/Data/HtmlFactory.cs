namespace FalseApple.Core.Data
{
    public class HtmlFactory
    {
        public HtmlFactory(string tag)
        {
            Tag = tag;
        }
        public string Tag { get; set; }

        public string get()
        {
            return Tag;
        }
    }
}
