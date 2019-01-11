using System.Drawing;
using System.Windows.Forms;

namespace PurpleShine.Core.Expansions
{
    public static class DrawingExpansion
    {
        /// <summary>
        /// 非同步委派更新UI
        /// </summary>
        /// <param name="this">UI control</param>
        /// <param name="action">要做的事情</param>
        public static void UpdateUI(this Control @this, MethodInvoker action)
        {
            if (@this.IsDisposed) return;

            if (@this.InvokeRequired)
                @this.Invoke(action);
            else
                action();
        }

        public static void AppendText(this RichTextBox @this, string text, Color color)
        {
            @this.SelectionStart = @this.TextLength;
            @this.SelectionLength = 0;
            @this.SelectionColor = color;
            @this.AppendText(text);
        }

        public static void AppendText(this RichTextBox @this, string text, Font font)
        {
            @this.SelectionStart = @this.TextLength;
            @this.SelectionLength = 0;
            @this.SelectionFont = font;
            @this.AppendText(text);
        }

        public static void AppendText(this RichTextBox @this, string text, Color color, Font font)
        {
            @this.SelectionStart = @this.TextLength;
            @this.SelectionLength = 0;
            @this.SelectionColor = color;
            @this.SelectionFont = font;
            @this.AppendText(text);
        }
    }
}