using ParametersSDK;
using Plugins.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK;

namespace ActiveContourFilter
{
    public class ActiveContourFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        static ActiveContourFilter()
        {
            parameters.Add(new ParametersInt32(1, 100, 60, "Num Points:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersInt32(1, 10000000, 10, "Num Steps:", ParameterDisplayTypeEnum.textBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int numPoints;
        private int numSteps;

        public ActiveContourFilter(int numPoints, int numSteps)
        {
            this.numPoints = numPoints;
            this.numSteps = numSteps;
        }

        public ImageDependencies getImageDependencies()
        {
            return null;
        }
        struct Point
        {
            public double x;
            public double y;
            public double speedY;
            public double speedX;
            public double accelX;
            public double accelY;
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark(String.Format("Active contour filter watermark"));

            double k = 0.001;
            int radius = 25;
            
            byte[,] inputLuminance = inputImage.getLuminance();
            int centerX = inputImage.getSizeX() / 2;
            int centerY = inputImage.getSizeY() / 2;

            int[,] outputImageResult = new int[inputImage.getSizeY() , inputImage.getSizeX() ];

            outputImageResult[centerY, centerX] = 255;
            Point[] points = new Point[numPoints];
            int n = 0;
            for(double i=0; i < 2*Math.PI && n < numPoints; i+=(2*Math.PI)/numPoints)
            {
                double sinRes = Math.Sin(i);
                double cosRes = Math.Cos(i);
                points[n] = new Point();
                points[n].x = centerY + (radius * sinRes);
                points[n].y = centerX + (radius * cosRes);
                n++;
            }


            for (int i = 0; i < numPoints; i++)
            {
                outputImageResult[(int)points[i].y, (int)points[i].x] = 255;
            }

            for (int s=0; s<numSteps; s++)
            {
                for (int i = 0; i < numPoints; i++)
                {
                    int up_i = i + 1;
                    int down_i = i - 1;

                    if(up_i >= numPoints-1)
                    {
                        up_i = 0;
                    }
                    
                    if(down_i < 0)
                    {
                        down_i = numPoints - 1;
                    }

                    double dx1 = points[down_i].x - points[i].x;
                    double dx2 = points[up_i].x - points[i].x;

                    double dy1 = points[down_i].y - points[i].y;
                    double dy2 = points[up_i].y - points[i].y;

                    points[i].accelX = k * dx1 + k * dx2; //elastic force, we approximate F=ma with a mass of 1
                    points[i].accelY = k * dy1 + k * dy2; // sum of forces that act on the object on both axes

                    points[i].speedY = points[i].speedY + points[i].accelY;
                    points[i].speedX = points[i].speedX + points[i].accelX;

                    if (points[i].speedY > 2)
                    {
                        points[i].speedY = 2;
                    }

                    if (points[i].speedY < -2)
                    {
                        points[i].speedY = -2;
                    }

                    if (points[i].speedX > 2)
                    {
                        points[i].speedX = 2;
                    }

                    if(points[i].speedX < - 2)
                    {
                        points[i].speedX = -2;
                    }

                    points[i].y = (points[i].y + points[i].speedY);
                    points[i].x = (points[i].x + points[i].speedX);
                }
            }

            for (int i = 0; i < numPoints; i++)
            {
                if((int)points[i].y >= 0 && (int)points[i].x >=0 && (int)points[i].x < outputImage.getSizeX() && (int)points[i].y < outputImage.getSizeY())
                {

                outputImageResult[(int)points[i].y, (int)points[i].x] = 100;
                }
            }

            ////////////////////////////////// to byte
            byte[,] outputGray = new byte[outputImageResult.GetLength(0), outputImageResult.GetLength(1)];
            for (int y = 0; y < outputGray.GetLength(0); y++)
            {
                for (int x = 0; x < outputGray.GetLength(1); x++)
                {
                    float r = outputImageResult[y, x];
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
    }
}
