using CommonLib.Data;

namespace CommonLib.Expansion
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
        public static HtmlFactory Align(this HtmlFactory f, Align a)
        {
            f.Tag = $"<div align='{a.ToString()}'>{f.Tag}</div>";
            return f;
        }

        public static HtmlFactory Font(this HtmlFactory f, string color)
        {
            return f.Font(color, 0);
        }
        public static HtmlFactory Font(this HtmlFactory f, int size)
        {
            return f.Font(null, size);
        }
        public static HtmlFactory Font(this HtmlFactory f, string color, int size)
        {
            f.Tag = $"<font {(color == null ? "" : "color='" + color + "'")} {(size == 0 ? "" : "size='" + size + "'")}>{f.Tag}</font>";
            return f;
        }
    }
}
