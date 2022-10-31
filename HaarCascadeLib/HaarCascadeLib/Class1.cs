using System.Drawing;

namespace HaarCascadeLib
{
    public class imageProcessorClass
    {
        public void imageToGrayscale(Bitmap imageIn)
        {
            for (int i = 0; i < imageIn.Width; i++)
            {
                for (int j = 0; j < imageIn.Height; j++)
                {
                    //works out the average colour and then replaces the red green and
                    //blue values with this so that there is no colour, only intensity
                    Color col = imageIn.GetPixel(i, j);
                    int avgColour = (col.R + col.G + col.B) / 3;
                    imageIn.SetPixel(i, j, Color.FromArgb(col.A, avgColour, avgColour, avgColour));
                }
            }
        }

        public Bitmap pixelByPixelScaler(Bitmap imageIn)
        {
            double originalWidth = imageIn.Width;
            /*int sf = (int)(pictureBox.Width / originalWidth);*/
            int sf = 3;
            Bitmap imageOut = new Bitmap(imageIn.Width * sf, imageIn.Height * sf);

            for (int i = 0; i < imageOut.Width; i++)
            {
                for (int j = 0; j < imageOut.Height; j++)
                {
                    if (i % sf == 0 && j % sf == 0)
                    {
                        Color colour = imageIn.GetPixel(i % sf, j % sf);
                        imageOut.SetPixel(i, j, colour);
                    }
                    else
                    {
                        imageOut.SetPixel(i, j, Color.White);
                    }
                }
            }

            return imageOut;
        }

        public Bitmap NearestNeighborScaleUp(Bitmap imageIn, PictureBox pictureBox)
        {
            double originalWidth = imageIn.Width;
            int sf = (int)(pictureBox.Width / originalWidth);
            Bitmap imageOut = new Bitmap(imageIn.Width * sf, imageIn.Height * sf);

            for (int i = 0; i < imageOut.Width; i++)
            {
                for (int j = 0; j < imageOut.Height; j++)
                {
                    if(i % sf == 0 && j % sf == 0)
                    {
                        int newColour = imageIn.GetPixel(i / sf, j / sf).R;
                        imageOut.SetPixel(i, j, Color.FromArgb(100, newColour, newColour, newColour));
                    }
                    else
                    {
                        //finds the index of the pixel in the original image
                        int localIdxI = sf - i % sf;
                        int localIdxJ = sf - j % sf;
                        if ((int)((Math.Floor((double)(i / sf))) + 1) < imageIn.Width && (int)((Math.Floor((double)(j / sf))) + 1) < imageIn.Height)
                        {
                            if (localIdxI >= sf / 2)
                            {
                                if (localIdxJ >= sf / 2)
                                {
                                    int newColour = imageIn.GetPixel((int)((Math.Floor((double)(i / sf))) + 1), (int)((Math.Floor((double)(j / sf))) + 1)).R;
                                    imageOut.SetPixel(i, j, Color.FromArgb(100, newColour, newColour, newColour));
                                }
                                else
                                {
                                    int newColour = imageIn.GetPixel((int)((Math.Floor((double)(i / sf))) + 1), (int)(Math.Floor((double)(j / sf)))).R;
                                    imageOut.SetPixel(i, j, Color.FromArgb(100, newColour, newColour, newColour));
                                }
                            }
                        }
                        else if((int)((Math.Floor((double)(i / sf))) + 1) >= imageIn.Width && (int)((Math.Floor((double)(j / sf))) + 1) < imageIn.Height)
                        {
                            if (localIdxJ >= sf / 2)
                            {
                                int newColour = imageIn.GetPixel((int)((Math.Floor((double)(i / sf)))), (int)((Math.Floor((double)(j / sf))) + 1)).R;
                                imageOut.SetPixel(i, j, Color.FromArgb(100, newColour, newColour, newColour));
                            }
                        } 
                        if(localIdxI < sf / 2)
                        {
                            if (localIdxJ < sf / 2)
                            {
                                int newColour = imageIn.GetPixel((int)((Math.Floor((double)(i / sf)))), (int)((Math.Floor((double)(j / sf))))).R;
                                imageOut.SetPixel(i, j, Color.FromArgb(100, newColour, newColour, newColour));
                            }
                            else
                            {
                                if ((int)((Math.Floor((double)(j / sf))) + 1) < imageIn.Height)
                                {
                                    int newColour = imageIn.GetPixel((int)((Math.Floor((double)(i / sf)))), (int)((Math.Floor((double)(j / sf))) + 1)).R;
                                    imageOut.SetPixel(i, j, Color.FromArgb(100, newColour, newColour, newColour));
                                }
                                else
                                {
                                    int newColour = imageIn.GetPixel((int)((Math.Floor((double)(i / sf)))), (int)((Math.Floor((double)(j / sf))))).R;
                                    imageOut.SetPixel(i, j, Color.FromArgb(100, newColour, newColour, newColour));
                                }
                            }
                        }
                    }
                    
                }
            }
            return imageOut;
        }

        public Bitmap BicubicScaleUp(Bitmap imageIn, PictureBox pictureBox, RichTextBox richTextBox1)
        {
            //im only going to handle integer scale factors as i want my pixels to have 1:1 mappings with equal pixels in between where a 3sf transformation will have 2 pixels between each original pixel
            int sf = pictureBox.Width / imageIn.Width;
            if (sf > 3)
            {
                Console.WriteLine("image too small to preserve quality");
                richTextBox1.Text = "current image too small to preserve quality";
                return imageIn;
            }
            else if (sf > 1)
            {
                richTextBox1.Text = "";
                int newX = imageIn.Width * sf;
                int newY = imageIn.Height * sf;

                Bitmap newImg = new Bitmap(newX, newY);

                double[,] colourGradX = new double[imageIn.Width, imageIn.Height];
                double[,] colourGradY = new double[imageIn.Width, imageIn.Height];
                GetGrads(imageIn, ref colourGradX, ref colourGradY);

                //I will work out the colour of the points in the x direction and y direction first and
                //then take the average of each pixel in the x and y direction to incorporate both directions
                double[,] predictedX = new double[newX, newY];
                double[,] predictedY = new double[newX, newY];

                double[] coefX = new double[4];
                double[] coefY = new double[4];

                int LastVal = 0;
                bool isCubic = true;
                //predicts the values of the pixels on the bottom row
                for (int k = 0; k < sf; k++)
                {
                    for (int i = 0; i < newX; i++)
                    {
                        if (i % sf == 0 && (newY - 1) % sf == 0)
                        {
                            if (colourGradX[i, imageIn.Height - (k) - 1] == 0 && colourGradX[i + 1, imageIn.Height - (k) - 1] == 0)
                            {
                                isCubic = false;
                                LastVal = imageIn.GetPixel(i / sf, imageIn.Height - (k) - 1).R;
                            }
                            else
                            {
                                isCubic = true;
                                coefX = GetCubicBetweenPoints(i, imageIn.GetPixel(i / sf, imageIn.Height - (k) - 1).R, i + 1, imageIn.GetPixel((i / sf) + 1, imageIn.Height - (k) - 1).R, colourGradX[i, imageIn.Height - (k) - 1], colourGradX[i + 1, imageIn.Height - (k) - 1]);
                                if (coefX.Contains<double>(double.NaN))
                                {
                                    isCubic = false;
                                    LastVal = imageIn.GetPixel(i / sf, imageIn.Height - (k) - 1).R;
                                }
                            }
                        }
                        if (isCubic)
                        {
                            predictedX[i, newY - 1 - k] = coefX[0] * Math.Pow(i, 3) + coefX[3] * Math.Pow(i, 2) + coefX[2] * (i) + coefX[1];
                        }
                        else
                        {
                            predictedX[i, newY - 1 - k] = LastVal;
                        }
                    }
                }

                for (int k = 0; k < sf; k++)
                {
                    LastVal = 0;
                    isCubic = true;
                    //predicts pixel values of the rightmost column
                    for (int j = 0; j < newY; j++)
                    {
                        if ((newX - 1) % sf == 0 && j % sf == 0)
                        {
                            if (colourGradX[imageIn.Width - (k) - 1, j / sf] == 0 && colourGradX[imageIn.Width - (k) - 1, j / sf + 1] == 0)
                            {
                                LastVal = imageIn.GetPixel(imageIn.Width - (k) - 1 / sf, j / sf).R;
                                isCubic = false;
                            }
                            else
                            {
                                isCubic = true;
                                coefY = GetCubicBetweenPoints(j, imageIn.GetPixel((imageIn.Width - 1 - k) / sf, j / sf).R, j + 1, imageIn.GetPixel((imageIn.Width - 1 - (k)) / sf, (j / sf) + 1).R, colourGradY[imageIn.Width - (k) - 1, j / sf], colourGradY[imageIn.Width - (k) - 1, (j / sf) + 1]);
                                if (coefX.Contains<double>(double.NaN))
                                {
                                    isCubic = false;
                                    LastVal = imageIn.GetPixel(imageIn.Width - (k) - 1 / sf, j / sf).R;
                                }
                            }
                        }
                        if (isCubic)
                        {
                            predictedY[newX - 1 - k, j] = coefY[0] * Math.Pow(j, 3) + coefY[3] * Math.Pow(j, 2) + coefY[2] * (j) + coefY[1];
                        }
                        else
                        {
                            predictedY[newX - 1 - k, j] = LastVal;
                        }
                    }
                }


                bool isCubicY = true;
                double lastY = 0;
                //preicts rest of pixel values in between
                for (int i = 0; i < newX - sf; i++)
                {
                    for (int j = 0; j < newY - sf; j++)
                    {

                        if (j % sf == 0)
                        {
                            if (colourGradY[i / sf, j / sf] == 0 && colourGradY[i / sf, (j / sf) + 1] == 0)
                            {
                                isCubicY = false;
                                lastY = imageIn.GetPixel(i / sf, j / sf).R;
                            }
                            else
                            {
                                isCubicY = true;
                                coefY = GetCubicBetweenPoints(j / sf, imageIn.GetPixel(i / sf, j / sf).R, (j / sf) + 1, imageIn.GetPixel(i / sf, j / sf + 1).R, colourGradX[i / sf, j / sf], colourGradX[i / sf, j / sf + 1]);
                                if (coefX.Contains<double>(double.NaN))
                                {
                                    isCubic = false;
                                    lastY = imageIn.GetPixel(i / sf, j / sf).R;
                                }
                            }
                        }

                        if (isCubicY)
                        {
                            predictedY[i, j] = coefY[0] * Math.Pow(j / sf + j % sf, 3) + coefY[3] * Math.Pow(j / sf + j % sf, 2) + coefY[2] * (j / sf + j % sf) + coefY[1];
                        }
                        else
                        {
                            predictedY[i, j] = lastY;
                        }
                    }

                }

                bool isCubicX = true;
                double lastX = 0;
                for (int j = 0; j < newY - sf; j++)
                {
                    for (int i = 0; i < newX - sf; i++)
                    {
                        if (i % sf == 0)
                        {
                            if (colourGradX[i / sf, j / sf] == 0 && colourGradX[(i / sf) + 1, j / sf] == 0)
                            {
                                isCubicX = false;
                                lastX = imageIn.GetPixel(i / sf, j / sf).R;
                            }
                            else
                            {
                                isCubicX = true;
                                coefX = GetCubicBetweenPoints(i / sf, imageIn.GetPixel(i / sf, j / sf).R, (i / sf) + 1, imageIn.GetPixel(i / sf + 1, j / sf).R, colourGradX[i / sf, j / sf], colourGradX[i / sf + 1, j / sf]);
                                if (coefX.Contains<double>(double.NaN))
                                {
                                    isCubic = false;
                                    lastX = imageIn.GetPixel(i / sf, j / sf).R;
                                }
                            }
                            if (isCubicX)
                            {
                                predictedX[i, j] = coefX[0] * Math.Pow(i / sf + i % sf, 3) + coefX[3] * Math.Pow(i / sf + i % sf, 2) + coefX[2] * (i / sf + i % sf) + coefX[1];
                            }
                            else
                            {
                                predictedX[i, j] = lastX;
                            }
                        }
                    }
                }

                //averages the predicted y and predicted x values and puts it into the new image
                for (int i = 0; i < predictedX.GetLength(0); i++)
                {
                    for (int j = 0; j < predictedX.GetLength(1); j++)
                    {
                        //conditionals for insurance but the
                        //program functions well without
                        if (predictedY[i, j] > 255)
                        {
                            predictedY[i, j] = 255;
                        }
                        else if (predictedY[i, j] < 0)
                        {
                            predictedY[i, j] = 0;
                        }
                        if (predictedX[i, j] > 255)
                        {
                            predictedX[i, j] = 255;
                        }
                        else if (predictedX[i, j] < 0)
                        {
                            predictedX[i, j] = 0;
                        }
                        //certain pixels on the edge of the picture come up
                        //as NaNs but this shouldnt effect the end result
                        if (Double.IsNaN(predictedX[i, j]))
                        {
                            predictedX[i, j] = 0;
                        }
                        if (Double.IsNaN(predictedY[i, j]))
                        {
                            predictedY[i, j] = 0;
                        }

                        //sets pixel in new image
                        newImg.SetPixel(i, j, Color.FromArgb(Convert.ToInt32((predictedX[i, j] + predictedY[i, j]) / 2), Convert.ToInt32((predictedX[i, j] + predictedY[i, j]) / 2), Convert.ToInt32((predictedX[i, j] + predictedY[i, j]) / 2)));
                    }
                }

                return newImg;
            }
            return imageIn;
        }

        public void GetGrads(Bitmap imageIn, ref double[,] colourGradX, ref double[,] colourGradY)
        {
            for (int i = 0; i < imageIn.Height; i++)
            {
                colourGradX[0, i] = (imageIn.GetPixel(1, i).R - imageIn.GetPixel(0, i).R);
                colourGradX[imageIn.Width - 1, i] = (imageIn.GetPixel(imageIn.Width - 1, i).R - imageIn.GetPixel(imageIn.Width - 2, i).R);
            }
            for (int i = 0; i < imageIn.Width; i++)
            {
                colourGradY[i, 0] = (imageIn.GetPixel(i, 1).R - imageIn.GetPixel(i, 0).R);
                colourGradY[i, imageIn.Height - 1] = (imageIn.GetPixel(i, imageIn.Height - 1).R - imageIn.GetPixel(i, imageIn.Height - 2).R);
            }



            for (int i = 1; i < imageIn.Width - 1; i++)
            {
                for (int j = 0; j < imageIn.Height; j++)
                {
                    colourGradX[i, j] = (imageIn.GetPixel(i + 1, j).R - imageIn.GetPixel(i - 1, j).R) / 2;
                }
            }
            for (int i = 0; i < imageIn.Width; i++)
            {
                for (int j = 1; j < imageIn.Height - 1; j++)
                {
                    colourGradY[i, j] = (imageIn.GetPixel(i, j + 1).R - imageIn.GetPixel(i, j - 1).R) / 2;
                }
            }
        }

        public double[] GetCubicBetweenPoints(int x1, int y1, int x2, int y2, double grad1, double grad2)
        {
            //this works out cubics in 1 row or 1 column, the x values represent the index in either the x or y direction,
            //the y values represent the colour intensity at the pixels and gradients are just the colour gradient across the points
            //equation is ax^3 + dx^2 + cx + b.
            //above differentiated becomes: 3ax^2 + 2dx + c.
            //cannot put either into row-echelon form so I have
            //switched the places of b and d which should eliminate this issue

            double[,] coMat =
            {
                {Math.Pow(x1, 3),        1,    x1,    Math.Pow(x1, 2) },
                {Math.Pow(x2, 3),        1,    x2,    Math.Pow(x2, 2)},
                {Math.Pow(x1, 2) * 3,    0,    1,     x1 * 2},
                {Math.Pow(x2, 2) * 3,    0,    1,     x2 * 2},
            };
            double[] rhs = { y1, y2, grad1, grad2 };

            //results in form { a, d, c, b }
            double[] res = Eliminator(coMat, rhs);
            // cubic is res[0]x^3 + res[3]x^2 + res[2]x + res[1]
            return res;
        }

        private static double[] Eliminator(double[,] CoMat, double[] rhs)
        {
            //store the length - 1 to reduce computation
            int n = rhs.Length - 1;

            //make a results array with the same length as the right hand side of the equation
            //this will store the values of the unknowns in order
            double[] res = new double[n + 1];

            //reduce all equations to be in row-echelon form
            for (int i = 0; i <= n; i++)
            {
                for (int j = n; j > i; j--)
                {
                    double factor = CoMat[j, i] / CoMat[i, i];

                    for (int k = 0; k <= n; k++)
                    {
                        CoMat[j, k] -= CoMat[i, k] * factor;
                    }
                    rhs[j] -= rhs[i] * factor;
                }
            }

            //we can immediately find the value of the last result
            res[n] = rhs[n] / CoMat[n, n];
            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = i + 1; j <= n; j++)
                {
                    // i didnt scale the rows down to 1s so this handles that by multiplying by the coefficients
                    rhs[i] -= res[j] * CoMat[i, j];
                }
                res[i] = rhs[i] / CoMat[i, i];
            }
            return res;
        }

        public static double[] RREFEliminator(double[,] CoMat, double[] rhs)
        {
            //store the length - 1 to reduce computation
            int n = rhs.Length - 1;

            //make a results array with the same length as the right hand side of the equation
            //this will store the values of the unknowns in order
            double[] res = new double[n + 1];

            /*//reduce all equations to be in row-echelon form
            for (int i = 0; i <= n; i++)
            {
                for (int j = n; j > i; j--)
                {
                    double factor = CoMat[j, i] / CoMat[i, i];

                    for (int k = 0; k <= n; k++)
                    {
                        CoMat[j, k] -= CoMat[i, k] * factor;
                    }
                    rhs[j] -= rhs[i] * factor;
                }
            }*/

            //v2
            for (int i = 0; i <= n; i++)
            {
                int row = i;
                bool validRow = false;
                int col = 0;
                for (int j = 0; j < CoMat.GetLength(1); j++)
                {
                    if (CoMat[row, j] != 0)
                    {
                        if (!validRow)
                        {
                            col = j;
                        }
                        validRow = true;
                    }
                }
                if (validRow && row != n)
                {
                    for (int j = i + 1; j <= n; j++)
                    {
                        double coef = CoMat[j, col] / CoMat[row, col];
                        for (int k = 0; k < CoMat.GetLength(1); k++)
                        {
                            CoMat[j, k] -= coef * CoMat[row, k];
                        }
                        rhs[j] -= coef * rhs[row];
                    }
                }
            }

            //make row arranger with spearmann rank type things
            int[] ranks = new int[CoMat.GetLength(0)];
            for (int i = 0; i <= n; i++)
            {
                bool validRow = false;
                int col = 0;
                for (int j = 0; j < CoMat.GetLength(1); j++)
                {
                    if (CoMat[i, j] != 0)
                    {
                        if (!validRow)
                        {
                            col = j;
                            ranks[i] = col;
                        }
                        validRow = true;
                    }
                }
            }

            double[,] temp = new double[CoMat.GetLength(0), CoMat.GetLength(0)];
            double[] tempRhs = new double[rhs.Length];


            for (int i = 0; i < temp.GetLength(0); i++)
            {
                for (int j = 0; j < temp.GetLength(1); j++)
                {
                    temp[ranks[i], j] = CoMat[i, j];
                }
                tempRhs[ranks[i]] = rhs[i];
            }

            CoMat = temp;
            rhs = tempRhs;

            //rref
            for (int i = 0; i <= n; i++)
            {
                bool validRow = false;
                int col = 0;
                for (int j = 0; j < CoMat.GetLength(1); j++)
                {
                    if (CoMat[i, j] != 0)
                    {
                        if (!validRow)
                        {
                            col = j;
                        }
                        validRow = true;
                    }

                }
                double pivot = CoMat[i, col];
                for (int j = col; j < CoMat.GetLength(1); j++)
                {
                    CoMat[i, j] /= pivot;
                }
                rhs[i] /= pivot;
            }
            for (int i = n; i >= 0; i--)
            {
                int row = i;
                bool validRow = false;
                int col = 0;
                for (int j = 0; j < CoMat.GetLength(1); j++)
                {
                    if (CoMat[row, j] != 0)
                    {
                        if (!validRow)
                        {
                            col = j;
                        }
                        validRow = true;
                    }
                }
                if (validRow)
                {

                    for (int j = 0; j < row; j++)
                    {
                        double coef = CoMat[j, col] / CoMat[row, col];
                        for (int k = 0; k < CoMat.GetLength(1); k++)
                        {
                            CoMat[j, k] -= coef * CoMat[row, k];
                        }
                        rhs[j] -= coef * rhs[row];
                    }
                }
            }

            //solves
            for (int i = n; i >= 0; i--)
            {
                bool found = false;
                for (int j = 0; j < CoMat.GetLength(1); j++)
                {
                    if (CoMat[i, j] != 0 && !found)
                    {
                        res[i] = rhs[i] / CoMat[i, j];
                        found = true;
                    }
                }
            }
            /*//we can immediately find the value of the last result
            res[n] = rhs[n] / CoMat[n, n];
            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = i + 1; j <= n; j++)
                {
                    // i didnt scale the rows down to 1s so this handles that by multiplying by the coefficients
                    rhs[i] -= res[j] * CoMat[i, j];
                }
                res[i] = rhs[i] / CoMat[i, i];
            }*/
            return res;
        }

        public Bitmap BicubicScaleUpDecimal(Bitmap imageIn, PictureBox pictureBox, RichTextBox richTextBox1)
        {
            //im only going to handle integer scale factors as i want my pixels to have 1:1 mappings with equal pixels in between where a 3sf transformation will have 2 pixels between each original pixel
            int sf = pictureBox.Width / imageIn.Width;
            if (sf > 3)
            {
                Console.WriteLine("image too small to preserve quality");
                richTextBox1.Text = "current image too small to preserve quality";
                return imageIn;
            }
            else if (sf > 1)
            {
                richTextBox1.Text = "";
                int newX = imageIn.Width * sf;
                int newY = imageIn.Height * sf;

                Bitmap newImg = new Bitmap(newX, newY);

                decimal[,] colourGradX = new decimal[imageIn.Width, imageIn.Height];
                decimal[,] colourGradY = new decimal[imageIn.Width, imageIn.Height];
                GetGradsDecimal(imageIn, ref colourGradX, ref colourGradY);

                //I will work out the colour of the points in the x direction and y direction first and
                //then take the average of each pixel in the x and y direction to incorporate both directions
                decimal[,] predictedX = new decimal[newX, newY];
                decimal[,] predictedY = new decimal[newX, newY];

                decimal[] coefX = new decimal[4];
                decimal[] coefY = new decimal[4];

                int LastVal = 0;
                bool isCubic = true;
                //predicts the values of the pixels on the bottom row
                for (int k = 0; k < sf; k++)
                {
                    for (int i = 0; i < newX; i++)
                    {
                        if (i % sf == 0 && (newY - 1) % sf == 0)
                        {
                            if (colourGradX[i, imageIn.Height - (k) - 1] == 0 && colourGradX[i + 1, imageIn.Height - (k) - 1] == 0)
                            {
                                isCubic = false;
                                LastVal = imageIn.GetPixel(i / sf, imageIn.Height - (k) - 1).R;
                            }
                            else
                            {
                                isCubic = true;
                                coefX = GetCubicBetweenPointsDecimal(i, imageIn.GetPixel(i / sf, imageIn.Height - (k) - 1).R, i + 1, imageIn.GetPixel((i / sf) + 1, imageIn.Height - (k) - 1).R, colourGradX[i, imageIn.Height - (k) - 1], colourGradX[i + 1, imageIn.Height - (k) - 1]);
                            }
                        }
                        if (isCubic)
                        {
                            predictedX[i, newY - 1 - k] = coefX[0] * (decimal)Math.Pow(i, 3) + coefX[3] * (decimal)Math.Pow(i, 2) + coefX[2] * (i) + coefX[1];
                        }
                        else
                        {
                            predictedX[i, newY - 1 - k] = LastVal;
                        }
                    }
                }

                for (int k = 0; k < sf; k++)
                {
                    LastVal = 0;
                    isCubic = true;
                    //predicts pixel values of the rightmost column
                    for (int j = 0; j < newY; j++)
                    {
                        if ((newX - 1) % sf == 0 && j % sf == 0)
                        {
                            if (colourGradX[imageIn.Width - (k) - 1, j / sf] == 0 && colourGradX[imageIn.Width - (k) - 1, j / sf + 1] == 0)
                            {
                                LastVal = imageIn.GetPixel(imageIn.Width - (k) - 1 / sf, j / sf).R;
                                isCubic = false;
                            }
                            else
                            {
                                isCubic = true;
                                coefY = GetCubicBetweenPointsDecimal(j, imageIn.GetPixel((imageIn.Width - 1 - k) / sf, j / sf).R, j + 1, imageIn.GetPixel((imageIn.Width - 1 - (k)) / sf, (j / sf) + 1).R, colourGradY[imageIn.Width - (k) - 1, j / sf], colourGradY[imageIn.Width - (k) - 1, (j / sf) + 1]);
                            }
                        }
                        if (isCubic)
                        {
                            predictedY[newX - 1 - k, j] = coefY[0] * (decimal)Math.Pow(j, 3) + coefY[3] * (decimal)Math.Pow(j, 2) + coefY[2] * (j) + coefY[1];
                        }
                        else
                        {
                            predictedY[newX - 1 - k, j] = LastVal;
                        }
                    }
                }


                bool isCubicY = true;
                double lastY = 0;
                //preicts rest of pixel values in between
                for (int i = 0; i < newX - sf; i++)
                {
                    for (int j = 0; j < newY - sf; j++)
                    {

                        if (j % sf == 0)
                        {
                            if (colourGradY[i / sf, j / sf] == 0 && colourGradY[i / sf, (j / sf) + 1] == 0)
                            {
                                isCubicY = false;
                                lastY = imageIn.GetPixel(i / sf, j / sf).R;
                            }
                            else
                            {
                                isCubicY = true;
                                coefY = GetCubicBetweenPointsDecimal(j / sf, imageIn.GetPixel(i / sf, j / sf).R, (j / sf) + 1, imageIn.GetPixel(i / sf, j / sf + 1).R, colourGradX[i / sf, j / sf], colourGradX[i / sf, j / sf + 1]);
                            }
                        }

                        if (isCubicY)
                        {
                            predictedY[i, j] = coefY[0] * (decimal)Math.Pow(j / sf + j % sf, 3) + coefY[3] * (decimal)Math.Pow(j / sf + j % sf, 2) + coefY[2] * (j / sf + j % sf) + coefY[1];
                        }
                        else
                        {
                            predictedY[i, j] = (decimal)lastY;
                        }
                    }

                }

                bool isCubicX = true;
                double lastX = 0;
                for (int j = 0; j < newY - sf; j++)
                {
                    for (int i = 0; i < newX - sf; i++)
                    {
                        if (i % sf == 0)
                        {
                            if (colourGradX[i / sf, j / sf] == 0 && colourGradX[(i / sf) + 1, j / sf] == 0)
                            {
                                isCubicX = false;
                                lastX = imageIn.GetPixel(i / sf, j / sf).R;
                            }
                            else
                            {
                                isCubicX = true;
                                coefX = GetCubicBetweenPointsDecimal(i / sf, imageIn.GetPixel(i / sf, j / sf).R, (i / sf) + 1, imageIn.GetPixel(i / sf + 1, j / sf).R, colourGradX[i / sf, j / sf], colourGradX[i / sf + 1, j / sf]);

                            }
                            if (isCubicX)
                            {
                                predictedX[i, j] = coefX[0] * (decimal)Math.Pow(i / sf + i % sf, 3) + coefX[3] * (decimal)Math.Pow(i / sf + i % sf, 2) + coefX[2] * (i / sf + i % sf) + coefX[1];
                            }
                            else
                            {
                                predictedX[i, j] = (decimal)lastX;
                            }
                        }
                    }
                }

                //averages the predicted y and predicted x values and puts it into the new image
                for (int i = 0; i < predictedX.GetLength(0); i++)
                {
                    for (int j = 0; j < predictedX.GetLength(1); j++)
                    {
                        //conditionals for insurance but the
                        //program functions well without
                        if (predictedY[i, j] > 255)
                        {
                            predictedY[i, j] = 255;
                        }
                        else if (predictedY[i, j] < 0)
                        {
                            predictedY[i, j] = 0;
                        }
                        if (predictedX[i, j] > 255)
                        {
                            predictedX[i, j] = 255;
                        }
                        else if (predictedX[i, j] < 0)
                        {
                            predictedX[i, j] = 0;
                        }
                        //certain pixels on the edge of the picture come up
                        //as NaNs but this shouldnt effect the end result
                        if (Double.IsNaN((double)predictedX[i, j]))
                        {
                            predictedX[i, j] = 0M;
                        }
                        if (Double.IsNaN((double)predictedY[i, j]))
                        {
                            predictedY[i, j] = 0M;
                        }

                        //sets pixel in new image
                        newImg.SetPixel(i, j, Color.FromArgb(Convert.ToInt32((predictedX[i, j] + predictedY[i, j]) / 2), Convert.ToInt32((predictedX[i, j] + predictedY[i, j]) / 2), Convert.ToInt32((predictedX[i, j] + predictedY[i, j]) / 2)));
                    }
                }

                return newImg;
            }
            return imageIn;
        }

        public void GetGradsDecimal(Bitmap imageIn, ref decimal[,] colourGradX, ref decimal[,] colourGradY)
        {
            for (int i = 0; i < imageIn.Height; i++)
            {
                colourGradX[0, i] = (imageIn.GetPixel(1, i).R - imageIn.GetPixel(0, i).R);
                colourGradX[imageIn.Width - 1, i] = (imageIn.GetPixel(imageIn.Width - 1, i).R - imageIn.GetPixel(imageIn.Width - 2, i).R);
            }
            for (int i = 0; i < imageIn.Width; i++)
            {
                colourGradY[i, 0] = (imageIn.GetPixel(i, 1).R - imageIn.GetPixel(i, 0).R);
                colourGradY[i, imageIn.Height - 1] = (imageIn.GetPixel(i, imageIn.Height - 1).R - imageIn.GetPixel(i, imageIn.Height - 2).R);
            }



            for (int i = 1; i < imageIn.Width - 1; i++)
            {
                for (int j = 0; j < imageIn.Height; j++)
                {
                    colourGradX[i, j] = (imageIn.GetPixel(i + 1, j).R - imageIn.GetPixel(i - 1, j).R) / 2;
                }
            }
            for (int i = 0; i < imageIn.Width; i++)
            {
                for (int j = 1; j < imageIn.Height - 1; j++)
                {
                    colourGradY[i, j] = (imageIn.GetPixel(i, j + 1).R - imageIn.GetPixel(i, j - 1).R) / 2;
                }
            }
        }

        public decimal[] GetCubicBetweenPointsDecimal(int x1, int y1, int x2, int y2, decimal grad1, decimal grad2)
        {
            //this works out cubics in 1 row or 1 column, the x values represent the index in either the x or y direction,
            //the y values represent the colour intensity at the pixels and gradients are just the colour gradient across the points
            //equation is ax^3 + dx^2 + cx + b.
            //above differentiated becomes: 3ax^2 + 2dx + c.
            //cannot put either into row-echelon form so I have
            //switched the places of b and d which should eliminate this issue

            decimal[,] coMat =
            {
                {(decimal)Math.Pow(x1, 3), 1M, (decimal)x1, (decimal)Math.Pow(x1, 2) },
                {(decimal)Math.Pow(x2, 3), 1M, (decimal)x2, (decimal)Math.Pow(x2, 2)},
                {(decimal)Math.Pow(x1, 2) * 3, 0M, 1M, (decimal)x1 * 2},
                {(decimal)Math.Pow(x2, 2) * 3, 0M, 1M, (decimal)x2 * 2},
            };
            decimal[] rhs = { (decimal)y1, (decimal)y2, grad1, grad2 };

            //results in form { a, d, c, b }
            decimal[] res = EliminatorDecimal(coMat, rhs);
            // cubic is res[0]x^3 + res[3]x^2 + res[2]x + res[1]
            return res;
        }

        /*private static decimal[] EliminatorV2Decimal(decimal[,] CoMat, decimal[] rhs)
        {
            //make a results array with the same length as the right hand side of the equation
            //this will store the values of the unknowns in order
            decimal[] res = new decimal[rhs.Length];
            List<int> prevIdx = new List<int>();
            bool[] nullRow = new bool[rhs.Length];


            for (int i = 0; i < CoMat.GetLength(1); i++)
            {
                int pivotIdx = 0;
                bool found = false;
                while(!found && pivotIdx < CoMat.GetLength(0))
                {
                    if (CoMat[i, pivotIdx] != 0 && !prevIdx.Contains(pivotIdx))
                    {
                        found = true;
                        prevIdx.Add(pivotIdx);
                    }
                    if (!found)
                    {
                        pivotIdx++;
                    }
                }
                if (!found)
                {
                    nullRow = true;
                }

            }
        }*/

        private static decimal[] EliminatorDecimal(decimal[,] CoMat, decimal[] rhs)
        {
            //store the length - 1 to reduce computation
            int n = rhs.Length - 1;

            //make a results array with the same length as the right hand side of the equation
            //this will store the values of the unknowns in order
            decimal[] res = new decimal[n + 1];
            bool[] nullRows = new bool[n+1];
            for (int i = 0; i <= n; i++)
            {
                res[i] = 0;
                nullRows[i] = false;
            }


            //reduce all equations to be in row-echelon form
            for (int i = 0; i <= n; i++)
            {
                bool nullRow = false;
                if (CoMat[i, i] == 0 && i < n)
                {
                    nullRow = true;
                    for (int j = i + 1; j <= n; j++)
                    {
                        if (CoMat[j, i] != 0)
                        {
                            CoMat = SwapRows(i, j, CoMat, ref res);
                            nullRow = false;
                        }
                    }
                    if (nullRow)
                    {
                        CoMat = SwapRows(i, n, CoMat, ref res);
                    }
                }
                else if (CoMat[i,i] == 0)
                {
                    nullRow = true;
                }
                if (!nullRow)
                {
                    for (int j = n; j > i; j--)
                    {
                        decimal factor = decimal.Divide(CoMat[j, i], CoMat[i, i]);

                        for (int k = 0; k <= n; k++)
                        {
                            CoMat[j, k] = decimal.Subtract(CoMat[i, k], decimal.Multiply(CoMat[i, k], factor));
                        }
                        rhs[j] = decimal.Subtract(rhs[j], decimal.Multiply(rhs[i], factor));
                    }
                }
                nullRows[i] = nullRow;
            }

            //we can immediately find the value of the last result
            res[n] = decimal.Divide(rhs[n], CoMat[n, n]);
            for (int i = n - 1; i >= 0; i--)
            {
                if (!nullRows[i])
                {
                    for (int j = i + 1; j <= n; j++)
                    {
                        // i didnt scale the rows down to 1s so this handles that by multiplying by the coefficients
                        rhs[i] -= res[j] * CoMat[i, j];
                    }
                    res[i] = rhs[i] / CoMat[i, i];
                }
            }
            return res;
        }

        private static decimal[,] SwapRows(int i, int j, decimal[,] CoMat, ref decimal[] res)
        {
            for(int k = 0; k < CoMat.GetLength(0); k++)
            {
                decimal temp = CoMat[i, k];
                CoMat[i, k] = CoMat[j, k];
                CoMat[j, k] = CoMat[i, k];
                temp = res[i];
                res[i] = res[j];
                res[j] = temp;
            }

            return CoMat;
        }

        public Bitmap ScaleDown(Bitmap imageIn, PictureBox pictureBox)
        {
            int sf = (int)Math.Ceiling((decimal)(imageIn.Width / pictureBox.Width));
            Bitmap imageOut = new Bitmap((imageIn.Width / sf) + 1, (imageIn.Height / sf) + 1);
            int counter = sf;
            for (int i = 0; i < imageIn.Width; i++)
            {
                if (i % sf == 0)
                {
                    for (int j = 0; j < imageIn.Height; j++)
                    {
                        if (j % sf == 0)
                        {
                            imageOut.SetPixel(i / sf, j / sf, imageIn.GetPixel(i, j));
                        }
                    }
                }
            }
            return imageOut;
        }

        public Bitmap CropHorz(Bitmap imageIn, PictureBox pictureBox)
        {
            int diff = imageIn.Width - pictureBox.Width;
            int cutleft = 0;
            int cutright = 0;

            Bitmap imageOut = new Bitmap(pictureBox.Width, imageIn.Height);

            if (diff % 2 == 0)
            {
                cutleft = diff / 2;
                cutright = diff - cutleft;
            }
            else
            {
                cutleft = (int)(diff / 2);
                cutright = diff - cutleft;
            }

            for (int i = cutleft; i < pictureBox.Width; i++)
            {
                for (int j = 0; j < imageIn.Height; j++)
                {
                    imageOut.SetPixel(i, j, imageIn.GetPixel(i, j));
                }
            }

            return imageOut;
        }

        public Bitmap CropVert(Bitmap imageIn, PictureBox pictureBox)
        {
            int diff = imageIn.Height - pictureBox.Height;
            int cutUp = 0;
            int cutDown = 0;

            Bitmap imageOut = new Bitmap(imageIn.Width, pictureBox.Height);

            if (diff % 2 == 0)
            {
                cutUp = diff / 2;
                cutDown = diff - cutUp;
            }
            else
            {
                cutUp = (int)(diff / 2);
                cutDown = diff - cutUp;
            }

            for (int i = 0; i < imageIn.Width; i++)
            {
                for (int j = cutUp; j < pictureBox.Height; j++)
                {
                    imageOut.SetPixel(i, j, imageIn.GetPixel(i, j));
                }
            }

            return imageOut;
        }

        public void convertToIntImg(PictureBox pictureBox, ref RichTextBox richTextBox1, ref double[][,] integralImages, int idx)
        {
            Bitmap ImageIn = (Bitmap)(pictureBox.Image);

            double[,] pixels = new double[ImageIn.Width + 1, ImageIn.Height + 1];
            Bitmap imageOut = new Bitmap(ImageIn.Width, ImageIn.Height);

            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                pixels[i, 0] = 0;
            }
            for (int i = 0; i < pixels.GetLength(1); i++)
            {
                pixels[0, i] = 0;
            }
            for (int i = 0; i < pictureBox.Image.Width; i++)
            {
                for (int j = 0; j < pictureBox.Image.Height; j++)
                {
                    pixels[i + 1, j + 1] = (ImageIn.GetPixel(i, j).R + ImageIn.GetPixel(i, j).G + ImageIn.GetPixel(i, j).B) / 3;
                }
            }
            double[,] intImg = new double[ImageIn.Width, ImageIn.Height];

            for (int i = 0; i < imageOut.Width; i++)
            {
                for (int j = 0; j < imageOut.Height; j++)
                {
                    double colour = pixels[i + 1, j + 1] + pixels[i, j + 1] + pixels[i + 1, j] - pixels[i, j];
                    pixels[i + 1, j + 1] = colour;
                    intImg[i, j] = colour;
                }
            }
            integralImages[idx] = intImg;
            richTextBox1.Text = "conversion complete";
        }

        public void convertToIntImg(Image Image, ref double[][,] integralImages, int idx)
        {
            Bitmap ImageIn = (Bitmap)Image;

            double[,] pixels = new double[ImageIn.Width + 1, ImageIn.Height + 1];
            Bitmap imageOut = new Bitmap(ImageIn.Width, ImageIn.Height);

            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                pixels[i, 0] = 0;
            }
            for (int i = 0; i < pixels.GetLength(1); i++)
            {
                pixels[0, i] = 0;
            }
            for (int i = 0; i < Image.Width; i++)
            {
                for (int j = 0; j < Image.Height; j++)
                {
                    pixels[i + 1, j + 1] = (ImageIn.GetPixel(i, j).R + ImageIn.GetPixel(i, j).G + ImageIn.GetPixel(i, j).B) / 3;
                }
            }
            double[,] intImg = new double[ImageIn.Width, ImageIn.Height];

            for (int i = 0; i < imageOut.Width; i++)
            {
                for (int j = 0; j < imageOut.Height; j++)
                {
                    double colour = pixels[i + 1, j + 1] + pixels[i, j + 1] + pixels[i + 1, j] - pixels[i, j];
                    pixels[i + 1, j + 1] = colour;
                    intImg[i, j] = colour;
                }
            }
            integralImages[idx] = intImg;
        }

        public int BinarySearch(decimal[] nums, decimal target)
        {
            return BinarySearcher(nums, target, nums.Length - 1, 0);
        }

        private int BinarySearcher(decimal[] nums, decimal target, int UP, int LP)
        {
            if (UP >= LP)
            {
                int mid = (int)(UP + LP) / 2;
                if (nums[mid] == target)
                {
                    return mid;
                }
                else if (nums[mid] > target)
                {
                    UP = mid - 1;
                }
                else
                {
                    LP = mid + 1;
                }
                return BinarySearcher(nums, target, UP, LP);
            }
            else
            {
                return UP;
            }
        }
        public int BinarySearch(double[] nums, double target)
        {
            return BinarySearcher(nums, target, nums.Length - 1, 0);
        }
        private int BinarySearcher(double[] nums, double target, int UP, int LP)
        {
            if (UP >= LP)
            {
                int mid = (int)(UP + LP) / 2;
                if (nums[mid] == target)
                {
                    return mid;
                }
                else if (nums[mid] > target)
                {
                    UP = mid - 1;
                }
                else
                {
                    LP = mid + 1;
                }
                return BinarySearcher(nums, target, UP, LP);
            }
            else
            {
                return UP;
            }
        }

    }
}