using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace instaPicsWebJob.Model
{
    public static class ImgProcessing
    {
        // change l'image en noir et blanc
        //source http://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale by Vercas
        public static Bitmap SetGrayscale(string pathImg)
        {
            Bitmap c = new Bitmap(pathImg);
            Bitmap d = new Bitmap(pathImg);

            for (int i = 0; i < c.Width; i++)
            {
                for (int x = 0; x < c.Height; x++)
                {
                    Color oc = c.GetPixel(i, x);
                    int grayScale = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                    Color nc = Color.FromArgb(oc.A, grayScale, grayScale, grayScale);
                    d.SetPixel(i, x, nc);
                }
            }

            return d;
        }

        //source http://www.dotnetperls.com/getthumbnailimage
        public static Image GetThumbnail(string pathImg)
        {

            // Load image.
            Image image = Image.FromFile(pathImg);

            // Compute thumbnail size.
            Size thumbnailSize = GetThumbnailSize(image);

            // Get thumbnail.
            Image thumbnail = image.GetThumbnailImage(thumbnailSize.Width,
                thumbnailSize.Height, null, IntPtr.Zero);

            return thumbnail;
        }

        //source http://www.dotnetperls.com/getthumbnailimage
        private static Size GetThumbnailSize(Image original)
        {
            // Maximum size of any dimension.
            const int maxPixels = 120;

            // Width and height.
            int originalWidth = original.Width;
            int originalHeight = original.Height;

            // Compute best factor to scale entire image based on larger dimension.
            double factor;
            if (originalWidth > originalHeight)
            {
                factor = (double)maxPixels / originalWidth;
            }
            else
            {
                factor = (double)maxPixels / originalHeight;
            }

            // Return thumbnail size.
            return new Size((int)(originalWidth * factor), (int)(originalHeight * factor));
        }
    }
}
