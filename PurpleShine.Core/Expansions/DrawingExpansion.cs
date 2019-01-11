using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PurpleShine.Core.Libraries;

namespace PurpleShine.Core.Expansions
{
    public static class DrawingExpansion
    {
        public const int GW_HWNDNEXT = 2; // The next window is below the specified window
        public const int GW_HWNDPREV = 3; // The previous window is above

        /// <summary>
        /// 為MdiContainer 建立 Child
        /// </summary>
        /// <typeparam name="T">child type</typeparam>
        /// <param name="Container">MdiContainer</param>
        /// <param name="multiple">是否可多個相同child</param>
        public static void OpenChild<T>(this Form Container, bool multiple = false) where T : Form
        {
            if (!Container.IsMdiContainer) return;

            if (!multiple)
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form.GetType() == typeof(T))
                    {
                        form.WindowState = FormWindowState.Normal;
                        form.Location = new Point((Container.ClientSize.Width - form.Width) / 2,
                                   (Container.ClientSize.Height - form.Height) / 2);

                        form.Activate();
                        return;
                    }
                }
            }
            Form t = (T)Activator.CreateInstance(typeof(T));
            t.MdiParent = Container;
            t.StartPosition = FormStartPosition.CenterScreen;
            t.Show();
        }


        /// <summary>
        /// Searches for the topmost visible form of your app in all the forms opened in the current Windows session.
        /// </summary>
        /// <param name="hWnd_mainFrm">Handle of the main form</param>
        /// <returns>The Form that is currently TopMost, or null</returns>
        public static Form GetTopMostWindow(IntPtr hWnd_mainFrm)
        {
            Form frm = null;

            IntPtr hwnd = SafeNativeMethods.GetTopWindow((IntPtr)null);
            if (hwnd != IntPtr.Zero)
            {
                while ((!SafeNativeMethods.IsWindowVisible(hwnd) || frm == null) && hwnd != hWnd_mainFrm)
                {
                    // Get next window under the current handler
                    hwnd = SafeNativeMethods.GetNextWindow(hwnd, GW_HWNDNEXT);

                    try
                    {
                        frm = (Form)Form.FromHandle(hwnd);
                    }
                    catch
                    {
                        // Weird behaviour: In some cases, trying to cast to a Form a handle of an object 
                        // that isn't a form will just return null. In other cases, will throw an exception.
                    }
                }
            }

            return frm;
        }

        public static void ClearTexLine(this RichTextBox @this, int keepLine)
        {
            if (keepLine < 1)
                return;

            bool isReadOnly = false;

            if (isReadOnly = @this.ReadOnly)
                @this.ReadOnly = false;

            while (@this.Lines.Length > keepLine)
            {
                @this.SelectionStart = 0;
                @this.SelectionLength = @this.Text.IndexOf("\n", 0) + 1;
                @this.SelectedText = "";
            }

            if (isReadOnly != @this.ReadOnly)
                @this.ReadOnly = isReadOnly;
        }

        public static void ScrollToEnd(this RichTextBox @this)
        {
            @this.SelectionStart = @this.Text.Length;
            @this.ScrollToCaret();
        }

        /// <summary>
        /// 取得該richbox這高度內可以顯示的行數 (不含scroll bar)
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static int GetVisibleLineCount(this RichTextBox @this)
        {
            return @this.Lines.Length - @this.GetLineFromCharIndex(@this.GetCharIndexFromPosition(new Point(1, 1)));
        }

        /// <summary>
        /// 自動關閉程式
        /// </summary>
        /// <param name="form"></param>
        public static void AutoClose(this Object form)
        {
            if (Application.MessageLoop)
            {
                Application.Exit();
            }
            else
            {
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 開啟Control雙向緩衝
        /// </summary>
        /// <param name="this"></param>
        /// <param name="types"></param>
        public static void OpenDoubleBuffered(this Control @this, params Type[] types)
        {
            Predicate<Type> _types;

            if (types.Length > 0)
                _types = (t) => types.Contains(t);
            else
                _types = (t) => true;

            foreach (Control ctl in @this.GetAll(_types))
            {
                ctl.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(ctl, true, null);
            }
        }

        public static IEnumerable<Control> GetAll(this Control @this, Predicate<Type> types)
        {
            var controls = @this.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, types))
                                      .Concat(controls)
                                      .Where(c => types(c.GetType()));
        }

        public static void CenterToMdiPerent(this Form form)
        {
            if (form.IsMdiChild)
            {
                form.UpdateUI(() =>
                {
                    form.Location = new Point(
                     form.MdiParent.ClientSize.Width / 2 - form.Width / 2,
                     form.MdiParent.ClientSize.Height / 2 - form.Height / 2);
                });
            }
        }

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