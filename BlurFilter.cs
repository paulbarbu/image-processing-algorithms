using Plugins.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK;
using ParametersSDK;

namespace BlurFilter
{
    public class BlurFilter : IFilter
    {

        private static readonly List<IParameters> parameters = new List<IParameters>();

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        static BlurFilter()
        {
        }

        public BlurFilter()
        {
        }
        
        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark("blur watermark: media aritmetica in cruce (sus, jo, centru, stanga, dreapta)");

            if(inputImage.grayscale)
            {
                byte[,] outputGray = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] inputGray = inputImage.getGray();
                
                for (int y = 1; y < inputImage.getSizeY() - 1; y++)
                {
                    for (int x = 1; x < inputImage.getSizeX() - 1; x++)
                    {
                        outputGray[y, x] = (byte)((
                                               inputGray[y-1, x] +
                            +inputGray[y, x-1] + inputGray[y, x]*0   + inputGray[y, x+1] +
                                               inputGray[y+1, x] 
                            )/5);
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


                for (int y = 1; y < inputImage.getSizeY()-1; y++)
                {
                    for (int x = 1; x < inputImage.getSizeX()-1; x++)
                    {
                        outputRed[y, x] = (byte)((
                                               inputRed[y - 1, x] +
                            +inputRed[y, x - 1] + inputRed[y, x] * 0 + inputRed[y, x + 1] +
                                               inputRed[y + 1, x]
                            ) / 5);
                        outputGreen[y, x] = (byte)((
                                               inputGreen[y - 1, x] +
                            +inputGreen[y, x - 1] + inputGreen[y, x] * 0 + inputGreen[y, x + 1] +
                                               inputGreen[y + 1, x]
                            ) / 5);
                        outputBlue[y, x] = (byte)((
                                               inputBlue[y - 1, x] +
                            +inputBlue[y, x - 1] + inputBlue[y, x] * 0 + inputBlue[y, x + 1] +
                                               inputBlue[y + 1, x]
                            ) / 5);
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
