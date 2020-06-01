using Plugins.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK;
using ParametersSDK;

namespace CannyFilter 
{
    public class CannyFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        private int gaussDim;
        private float gaussSigma;
        private int highThreshold;
        private int lowThreshold;

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

        public float applyConv(int x, int y, float[,] input, float[,] convMatrix)
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

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        static CannyFilter()
        {
            parameters.Add(new ParametersInt32(0, 10, 5, "Gauss Dim:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersFloat(0, 10, 1.4f, "Gauss Sigma:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersInt32(0, 255, 128, "High Threshold:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersInt32(0, 255, 64, "Low Threshold:", ParameterDisplayTypeEnum.textBox));
        }

        public CannyFilter(int gaussDim, float gaussSigma, int highThreshold, int lowThreshold)
        {
            this.gaussDim = gaussDim;
            this.gaussSigma = gaussSigma;
            this.highThreshold = highThreshold;
            this.lowThreshold = lowThreshold;
        }

        private int getDim()
        {
            return gaussDim;
        }

        private float[,] generateGaussianMatrix()
        {
            float[,] convMatrix = new float[getDim(), getDim()];
            int k = (getDim() - 1) / 2;
            float sum = 0;
            for(int y=1; y<=getDim(); y++)
            {
                for(int x=1; x<=getDim(); x++)
                {
                    int fst = y - (k + 1);
                    int snd = x - (k + 1);
                    convMatrix[y-1, x-1] = (float)(
                        (1 / (2 * Math.PI * gaussSigma * gaussSigma)) *
                        Math.Exp((fst*fst + snd*snd) /(-2*gaussSigma*gaussSigma))
                        );
                    
                    sum += convMatrix[y - 1, x - 1];
                }
            }

            for (int y = 0; y < getDim(); y++)
            {
                for (int x = 0; x < getDim(); x++)
                {
                    convMatrix[y, x] = convMatrix[y, x] / sum;
                }
            }
            
            return convMatrix;
        }

        /**
         * 

            For (x, y) in quadrant 1, 0 < θ < π/2.

            For (x, y) in quadrant 2, π/2 < θ≤π.

            For (x, y) in quadrant 3, -π < θ < -π/2.

            For (x, y) in quadrant 4, -π/2 < θ < 0.
        */

        enum DIRECTION
        {
            HORIZONTAL = 0,
            RIGHT45 = 1,
            VERTICAL = 2,
            LEFT45 = 3
        }

        private DIRECTION radiansToDirection(double radians)
        {
            
            DIRECTION dir = DIRECTION.LEFT45;
            if((radians >= - Math.PI/8 && radians <= Math.PI/8) || // 1st, 4th quadrant, la dreapta
                (radians >= Math.PI - Math.PI / 8 && radians <= -Math.PI + Math.PI / 8)) //2nd, 3rd q., la stanga
            {
                dir = DIRECTION.HORIZONTAL; 
            }

            if((radians > + Math.PI/8 && radians <= Math.PI/4 + Math.PI/8) ||// 1st quadrant, dreapta-sus
                (radians > -Math.PI + Math.PI / 8 && radians <= -Math.PI/2 - Math.PI / 8)) //3rd q. stanga jos
            {
                dir = DIRECTION.RIGHT45; 
            }

            if((radians > Math.PI / 4 + Math.PI / 8 && radians <= Math.PI/2+Math.PI/8) ||// 1st, 2nd q., sus
                (radians > -Math.PI / 2 - Math.PI / 8 && radians <= -Math.PI / 2 + Math.PI / 8))// 3rd, 4th q., jos
            {
                dir = DIRECTION.VERTICAL; 
            }

            if ((radians > Math.PI / 2 + Math.PI / 8 && radians <= Math.PI - Math.PI/8) ||// 2nd q., stanga-sus
                (radians > - Math.PI / 2 + Math.PI / 8 && radians <= -Math.PI/8)) // 4th q., dreapta-jos
            {
                dir = DIRECTION.LEFT45; 
            }
            
            /*
 DIRECTION dir = DIRECTION.LEFT45;
            if((radians >= 0 - Math.PI/8 && radians < 0 + Math.PI/8) || // 1st, 4th quadrant, la dreapta
                (radians >= Math.PI - Math.PI / 8 && radians < Math.PI + Math.PI / 8)) //2nd, 3rd q., la stanga
            {
                dir = DIRECTION.HORIZONTAL; 
            }

            if((radians >= 0 + Math.PI/8 && radians < Math.PI/4 + Math.PI/8) ||// 1st quadrant, dreapta-sus
                (radians >= Math.PI + Math.PI / 8 && radians < (3*Math.PI)/2 - Math.PI / 8)) //3rd q. stanga jos
            {
                dir = DIRECTION.RIGHT45; 
            }

            if((radians >= Math.PI / 4 + Math.PI / 8 && radians < Math.PI/2+Math.PI/8) ||// 1st, 2nd q., sus
                (radians >= (3 * Math.PI) / 2 - Math.PI / 8 && radians < (3 * Math.PI) / 2 + Math.PI / 8))// 3rd, 4th q., jos
            {
                dir = DIRECTION.VERTICAL; 
            }

            if ((radians >= Math.PI / 2 + Math.PI / 8 && radians < Math.PI - Math.PI/8) ||// 2nd q., stanga-sus
                (radians >= (3 * Math.PI) / 2 + Math.PI / 8 && radians < -Math.PI/8)) // 4th q., dreapta-jos
            {
                dir = DIRECTION.LEFT45; 
            }

            */           
            return dir;
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark(String.Format("canny watermark"));
            
            float[,] gaussianMatrix = generateGaussianMatrix();
            
            byte[,] inputLuminance = inputImage.getLuminance();

            float[,] outputImageGauss = new float[inputImage.getSizeY() - (gaussDim - 1), inputImage.getSizeX() - (gaussDim - 1)];

            /////////////////////////////////// input image -> GAUSS -> output image Gauss
            for (int y = gaussDim / 2; y < inputImage.getSizeY() - gaussDim / 2; y++)
            {
                for (int x = gaussDim / 2; x < inputImage.getSizeX() - gaussDim / 2; x++)
                {
                    float r = applyConv(x, y, inputLuminance, gaussianMatrix);
                    /*if (r < 0) r = 0;
                    if (r > 255) r = 255;*/

                    outputImageGauss[y - gaussDim / 2, x - gaussDim / 2] = r;
                }
            }

            /////////////////////////////////// output image Gauss -> SOBEL -> output image Sobel
            const int sobelDim = 3;            

            float[,] convMatrixGx = new float[sobelDim, sobelDim] {
                { 1, 0, -1},
                { 2, 0, -2},
                { 1, 0, -1}
            };

            float[,] convMatrixGy = new float[sobelDim, sobelDim]
            {
                { 1, 2, 1},
                { 0, 0, 0},
                {-1, -2, -1}
            };
            
            float[,] outputImageSobel = new float[outputImageGauss.GetLength(0) - (sobelDim - 1), outputImageGauss.GetLength(1) - (sobelDim - 1)];
            DIRECTION[,] outputImageDirection = new DIRECTION[outputImageGauss.GetLength(0) - (sobelDim - 1), outputImageGauss.GetLength(1) - (sobelDim - 1)];
            for (int y = sobelDim / 2; y < outputImageGauss.GetLength(0) - sobelDim / 2; y++)
            {
                for (int x = sobelDim / 2; x < outputImageGauss.GetLength(1) - sobelDim / 2; x++)
                {
                    float resx = applyConv(x, y, outputImageGauss, convMatrixGx);
                    float resy = applyConv(x, y, outputImageGauss, convMatrixGy);

                    double r = Math.Sqrt((resx * resx) + (resy * resy));
                    /*if (r < 0) r = 0;
                    if (r > 255) r = 255;*/

                    outputImageSobel[y - sobelDim / 2, x - sobelDim / 2] = (float)r;
                    outputImageDirection[y - sobelDim / 2, x - sobelDim / 2] = radiansToDirection(Math.Atan2(resy, resx));
                }
            }

            /////////////////////////////////// output image Sobel & direction -> non maximum suppression -> outputImageNMS float
            float[,] floatOutputImageNms = new float[outputImageSobel.GetLength(0), outputImageSobel.GetLength(1)];
            for (int y = 0; y < outputImageSobel.GetLength(0); y++)
            {
                for (int x = 0; x < outputImageSobel.GetLength(1); x++)
                {
                    switch(outputImageDirection[y,x])
                    {
                        case DIRECTION.HORIZONTAL:
                            if(x >= 1 && x < outputImageSobel.GetLength(1)-1 && 
                                outputImageSobel[y, x] > outputImageSobel[y, x-1] && 
                                outputImageSobel[y, x] >= outputImageSobel[y, x+1])
                            {
                                floatOutputImageNms[y, x] = outputImageSobel[y, x];
                            }
                            break;
                        case DIRECTION.LEFT45:
                        //case DIRECTION.RIGHT45:
                            if (x >= 1 && x < outputImageSobel.GetLength(1) - 1 &&
                                y >= 1 && y < outputImageSobel.GetLength(0) - 1 &&
                                outputImageSobel[y, x] >= outputImageSobel[y - 1, x + 1] &&
                                outputImageSobel[y, x] > outputImageSobel[y + 1, x - 1])
                            {
                                floatOutputImageNms[y, x] = outputImageSobel[y, x];
                            }
                            break;
                        case DIRECTION.VERTICAL:
                            if (y >= 1 && y < outputImageSobel.GetLength(0) - 1 &&
                                outputImageSobel[y, x] > outputImageSobel[y - 1, x] &&
                                outputImageSobel[y, x] >= outputImageSobel[y + 1, x])
                            {
                                floatOutputImageNms[y, x] = outputImageSobel[y, x];
                            }
                            break;
                        //case DIRECTION.LEFT45:
                        case DIRECTION.RIGHT45:
                            if (x >= 1 && x < outputImageSobel.GetLength(1) - 1 &&
                                y >= 1 && y < outputImageSobel.GetLength(0) - 1 &&
                                outputImageSobel[y, x] > outputImageSobel[y - 1, x - 1] &&
                                outputImageSobel[y, x] >= outputImageSobel[y + 1, x + 1])
                            {
                                floatOutputImageNms[y, x] = outputImageSobel[y, x];
                            }
                            break;
                    }
                }
            }

            /////////////////////////////////// output image non maximum suppression float -> acceptance matrix & byte outputNms
            bool[,] acceptanceMatrix = new bool[floatOutputImageNms.GetLength(0), floatOutputImageNms.GetLength(1)];
            byte[,] outputImageNms = new byte[floatOutputImageNms.GetLength(0), floatOutputImageNms.GetLength(1)];

            for (int y = 0; y < floatOutputImageNms.GetLength(0); y++)
            {
                for (int x = 0; x < floatOutputImageNms.GetLength(1); x++)
                {
                    float r = floatOutputImageNms[y, x];
                    if (r > 255)
                    {
                        r = 255;
                    }

                    if (r <0)
                    {
                        r = 0;
                    }

                    outputImageNms[y, x] = (byte)r;
                    acceptanceMatrix[y, x] = outputImageNms[y, x] >= highThreshold;
                }
            }
            
            Queue<KeyValuePair<int, int>> neighbors = new Queue<KeyValuePair<int, int>>();

            for (int y = 0; y < outputImageNms.GetLength(0); y++)
            {
                for (int x = 0; x < outputImageNms.GetLength(1); x++)
                {
                    if (acceptanceMatrix[y,x])
                    {
                        if(y >= 1 &&
                            outputImageNms[y-1, x] >= lowThreshold && outputImageNms[y-1, x] < highThreshold) // up
                        {
                            neighbors.Enqueue(new KeyValuePair<int, int>(y-1, x));
                        }

                        if (y < outputImageNms.GetLength(0) - 1 &&
                            outputImageNms[y + 1, x] >= lowThreshold && outputImageNms[y + 1, x] < highThreshold) // down
                        {
                            neighbors.Enqueue(new KeyValuePair<int, int>(y+1, x));
                        }

                        if (x < outputImageNms.GetLength(1) - 1 &&
                            outputImageNms[y, x+1] >= lowThreshold && outputImageNms[y, x+1] < highThreshold) // right
                        {
                            neighbors.Enqueue(new KeyValuePair<int, int>(y, x+1));
                        }

                        if (x >= 1 &&
                            outputImageNms[y, x - 1] >= lowThreshold && outputImageNms[y, x - 1] < highThreshold) // left
                        {
                            neighbors.Enqueue(new KeyValuePair<int, int>(y, x-1));
                        }

                        if(y >= 1 && x < outputImageNms.GetLength(1) - 1 &&
                            outputImageNms[y - 1, x + 1] >= lowThreshold && outputImageNms[y - 1, x + 1] < highThreshold) //right-up
                        {
                            neighbors.Enqueue(new KeyValuePair<int, int>(y-1, x + 1));
                        }

                        if (x >= 1 && y >= 1 &&
                            outputImageNms[y-1, x - 1] >= lowThreshold && outputImageNms[y-1, x - 1] < highThreshold) // left-up
                        {
                            neighbors.Enqueue(new KeyValuePair<int, int>(y-1, x - 1));
                        }

                        if (y < outputImageNms.GetLength(0) - 1 && x >= 1 &&
                            outputImageNms[y + 1, x-1] >= lowThreshold && outputImageNms[y + 1, x-1] < highThreshold) // left-down
                        {
                            neighbors.Enqueue(new KeyValuePair<int, int>(y + 1, x-1));
                        }

                        if (x < outputImageNms.GetLength(1) - 1 && y < outputImageNms.GetLength(0) - 1 &&
                            outputImageNms[y + 1, x + 1] >= lowThreshold && outputImageNms[y + 1, x + 1] < highThreshold) // right-down
                        {
                            neighbors.Enqueue(new KeyValuePair<int, int>(y + 1, x + 1));
                        }
                    }
                }
            }

            while(neighbors.Count > 0)
            {
                KeyValuePair<int, int> xy = neighbors.Dequeue();
                int y = xy.Key;
                int x = xy.Value;
                if (!acceptanceMatrix[y, x])
                {
                    acceptanceMatrix[y, x] = true;
                    
                    if (y >= 1 &&
                        outputImageNms[y - 1, x] >= lowThreshold && outputImageNms[y - 1, x] < highThreshold) // up
                    {
                        neighbors.Enqueue(new KeyValuePair<int, int>(y-1, x));
                    }

                    if (y < outputImageNms.GetLength(0) - 1 &&
                        outputImageNms[y + 1, x] >= lowThreshold && outputImageNms[y + 1, x] < highThreshold) // down
                    {
                        neighbors.Enqueue(new KeyValuePair<int, int>(y+1, x));
                    }

                    if (x < outputImageNms.GetLength(1) - 1 &&
                        outputImageNms[y, x + 1] >= lowThreshold && outputImageNms[y, x + 1] < highThreshold) // right
                    {
                        neighbors.Enqueue(new KeyValuePair<int, int>(y, x+1));
                    }

                    if (x >= 1 &&
                        outputImageNms[y, x - 1] >= lowThreshold && outputImageNms[y, x - 1] < highThreshold) // left
                    {
                        neighbors.Enqueue(new KeyValuePair<int, int>(y, x-1));
                    }
                    
                    if (y >= 1 && x < outputImageNms.GetLength(1) - 1 &&
                        outputImageNms[y - 1, x + 1] >= lowThreshold && outputImageNms[y - 1, x + 1] < highThreshold) //right-up
                    {
                        neighbors.Enqueue(new KeyValuePair<int, int>(y - 1, x + 1));
                    }

                    if (x >= 1 && y >= 1 &&
                        outputImageNms[y - 1, x - 1] >= lowThreshold && outputImageNms[y - 1, x - 1] < highThreshold) // left-up
                    {
                        neighbors.Enqueue(new KeyValuePair<int, int>(y - 1, x - 1));
                    }

                    if (y < outputImageNms.GetLength(0) - 1 && x >= 1 &&
                        outputImageNms[y + 1, x - 1] >= lowThreshold && outputImageNms[y + 1, x - 1] < highThreshold) // left-down
                    {
                        neighbors.Enqueue(new KeyValuePair<int, int>(y + 1, x - 1));
                    }

                    if (x < outputImageNms.GetLength(1) - 1 && y < outputImageNms.GetLength(0) - 1 &&
                        outputImageNms[y + 1, x + 1] >= lowThreshold && outputImageNms[y + 1, x + 1] < highThreshold) // right-down
                    {
                        neighbors.Enqueue(new KeyValuePair<int, int>(y + 1, x + 1));
                    }
                }
            }

            /////////////////////////////////// output image non maximum suppression  -> histeresis threshold-> outputImageHisteresis
            float[,] outputImageHist = new float[outputImageNms.GetLength(0), outputImageNms.GetLength(1)];
            
            for (int y = 0; y < outputImageNms.GetLength(0); y++)
            {
                for (int x = 0; x < outputImageNms.GetLength(1); x++)
                {
                    if (acceptanceMatrix[y,x])
                    {
                        outputImageHist[y, x] = outputImageNms[y, x];
                    }
                }
            }

            ////////////////////////////////// to byte
            byte[,] outputGray = new byte[outputImageHist.GetLength(0), outputImageHist.GetLength(1)];
            for(int y = 0; y < outputGray.GetLength(0); y++)
            {
                for (int x = 0; x < outputGray.GetLength(1); x++)
                {
                    float r = outputImageHist[y, x];
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
