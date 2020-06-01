using Plugins.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK;
using ParametersSDK;

namespace SobelFilter
{
    public class SobelFilter : IFilter
    {
        static SobelFilter()
        {

        }
        private static readonly List<IParameters> parameters = new List<IParameters>();

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        public float applyConv(int x, int y, byte[,] input, float[,] convMatrix)
        {
            float r = 0;

            for (int i = -convMatrix.GetLength(0) / 2; i <= convMatrix.GetLength(0) / 2; i++)
            {
                for (int j = -convMatrix.GetLength(1) / 2; j <= convMatrix.GetLength(1) / 2; j++)
                {
                    r += convMatrix[i + convMatrix.GetLength(0) / 2, j + convMatrix.GetLength(1) / 2] * input[y + i, x + j];
                }
            }

            /*if (r < 0) r = 0;
            if (r > 255) r = 255;

            return (byte)r;*/
            return r;
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark(String.Format("sobel watermark"));
            const int dim = 3;

            float[,] convMatrixGx = new float[dim, dim] {
                { 1, 0, -1},
                { 2, 0, -2},
                { 1, 0, -1}
            };

            float[,] convMatrixGy = new float[dim, dim]
            {
                { 1, 2, 1},
                { 0, 0, 0},
                {-1, -2, -1}
            };
            
            byte[,] inputLuminance = inputImage.getLuminance();
            
            byte[,] outputGray = new byte[inputImage.getSizeY() - (dim -1), inputImage.getSizeX() - (dim - 1)];
            
            for (int y = dim / 2; y < inputImage.getSizeY() - dim / 2; y++)
            {
                for (int x = dim / 2; x < inputImage.getSizeX() - dim / 2; x++)
                {
                    float resx =  applyConv(x, y, inputLuminance, convMatrixGx);
                    float resy = applyConv(x, y, inputLuminance, convMatrixGy);
                    
                    double r  = Math.Sqrt((resx*resx) + (resy*resy));
                    if (r < 0) r = 0;
                    if (r > 255) r = 255;

                    outputGray[y - dim / 2, x - dim / 2] = (byte)r;
                }
            }

            outputImage.setSizeX(inputImage.getSizeX() - (dim - 1));
            outputImage.setSizeY(inputImage.getSizeY() - (dim - 1));
            outputImage.setGray(outputGray);

            return outputImage;
        }

        public ImageDependencies getImageDependencies()
        {
            return null;
        }
    }
}
