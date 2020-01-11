using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPI_ImageFilter
{
    class ImageUtils
    {
        private int height;
        private int width;

        private int[,] r;
        private int[,] g;
        private int[,] b;
        private int[,,] resultMatrix;

        public ImageUtils(Bitmap image)
        {
            height = image.Height;
            width = image.Width;
            
            r = new int[height, width];
            g = new int[height, width];
            b = new int[height, width];
            resultMatrix = new int[height, width, 3];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    Color pixelColor = image.GetPixel(j, i);
                    r[i, j] = pixelColor.R;
                    g[i, j] = pixelColor.G;
                    b[i, j] = pixelColor.B;
                }
        }

        public int[, ,] sobbelOperatorForSection(Bitmap image, int begin, int end, int blurringDegree)
        {
            for (int i = begin; i < end; i++)
            {
                for (int j = 1; j < width-1; j++)
                {
                    int[] averagePixelArray = convolutePixel(i, j, blurringDegree);
                    for (int k = 0; k < averagePixelArray.Length; k++)
                    {
                        resultMatrix[i, j, k] = averagePixelArray[k];
                    }
                }
            }

            return resultMatrix;
        }

        private int[] convolutePixel(int x, int y, int size)
        {
            int[] resultPixel = new int[3];
            resultPixel[0] = 0;
            resultPixel[1] = 0;
            resultPixel[2] = 0;

            int[,] kernel = new int[3, 3];
            for (int i = x - 1; i <= x + 1; i++)
               for (int j = y - 1; j <= y + 1; j++)
                    {
                       kernel[i - x + 1, j - y + 1] = (r[i, j]+g[i,j]+b[i,j])/3;
                    }
                            
            int resultX = 0;
            resultX += -1 * (kernel[0, 0] + kernel[2, 0]);
            resultX += -2 * kernel[1, 0];
            resultX += 1 * (kernel[0, 2] + kernel[2, 2]);
            resultX += 2 * kernel[1, 2];

            int resultY = 0;
            resultY += -1 * (kernel[0, 0] + kernel[0, 2]);
            resultY += -2 * kernel[0, 1];
            resultY += 1 * (kernel[2, 0] + kernel[2, 2]);
            resultY += 2 * kernel[2, 1];

            int resultFinal = Convert.ToInt32(Math.Sqrt(Math.Pow(resultX, 2) + Math.Pow(resultY, 2)));

            if (resultFinal > 255)
            {
                resultFinal = 255;
            }

            resultPixel[0] = resultFinal;
            resultPixel[1] = resultFinal;
            resultPixel[2] = resultFinal;

            return resultPixel;
        }

        public void saveModifiedImage(int[,,] pixelMatrix, string oldPath, string newPath)
        {
            Bitmap newImage = new Bitmap(oldPath);
            for (int i = 1; i < height-1; i++)
            {
                for (int j = 1; j < width-1; j++)
                {
                    int[] pixelColorArray = new int[3];
                    pixelColorArray[0] = pixelMatrix[i, j, 0];
                    pixelColorArray[1] = pixelMatrix[i, j, 1];
                    pixelColorArray[2] = pixelMatrix[i, j, 2];

                    Color pixelColor = Color.FromArgb(pixelColorArray[0], pixelColorArray[1], pixelColorArray[2]);
                    newImage.SetPixel(j, i, pixelColor);
                }
            }

            newImage.Save(newPath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        public int[,,] createPixelMatrix(int[,] r, int[,] g, int[,] b)
        {
            int[,,] pixelMatrix = new int[height, width, 3];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    int[] pixel = new int[3];
                    pixel[0] = r[i, j];
                    pixel[1] = g[i, j];
                    pixel[2] = b[i, j];
                    for (int k = 0; k < pixel.Length; k++)
                    {
                        pixelMatrix[i, j, k] = pixel[k];
                    }
                }

            return pixelMatrix;
        }
    }
}
