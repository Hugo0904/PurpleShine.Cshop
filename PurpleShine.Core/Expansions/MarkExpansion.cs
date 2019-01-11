using PurpleShine.Core.Models;

namespace PurpleShine.Core.Expansions
{
    public enum Align
    {
        Center,
        Right,
        Left,
        //<div align="center"><font color="white" size ="14">投注中</font>
        //</div>
    }

    public static class HtmlFunc
    {
        public static HtmlNode Align(this HtmlNode f, Align a)
        {
            f.Tag = $"<div align='{a.ToString()}'>{f.Tag}</div>";
            return f;
        }

        public static HtmlNode Font(this HtmlNode f, string color)
        {
            return f.Font(color, 0);
        }
        public static HtmlNode Font(this HtmlNode f, int size)
        {
            return f.Font(null, size);
        }
        public static HtmlNode Font(this HtmlNode f, string color, int size)
        {
            f.Tag = $"<font {(color.IsNull() ? "" : "color='" + color + "'")} {(size == 0 ? "" : "size='" + size + "'")}>{f.Tag}</font>";
            return f;
        }
    }
}
