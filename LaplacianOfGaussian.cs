using Plugins.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK;
using ParametersSDK;

namespace LaplacianOfGaussianFilter
{
    public class LaplacianOfGaussian : IFilter
    {
        private static string[] intervalEnumValues = { "stretch", "truncate" };

        private static readonly List<IParameters> parameters = new List<IParameters>();
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

            return r;
        }

        static LaplacianOfGaussian()
        {
            
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark(String.Format("LoG watermark"));


            // intout -> laplacian of gaussian -> output LOG
            const int logDim = 5;
            float[,] laplacianOfGaussian = new float[logDim, logDim]
            {
                { 0, 0, -1, 0, 0},
                { 0, -1, -2, -1, 0},
                { -1, -2, 16, -2, -1},
                { 0, -1, -2, -1, 0},
                { 0, 0, -1, 0, 0},
            };

            byte[,] inputLuminance = inputImage.getLuminance();

            int[,] outputImageLoG = new int[inputImage.getSizeY() - (logDim - 1), inputImage.getSizeX() - (logDim - 1)];
            
            for (int y = logDim/ 2; y < inputImage.getSizeY() - logDim/ 2; y++) //getlength0
            {
                for (int x = logDim/ 2; x < inputImage.getSizeX() - logDim/ 2; x++) //getlength1
                {
                    float r = applyConv(x, y, inputLuminance, laplacianOfGaussian);
                    
                    outputImageLoG[y - logDim/ 2, x - logDim/ 2] = (int)r;
                }
            }

            // LOG -> zero crossing -> output
            byte[,] outputImageZC = new byte[outputImageLoG.GetLength(0), outputImageLoG.GetLength(1)];
            for (int y = 0; y < outputImageLoG.GetLength(0); y++) 
            {
                for (int x = 0; x < outputImageLoG.GetLength(1); x++) 
                {
                    bool zeroCrossing = false;
                    //left
                    if(x >=1 && Math.Sign(outputImageLoG[y, x]) != Math.Sign(outputImageLoG[y, x-1]))
                    {
                        zeroCrossing = true;
                    }
                    //upper left
                    else if(x >= 1 && y >= 1 && Math.Sign(outputImageLoG[y, x]) != Math.Sign(outputImageLoG[y-1, x - 1]))
                    {
                        zeroCrossing = true;
                    }
                    //up
                    else if(y >= 1 && Math.Sign(outputImageLoG[y,x]) != Math.Sign(outputImageLoG[y-1, x]))
                    {
                        zeroCrossing = true;
                    }
                    //upper right
                    else if(y >= 1 && x < outputImageLoG.GetLength(1) - 1 
                        && Math.Sign(outputImageLoG[y , x]) != Math.Sign(outputImageLoG[y - 1, x+1]))
                    {
                        zeroCrossing = true;
                    }

                    if(zeroCrossing)
                    {
                        outputImageZC[y, x] = 255;
                    }
                }
            }


            ////////////////////////////////// to byte
            byte[,] outputGray = new byte[outputImageZC.GetLength(0), outputImageZC.GetLength(1)];
            for (int y = 0; y < outputGray.GetLength(0); y++)
            {
                for (int x = 0; x < outputGray.GetLength(1); x++)
                {
                    float r = outputImageZC[y, x];
                    if (r < 0) r = 0;
                    if (r > 255) r = 255;
                    outputGray[y, x] = (byte)r;
                }
            }

            outputImage.setSizeX(outputGray.GetLength(1));
            outputImage.setSizeY(outputGray.GetLength(0));
            outputImage.setGray(outputGray);

            return outputImage;
        }

        public ImageDependencies getImageDependencies()
        {
            return null;
        }
    }
}
