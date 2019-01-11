using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using PurpleShine.Core.Expansions;

namespace PurpleShine.Core.Helpers
{
    public static class Language
    {
        public static void Apply(Form form, string language)
        {
            var resManager = new ComponentResourceManager(form.GetType());
            ApplyResources(resManager, form, new CultureInfo(language));
            resManager.ReleaseAllResources();
            GC.Collect();
        }

        public static void ApplyResources(ComponentResourceManager resManager, Control parent, CultureInfo culture)
        {
            resManager.ApplyResources(parent, parent.Name, culture);
            foreach (Control ctl in parent.Controls)
            {
                ApplyResources(resManager, ctl, culture);
            }
        }
    }

    public static class DrawingUtil
    {
        /// <summary>
        /// 將text繪製成圖片
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="textColor"></param>
        /// <param name="backColor"></param>
        /// <returns></returns>
        public static Image DrawTextPicture(string text, Font font, Color textColor, Color backColor)
        {
            using (Image img = new Bitmap(1, 1))
            using (Graphics drawing = Graphics.FromImage(img))
            {
                SizeF textSize = drawing.MeasureString(text, font);
                Image img2 = new Bitmap((int)textSize.Width, (int)textSize.Height);
                using (Graphics drawing2 = Graphics.FromImage(img2))
                {
                    drawing2.SmoothingMode = SmoothingMode.AntiAlias;
                    drawing2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    drawing2.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    drawing2.Clear(backColor);
                    using (Brush textBrush = new SolidBrush(textColor))
                    {
                        drawing2.DrawString(text, font, textBrush, 0, 0);
                        drawing2.Flush();
                        return img2;
                    }
                }
            }
        }

        /// <summary>
        /// 將text繪製成圖片(圓形)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="textColor"></param>
        /// <param name="backColor"></param>
        /// <returns></returns>
        public static Image DrawTextRectanglePicture(string text, Font font, Color textColor, Color backColor)
        {
            using (Image img = new Bitmap(1, 1))
            using (Graphics drawing = Graphics.FromImage(img))
            {
                SizeF textSize = drawing.MeasureString(text, font);
                Image img2 = new Bitmap((int)textSize.Width, (int)textSize.Height);
                using (Graphics drawing2 = Graphics.FromImage(img2))
                using (GraphicsPath path = new GraphicsPath())
                {
                    drawing2.SmoothingMode = SmoothingMode.AntiAlias;
                    drawing2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    drawing2.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    path.AddEllipse(0, 0, img2.Width, img2.Height);
                    drawing2.SetClip(path);
                    using (Brush textBrush = new SolidBrush(textColor))
                    using (Brush br = new SolidBrush(backColor))
                    {
                        drawing2.FillEllipse(br, drawing2.ClipBounds);
                        drawing2.DrawString(text, font, textBrush, 0, 0);
                    }
                    drawing2.Flush();
                    return img2;
                }
            }
        }

        /// <summary>
        /// 將字劃到圖形內
        /// </summary>
        /// <param name="image"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="textColor"></param>
        /// <param name="site"></param>
        public static void DrawTextInPicture(Image image, string text, Font font, Color textColor, ContentAlignment site)
        {
            using (Graphics drawing = Graphics.FromImage(image))
            {
                drawing.SmoothingMode = SmoothingMode.AntiAlias;
                drawing.InterpolationMode = InterpolationMode.HighQualityBicubic;
                drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (Brush textBrush = new SolidBrush(textColor))
                using (StringFormat sf = new StringFormat())
                {
                    string siteName = site.ToString();
                    sf.LineAlignment = siteName.StartsWith("Top") ? StringAlignment.Near : siteName.StartsWith("Middle") ? StringAlignment.Center : StringAlignment.Far;
                    sf.Alignment = siteName.EndsWith("Left") ? StringAlignment.Near : siteName.EndsWith("Center") ? StringAlignment.Center : StringAlignment.Far;
                    Rectangle rectf = new Rectangle(0, 0, image.Width, image.Height);
                    drawing.DrawString(text, font, textBrush, rectf, sf);
                }
                drawing.Flush();
            }
        }

        /// <summary>
        /// 字體大小自動變更
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="size"></param>
        /// <param name="font"></param>
        /// <param name="str"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static float NewFontSize(Graphics graphics, Size size, Font font, string str, Func<float, float> func = null)
        {
            SizeF stringSize = graphics.MeasureString(str, font);
            float f = font.Size * Math.Min(size.Height * 0.96f / stringSize.Height, size.Width * 0.9f / stringSize.Width);
            f = func.IsNull() ? f : func(f);
            return f < 0 || float.IsInfinity(f) ? 1f : f;
        }
    }
}
