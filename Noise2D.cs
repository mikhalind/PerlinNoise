using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PerlinNoise
{
    class Noise2D
    {
        public WriteableBitmap GetImageFromMap(double[,] map)
        {
            int txdWidth = map.GetLength(0);
            int txdHeight = map.GetLength(1);
            WriteableBitmap bmp = new WriteableBitmap(txdWidth, txdHeight, 72, 72, PixelFormats.Bgra32, null);
            for (int i = 0; i < txdHeight; i++)
            {
                for (int j = 0; j < txdWidth; j++)
                {
                    double alpha = (map[i, j] + 1) / 2 * 255;
                    byte[] color = { (byte)alpha, (byte)alpha, (byte)alpha, 255 };
                    Int32Rect rect = new Int32Rect(i, j, 1, 1);
                    bmp.WritePixels(rect, color, 4, 0);
                }
            }
            return bmp;
        }
    }
}
