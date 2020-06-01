using Plugins.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK;
using ParametersSDK;

namespace ConvFilter
{
    public class ConvFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        private int dim;

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        static ConvFilter()
        {
            parameters.Add(new ParametersInt32(0, 10, 3, "Dim:", ParameterDisplayTypeEnum.textBox));
        }

        public ConvFilter(int dim)
        {
            this.dim = dim;
        }

        public  byte applyConv(int x, int y, byte[,] input, float[,] convMatrix)
        {
            float r = 0;

            for(int i = -convMatrix.GetLength(0)/2; i < convMatrix.GetLength(0)/2; i++)
            {
                for (int j = -convMatrix.GetLength(1) / 2; j < convMatrix.GetLength(1)/2; j++)
                {
                    r += convMatrix[i+ convMatrix.GetLength(0) / 2, j+ convMatrix.GetLength(1)/2] * input[y + i, x + j];
                }
            }

            if (r < 0) r = 0;
            if (r > 255) r = 255;

            return (byte)r;
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark(String.Format("conv watermark, dim: {0}", dim));

            float[,] convMatrix = new float[dim, dim];

            for(int i=0; i<dim; i++)
            {
                for(int j=0; j<dim; j++)
                {
                    convMatrix[i, j] = (float)(1.0 / (dim*dim));
                }
            }

            if (inputImage.grayscale)
            {
                byte[,] outputGray = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] inputGray = inputImage.getGray();

                for (int y = dim/2; y < inputImage.getSizeY() - dim/2; y++)
                {
                    for (int x = dim/2; x < inputImage.getSizeX() - dim/2; x++)
                    {
                        outputGray[y,x] = applyConv(x, y, inputGray, convMatrix);
                    }
                }
                outputImage.setGray(outputGray);
            }
            else
            {
                byte[,] outputRed = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] outputGreen = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] outputBlue = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] inputRed = inputImage.getRed();
                byte[,] inputGreen = inputImage.getGreen();
                byte[,] inputBlue = inputImage.getBlue();


                for (int y = dim / 2; y < inputImage.getSizeY() - dim / 2; y++)
                {
                    for (int x = dim / 2; x < inputImage.getSizeX() - dim / 2; x++)
                    {
                        outputRed[y, x] = applyConv(x, y, inputRed, convMatrix);
                        outputGreen[y, x] = applyConv(x, y, inputGreen, convMatrix);
                        outputBlue[y, x] = applyConv(x, y, inputBlue, convMatrix);
                    }
                }

                outputImage.setRed(outputRed);
                outputImage.setGreen(outputGreen);
                outputImage.setBlue(outputBlue);
            }

            return outputImage;
        }

        public ImageDependencies getImageDependencies()
        {
            return null;
        }
    }
}
