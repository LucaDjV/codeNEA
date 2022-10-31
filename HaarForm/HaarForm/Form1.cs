using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using HaarCascadeLib;


namespace HaarForm
{
    //priority queue
    //if is interrupt, insert at front, else insert at back
    public class taskQueue
    {
        public static queueElement[] tQueue = new queueElement[100];
        public static int size, FP, BP;


        public taskQueue()
        {
            size = 0;
            FP = 0;
            BP = 0;
        }

        public void Enqueue(Task taskIn, bool interrupt)
        {
            if (interrupt)
            {
                if (size <= 100)
                {
                    if(FP == 0)
                    {
                        tQueue[99] = new queueElement(taskIn);
                        FP = 99;
                        size++;
                    }
                    else if (FP > BP)
                    {
                        FP--;
                        tQueue[FP] = new queueElement(taskIn);
                        size++;
                    }
                    else
                    {
                        FP++;
                        tQueue[FP] = new queueElement(taskIn);
                        size++;
                    }
                }
                else
                {
                    throw new Exception("No more space in queue for task");
                }
            }
            else
            {
                if(size <= 100)
                {
                    if(BP == 99)
                    {
                        BP = 0;
                        tQueue[BP] = new queueElement(taskIn);
                        size++;
                    }
                    else
                    {
                        BP++;
                        tQueue[BP] = new queueElement(taskIn);
                        size++;
                    }
                }
                else
                {
                    throw new Exception("No more space in queue for task");
                }
            }
        }

        public static async void clearQueue()
        {
            int amtDequeued = 0;
            Task[] inProgress = new Task[4];
            while (size > 0)
            {
                if (FP == 99)
                {
                    inProgress[amtDequeued] = tQueue[FP].element;
                    inProgress[amtDequeued].Start();
                    amtDequeued++;
                    FP = 0;
                    size--;
                }
                else
                {
                    inProgress[amtDequeued] = tQueue[FP].element;
                    inProgress[amtDequeued].Start();
                    amtDequeued++;
                    FP++;
                    size--;
                }
                if(amtDequeued == 4)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        await inProgress[i];
                    }
                }
            }
        }

        public class queueElement
        {
            public Task element
            {
                get { return element; }
                set
                {
                    element = value;
                    if (taskQueue.size == 0)
                    {
                        Task t = new Task(() => taskQueue.clearQueue());
                        t.Start();
                    }
            }
            }

            public queueElement(Task t)
            {
                element = t;
            }
        }

    }

    
    public partial class Form1 : Form
    {
        public Image[] Images;
        public Image[] testImages;
        double[][,] testIntImgs;
        double[][,] integralImages;
        int idx = -1;
        public static imageProcessorClass methods;
        bool[] isFace;
        public static HaarCascadeV2 haarCascade;
        public static int newest;
        public static bool trained;


        public static taskQueue tasks = new taskQueue();

        /// <summary>
        /// Something wrong with gini coefficients- check finishedFeatures[0]
        /// </summary>

        public Form1()
        {
            InitializeComponent();
            methods = new imageProcessorClass();
            List<Bitmap> images = new List<Bitmap>();
            List<bool> faces = new List<bool>();
            newest = new int();
            trained = false;
            using(StreamReader reader = new StreamReader("saves\\state.txt"))
            {
                string line = reader.ReadLine();
                if(line == null || line == "")
                {
                    newest = 2;
                }
                else
                {
                    newest = int.Parse(line);
                }
            }
            foreach (string path in Directory.EnumerateFiles("C:\\Users\\lucaD\\OneDrive\\Pictures\\data\\TrainingImages"))
            {
                Bitmap image = new Bitmap(path);
                images.Add(image);
                faces.Add(true);
            }
            
            foreach (string path in Directory.EnumerateFiles("C:\\Users\\lucaD\\OneDrive\\Pictures\\data\\TrainingImagesNon-Face"))
            {
                Bitmap image = new Bitmap(path);
                images.Add(image);
                faces.Add(false);
            }
            
            isFace = faces.ToArray();
            Console.WriteLine("isFaces Length = " + isFace.Length);
            Images = images.ToArray();
            Console.WriteLine("images Length = " + Images.Length);
            integralImages = new double[Images.Length][,];
            List<Image> ImagesTemp = new List<Image>();
            foreach (string path in Directory.EnumerateFiles("C:\\Users\\lucaD\\OneDrive\\Pictures\\data\\TestImage"))
            {
                Bitmap image = new Bitmap(path);
                ImagesTemp.Add(image);
            }
            testImages = ImagesTemp.ToArray();

            testIntImgs = new double[testImages.Length][,];
            for (int i = 0; i < testImages.Length; i++)
            {
                methods.convertToIntImg(testImages[i], ref testIntImgs, i);
            }
            haarCascade = new HaarCascadeV2();
            Console.WriteLine("testIntImgs length = " + testIntImgs.Length);
        }

        private void startProcess_Click_1(object sender, EventArgs e)
        {
            if (loadSave.Checked)
            {
                bool success = false;
                if (saveFile.Text == "1")
                {
                    success = LoadSaveFile(1);
                    success = true;
                }
                else if (saveFile.Text == "2")
                {
                    success = LoadSaveFile(2);
                    success = true;
                }
                else
                {
                    success = LoadSaveFile(true);
                    success = true;
                }

                if (success)
                {
                    richTextBox1.Text = "successfully loaded save" + saveFile.Text;
                    saveFile.Text = "";
                    saveFile.Visible = false;
                    trained = true;
                }
                else
                {
                    if(saveFile.Text == "1" || saveFile.Text == "2")
                    {
                        richTextBox1.Text = "save file " + saveFile.Text + " not valid";
                    }
                    else
                    {
                        richTextBox1.Text = "no valid save files";
                    }
                    loadSave.Checked = false;
                    saveFile.Text = "";
                    saveFile.Visible = false;
                }
            }
            else
            {
                //convert all images to integral images
                for (int i = 0; i < Images.Length; i++)
                {
                    methods.convertToIntImg(Images[i], ref integralImages, i);
                }

                //instantiate cascade
                HaarCascadeV2 cascade = new HaarCascadeV2(isFace, methods);
                haarCascade = cascade;

                //another function to make it easy to add async later
                Task train = new Task(() => startTraining());
                tasks.Enqueue(train, false);

                SaveState();

                trained = true;
                Console.WriteLine("training finished");
                Console.WriteLine("start testing?");
                string choice = Console.ReadLine();
                if (choice == "yes")
                {
                    bool[] classification = testCase();

                    for (int i = 0; i < classification.Length; i++)
                    {
                        if (classification[i])
                        {
                            Console.WriteLine("image " + i + 1 + " is a face");
                        }
                        else
                        {
                            Console.WriteLine("image " + i + 1 + " is not a face");
                        }
                    }
                }
            }
            
        }

        private bool[] testCase()
        {
            bool[] classifications = new bool[testIntImgs.Length];

            classifications = haarCascade.Test(testIntImgs);
            return classifications;
        }

        private bool startTraining()
        {
            //train model
            haarCascade.classifyInWindowTrain(integralImages, isFace, methods);
            return true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Loader_Click_1(object sender, EventArgs e)
        {
            Task t = new Task(() => loading());
            tasks.Enqueue(t, true);
        }

        private void loading()
        {
            if (idx < Images.Length - 1)
            {
                idx++;
                Bitmap newImg = ProcessImg((Bitmap)(Images[idx]));
                pictureBox.Image = newImg;
                pictureBox.Show();
                richTextBox1.Text = "image: " + idx + " is showing";
            }
            if (trained && pictureBox.Image.Width >= 46)
            {
                double[][,] intImg = new double[1][,];

                methods.convertToIntImg(pictureBox.Image, ref intImg, 0);

                if (intImg.GetLength(0) == 46 && intImg.GetLength(1) == 46)
                {
                    HaarCascadeV2.TestWindow window = new HaarCascadeV2.TestWindow(intImg[0]);
                    if (window.classifyWindow())
                    {
                        richTextBox1.Text = "image " + idx + " contains a face";
                    }
                    else
                    {
                        richTextBox1.Text = "image " + idx + " does not contain a face";
                    }
                }
                else
                {
                    List<HaarCascadeV2.TestWindow> windowList = new List<HaarCascadeV2.TestWindow>();
                    List<bool> classifications = new List<bool>();

                    for (int i = 0; i < intImg.GetLength(0) - 46; i++)
                    {
                        for (int j = 0; j < intImg.GetLength(1) - 46; j++)
                        {
                            double[,] currwindow = new double[46, 46];
                            for (int k = 0; k < 46; k++)
                            {
                                for (int l = 0; l < 46; l++)
                                {
                                    currwindow[k, l] = intImg[0][k, l];
                                    HaarCascadeV2.TestWindow window = new HaarCascadeV2.TestWindow(intImg[0]);
                                    windowList.Add(window);
                                    classifications.Add(window.classifyWindow());
                                }
                            }
                        }
                    }


                    if (classifications.Contains<bool>(true))
                    {
                        richTextBox1.Text = "image " + idx + " contains a face";
                    }
                    else
                    {
                        richTextBox1.Text = "image " + idx + " does not contain a face";
                    }
                }
            }
        }

        private void Back_Click_1(object sender, EventArgs e)
        {
            if (idx > 0)
            {
                idx--;
                Bitmap newImg = ProcessImg((Bitmap)(Images[idx]));
                pictureBox.Image = newImg;
                pictureBox.Show();
            }
            if (trained && pictureBox.Image.Width >= 46)
            {
                double[][,] intImg = new double[1][,];

                methods.convertToIntImg(pictureBox.Image, ref intImg, 0);

                if (intImg.GetLength(0) == 46 && intImg.GetLength(1) == 46)
                {
                    HaarCascadeV2.TestWindow window = new HaarCascadeV2.TestWindow(intImg[0]);
                    if (window.classifyWindow())
                    {
                        richTextBox1.Text = "image " + idx + " contains a face";
                    }
                    else
                    {
                        richTextBox1.Text = "image " + idx + " does not contain a face";
                    }
                }
                else
                {
                    List<HaarCascadeV2.TestWindow> windowList = new List<HaarCascadeV2.TestWindow>();
                    List<bool> classifications = new List<bool>();

                    for (int i = 0; i < intImg.GetLength(0) - 46; i++)
                    {
                        for (int j = 0; j < intImg.GetLength(1) - 46; j++)
                        {
                            double[,] currwindow = new double[46, 46];
                            for (int k = 0; k < 46; k++)
                            {
                                for (int l = 0; l < 46; l++)
                                {
                                    currwindow[k, l] = intImg[0][k, l];
                                    HaarCascadeV2.TestWindow window = new HaarCascadeV2.TestWindow(intImg[0]);
                                    windowList.Add(window);
                                    classifications.Add(window.classifyWindow());
                                }
                            }
                        }
                    }


                    if (classifications.Contains<bool>(true))
                    {
                        richTextBox1.Text = "image " + idx + " contains a face";
                    }
                    else
                    {
                        richTextBox1.Text = "image " + idx + " does not contain a face";
                    }
                }
            }
        }

        public static void SaveState()
        {
            string path = "saves\\save";
            if(newest == 1)
            {
                path += "2.txt";
                newest = 2;

                File.WriteAllText("saves\\state.txt", "2");

                string[] temp = File.ReadAllLines(path);
                using(StreamWriter writer = new StreamWriter("saves\\save1.txt", false))
                {
                    for(int i = 0; i < temp.Length; i++)
                    {
                        writer.WriteLine(temp[i]);
                    }
                }
            }
            else
            {
                path += "1.txt";
                newest = 1;

                File.WriteAllText("saves\\state.txt", "1");

                string[] temp = File.ReadAllLines(path);
                using (StreamWriter writer = new StreamWriter("saves\\save2.txt", false))
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        writer.WriteLine(temp[i]);
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine("1");
                for(int i = 0; i < HaarCascadeV2.finishedFeatures.Length; i++)
                {
                    writer.WriteLine(HaarCascadeV2.finishedFeatures[i].threshold);
                    writer.WriteLine(HaarCascadeV2.finishedFeatures[i].vote);

                }
            }
        }

        public static bool LoadSaveFile(int save)
        {
            bool success = true;
            string path = "";
            bool valid = false;
            if (save == 1)
            {
                path = "saves\\save1.txt";
                valid = true;
            }
            else if (save == 2)
            {
                path = "saves\\save2.txt";
                valid = true;
            }
            else
            {
                Console.WriteLine("save" + save + " is not a valid save file");
                success = false;
            }

            if (valid)
            {
                HaarCascadeV2 cascade = new HaarCascadeV2(methods);
                haarCascade = cascade;
                using (StreamReader reader = new StreamReader(path))
                {
                    string head = "";
                    head += reader.ReadLine();
                    if (head == "1")
                    {
                        for (int i = 0; i < HaarCascadeV2.finishedFeatures.Length; i++)
                        {
                            string curr = "";
                            curr += reader.ReadLine();
                            double threshold = double.Parse(curr);
                            HaarCascadeV2.UpdateThreshold(threshold, i);
                            curr = "";
                            curr += reader.ReadLine();
                            decimal vote = decimal.Parse(curr);
                            HaarCascadeV2.UpdateVote(vote, i);
                        }
                    }
                    else
                    {
                        Console.WriteLine("save" + save + " is not a valid save file");
                        success = false;
                    }
                }
            }

            return success;
        }

        public static bool LoadSaveFile(bool auto)
        {
            bool success = true;
            if (auto)
            {
                string path = "saves\\save";

                if(newest == 1)
                {
                    path += "1.txt";
                }
                else
                {
                    path += "2.txt";
                }

                using (StreamReader reader = new StreamReader(path))
                {
                    string head = "";
                    head += reader.ReadLine();
                    if (head == "1")
                    {
                        for (int i = 0; i < HaarCascadeV2.finishedFeatures.Length; i++)
                        {
                            string curr = "";
                            curr += reader.ReadLine();
                            double threshold = double.Parse(curr);
                            HaarCascadeV2.UpdateThreshold(threshold, i);
                            curr = "";
                            curr += reader.ReadLine();
                            decimal vote = decimal.Parse(curr);
                            HaarCascadeV2.UpdateVote(vote, i);
                        }
                    }
                    else
                    {
                        success = false;
                        Console.WriteLine("no valid save files");
                    }

                }
                /*string path = "saves\\save2.txt";
                bool save2 = true;
                bool load1 = false;
                using (StreamReader reader = new StreamReader(path))
                {
                    string head = "";
                    head += reader.ReadLine();
                    if (head == "1")
                    {
                        for (int i = 0; i < HaarCascadeV2.finishedFeatures.Length; i++)
                        {
                            string curr = "";
                            curr += reader.ReadLine();
                            int threshold = int.Parse(curr);
                            HaarCascadeV2.finishedFeatures[i].threshold = threshold;
                            curr = "";
                            curr += reader.ReadLine();
                            int vote = int.Parse(curr);
                            HaarCascadeV2.finishedFeatures[i].vote = vote;
                        }
                    }
                    else
                    {
                        save2 = false;
                        load1 = true;
                    }

                }
                if (!save2 || load1)
                {
                    path = "saves\\save1.txt";
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string head = "";
                        head += reader.ReadLine();
                        if (head == "1")
                        {
                            for (int i = 0; i < HaarCascadeV2.finishedFeatures.Length; i++)
                            {
                                string curr = "";
                                curr += reader.ReadLine();
                                int threshold = int.Parse(curr);
                                HaarCascadeV2.finishedFeatures[i].threshold = threshold;
                                curr = "";
                                curr += reader.ReadLine();
                                int vote = int.Parse(curr);
                                HaarCascadeV2.finishedFeatures[i].vote = vote;
                            }
                        }
                        else
                        {
                            Console.WriteLine("no valid saves");
                            success = false;
                        }
                    }
                }*/
            }
            else
            {
                success = false;
            }


            return success;
        }

        private Bitmap ProcessImg(Bitmap imageIn)
        {
            Bitmap imageOut = imageIn;
            if (checkBox.Checked)
            {
                methods.imageToGrayscale(imageIn);
                if (imageIn.Width > pictureBox.Width)
                {
                    imageOut = methods.ScaleDown(imageIn, pictureBox);
                    if (imageIn.Width > pictureBox.Width)
                    {
                        imageOut = methods.CropHorz(imageIn, pictureBox);
                    }
                    if (imageIn.Height > pictureBox.Height)
                    {
                        imageOut = methods.CropVert(imageIn, pictureBox);
                    }
                }
                else if (imageIn.Width < pictureBox.Width)
                {
                    imageOut = methods.BicubicScaleUp(imageIn, pictureBox, richTextBox1);
                    if (imageIn.Width > pictureBox.Width)
                    {
                        imageOut = methods.CropHorz(imageIn, pictureBox);
                    }
                    if (imageIn.Height > pictureBox.Height)
                    {
                        imageOut = methods.CropVert(imageIn, pictureBox);
                    }
                }
                else if (imageIn.Height > pictureBox.Height)
                {
                    imageOut = methods.CropVert(imageIn, pictureBox);
                }
                else { imageOut = imageIn; }

            }
            return imageOut;
        }

        private void ConvToInt_Click_1(object sender, EventArgs e)
        {
            methods.convertToIntImg(pictureBox, ref richTextBox1, ref integralImages, idx);
        }

        public class HaarCascadeV2
        {
            int size = 46;
            public static int amtFeatures = 13;
            public static HaarFeatureV2[] features;
            public static HaarFeatureV2[] finishedFeatures;

            public HaarCascadeV2()
            {
                //just a blank overload so i can instantiate it in the Form1 constructor
            }

            public HaarCascadeV2(bool[] isFace, imageProcessorClass methods)
            {
                features = new HaarFeatureV2[amtFeatures];
                features[0] = new Type1(2, 9, 9, 9, 2, 23, 9, 23, true); //        rightear


                features[1] = new Type2(10, 9, 35, 9, 10, 17, 35, 17, true); //     eyes
                features[11] = new Type2(10, 7, 35, 7, 10, 15, 35, 15, true); //     eyes2
                features[12] = new Type2(10, 5, 35, 5, 10, 13, 35, 13, true); //     eyes



                features[2] = new Type1(36, 9, 43, 9, 36, 23, 43, 23, true); //     leftear
                features[3] = new Type1(17, 18, 24, 18, 17, 25, 24, 25, false); //  leftSectNose
                features[4] = new Type1(21, 18, 28, 18, 21, 25, 28, 25, true); //   rightSectNose
                features[5] = new Type2(14, 28, 31, 29, 14, 36, 31, 36, false); //  mouth
                features[6] = new Type2(10, 19, 16, 19, 10, 24, 16, 24, true);  //  cheekboneLeft
                features[7] = new Type2(29, 19, 35, 19, 29, 24, 35, 24, true);  //  cheekboneright
                features[8] = new Type2(15, 37, 30, 37, 15, 44, 30, 44, true);  //  chin
                features[9] = new Type1(6, 25, 11, 25, 6, 30, 11, 30, false); //    leftEdge
                features[10] = new Type1(34, 25, 39, 25, 34, 30, 39, 30, true); //    rightEdge


                finishedFeatures = new HaarFeatureV2[amtFeatures];
            }

            public HaarCascadeV2(imageProcessorClass methods)
            {
                finishedFeatures = new HaarFeatureV2[amtFeatures];
                finishedFeatures[0] = new Type1(2, 9, 9, 9, 2, 23, 9, 23, true); //        rightear
                
                finishedFeatures[1] = new Type2(10, 9, 35, 9, 10, 17, 35, 17, true); //     eyes
                finishedFeatures[11] = new Type2(10, 7, 35, 7, 10, 15, 35, 15, true); //     eyes2
                finishedFeatures[12] = new Type2(10, 5, 35, 5, 10, 13, 35, 13, true); //     eyes
                
                finishedFeatures[2] = new Type1(36, 9, 43, 9, 36, 23, 43, 23, true); //     leftear
                finishedFeatures[3] = new Type1(17, 18, 24, 18, 17, 25, 24, 25, false); //  leftSectNose
                finishedFeatures[4] = new Type1(21, 18, 28, 18, 21, 25, 28, 25, true); //   rightSectNose
                finishedFeatures[5] = new Type2(14, 28, 31, 29, 14, 36, 31, 36, false); //  mouth
                finishedFeatures[6] = new Type2(10, 19, 16, 19, 10, 24, 16, 24, true);  //  cheekboneLeft
                finishedFeatures[7] = new Type2(29, 19, 35, 19, 29, 24, 35, 24, true);  //  cheekboneright
                finishedFeatures[8] = new Type2(15, 37, 30, 37, 15, 44, 30, 44, true);  //  chin
                finishedFeatures[9] = new Type1(6, 25, 11, 25, 6, 30, 11, 30, false); //    leftEdge
                finishedFeatures[10] = new Type1(34, 25, 39, 25, 34, 30, 39, 30, true); //    rightEdge
            }

            public bool[] Test(double[][,] intImgs)
            {
                bool[] classifications = new bool[intImgs.Length];
                

                int amtWindows = 0;
                for (int r = 0; r < intImgs.Length; r++)
                {
                    if (intImgs[r].GetLength(0) == 46 && intImgs[r].GetLength(1) == 46)
                    {

                        //instantiates and classifies with window
                        TestWindow haarWindow = new TestWindow(intImgs[r]);
                        Console.WriteLine("window " + amtWindows + " is " + haarWindow.classifyWindow());
                        Console.WriteLine();
                        amtWindows++;
                    }
                    else
                    {
                        //loops through intImg
                        for (int i = 0; i < intImgs[r].GetLength(0) - size; i++)
                        {
                            for (int j = 0; j < intImgs[r].GetLength(1) - size; j++)
                            {
                                double[,] windowImg = new double[size, size];

                                //Has to Loop through a section of the intImg to get the window
                                for (int p = 0; p < size; p++)
                                {
                                    for (int q = 0; q < size; q++)
                                    {
                                        windowImg[p, q] = intImgs[r][p + i, q + j];
                                    }
                                }

                                //instantiates and classifies with window
                                TestWindow haarWindow = new TestWindow(windowImg);
                                Console.WriteLine("window " + amtWindows + " is " + haarWindow.classifyWindow());
                                Console.WriteLine();
                                amtWindows++;
                            }
                        }
                    }
                }





                return classifications;
            }

            public static void UpdateThreshold(double threshold, int i)
            {
                finishedFeatures[i].threshold = threshold;
            }

            public static void UpdateVote(decimal vote, int i)
            {
                finishedFeatures[i].vote = vote;
            }

            public int CreateStump(int amtWindows, int[] wrongPerFeature, ref List<int> finishedStumps)
            {
                Decimal minGini = Decimal.MaxValue;
                int idx = 0;
                for (int i = 0; i < amtFeatures; i++)
                {
                    features[i].UpdateGiniCoef(amtWindows - wrongPerFeature[i], wrongPerFeature[i], amtWindows);
                    if (features[i].giniCoef < minGini && !finishedStumps.Contains(i))
                    {
                        minGini = features[i].giniCoef;
                        idx = i;
                    }
                }
                if (features[idx].GetType().Name == "Type1")
                {
                    finishedFeatures[idx] = new Type1(features[idx].xCoords[0], features[idx].yCoords[0], features[idx].xCoords[1], features[idx].yCoords[1], features[idx].xCoords[2], features[idx].yCoords[2], features[idx].xCoords[3], features[idx].yCoords[3], features[idx].GetBlackLocation());
                }
                else
                {
                    finishedFeatures[idx] = new Type2(features[idx].xCoords[0], features[idx].yCoords[0], features[idx].xCoords[1], features[idx].yCoords[1], features[idx].xCoords[2], features[idx].yCoords[2], features[idx].xCoords[3], features[idx].yCoords[3], features[idx].GetBlackLocation());
                }
                finishedFeatures[idx].giniCoef = minGini;
                Decimal errorForFeature = Decimal.Divide(wrongPerFeature[idx], amtWindows);
                finishedFeatures[idx].UpdateVote(errorForFeature);
                finishedFeatures[idx].threshold = features[idx].threshold;
                finishedStumps.Add(idx);
                return idx;
            }

            public double[][] getThresholds(ref List<windowTrainer> Data, int amtWindows, imageProcessorClass methods)
            {
                List<double>[] thresholds = new List<double>[amtFeatures];
                Decimal[][] giniImpurity = new decimal[amtFeatures][];

                double[][] ValuesPerKernel = new double[amtFeatures][];
                for (int i = 0; i < amtFeatures; i++)
                {
                    ValuesPerKernel[i] = new double[amtWindows];
                    giniImpurity[i] = new decimal[amtWindows - 1];
                    thresholds[i] = new List<double>();
                }

                

                int yesAfter = 0;
                int noAfter = 0;
                Console.WriteLine("threshold variables instantiated");
                for (int i = 0; i < Data.Count; i++)
                {
                    double[] kernelVals = Data[i].getKernelValues();
                    for (int j = 0; j < amtFeatures; j++)
                    {
                        ValuesPerKernel[j][i] = kernelVals[j];
                    }

                    //tallying total yes(s) and no(s) in this loop so that i can save time looping later
                    if (Data[i].isFace)
                    {
                        yesAfter++;
                    }
                    else
                    {
                        noAfter++;
                    }
                }

                Console.WriteLine("line 297: yes(s) and no(s) tallied");

                for(int k = 0; k < amtFeatures; k++)
                {
                    ValuesPerKernel[k] = quickSort(ValuesPerKernel[k]);
                }
                

                for (int i = 0; i < amtFeatures; i++)
                {
                    int yesAfterTemp = yesAfter;
                    int noAfterTemp = noAfter;
                    int yes = 0;
                    int no = 0;
                    for (int j = 0; j < amtWindows - 1; j++)
                    {
                        thresholds[i].Add((ValuesPerKernel[i][j + 1] + ValuesPerKernel[i][j]) / 2);
                        int indexOfData = methods.BinarySearch(ValuesPerKernel[i], Data[j].internalKernelValues[i]);
                        if (Data[indexOfData].isFace)
                        {
                            yes++;
                            yesAfterTemp--;
                        }
                        else
                        {
                            no++;
                            noAfterTemp--;
                        }

                        double impurityBefore = 1 - Math.Pow(yes / (yes + no), 2) - Math.Pow(no / (yes + no), 2);
                        double impurityAfter = 1 - Math.Pow(yesAfterTemp / (yesAfterTemp + noAfterTemp), 2) - Math.Pow(noAfterTemp / (yesAfterTemp + noAfterTemp), 2);
                        Decimal before = Decimal.Multiply(Decimal.Divide((yes + no), yes + no + yesAfter + noAfter), (Decimal)impurityBefore);
                        Decimal after = Decimal.Multiply(Decimal.Divide((yesAfter + noAfter), yes + no + yesAfter + noAfter), (Decimal)impurityAfter);
                        giniImpurity[i][j] = before + after;
                    }
                    Decimal min = Decimal.MaxValue;
                    int index = 0;
                    for (int j = 0; j < giniImpurity[i].Length; j++)
                    {
                        int temp = Decimal.Compare(giniImpurity[i][j], min);
                        if (temp < 0)
                        {
                            min = giniImpurity[i][j];
                            index = j;
                        }
                    }
                    features[i].threshold = thresholds[i][index];



                    //need to implement negative classifications so if a classification is negative, that means the results are flipped or something.

                    //finishedFeatures have 0 threshold, -infinity vote and gini coef 1.

                }
                return ValuesPerKernel;
            }

            public double[] quickSort(double[] nums)
            {
                double[] res;
                List<double> less = new List<double>();
                List<double> more = new List<double>();
                if (nums.Length > 1)
                {
                    res = new double[nums.Length];
                    for (int i = 1; i < nums.Length; i++)
                    {
                        if (nums[i] < nums[0])
                        {
                            less.Add(nums[i]);
                        }
                        else
                        {
                            more.Add(nums[i]);
                        }
                    }
                    if (less.Count > 0)
                    {
                        double[] before = quickSort(less.ToArray());
                        for (int i = 0; i < before.Length; i++)
                        {
                            res[i] = before[i];
                        }
                    }

                    res[less.Count] = nums[0];

                    if (more.Count > 0)
                    {
                        double[] after = quickSort(more.ToArray());
                        for (int i = 0; i < after.Length; i++)
                        {
                            res[less.Count + 1 + i] = after[i];
                        }
                    }
                    return res;
                }
                else
                {
                    res = new double[1];
                    res[0] = nums[0];
                    return res;
                }
            }

            public void classifyInWindowTrain(double[][,] intImgs, bool[] isFace, imageProcessorClass methods)
            {
                //instantiate all variables
                List<int>[] indexOfErrorCurrFeature = new List<int>[amtFeatures];
                int amtWindows = 0;
                List<int> featuresAdded = new List<int>();
                int[] wrongPerFeature = new int[amtFeatures];
                List<int>[] indexOfErrorOfLastFeature = new List<int>[amtFeatures];
                for (int i = 0; i < amtFeatures; i++)
                {
                    wrongPerFeature[i] = 0;
                    indexOfErrorOfLastFeature[i] = new List<int>();
                }

                List<windowTrainer> Data = new List<windowTrainer>();
                List<bool> internalIsFace = new List<bool>();

                Console.WriteLine("variables instantiated");

                //loops through all images
                //creates windows and stores them in a parallel array with a
                //boolean array that says if it is a face or not
                for (int r = 0; r < intImgs.Length; r++)
                {
                    if (intImgs[r].GetLength(0) == 46 && intImgs[r].GetLength(1) == 46)
                    {

                        if (isFace[r])
                        {
                            internalIsFace.Add(true);
                        }
                        else
                        {
                            internalIsFace.Add(false);
                        }

                        //instantiates and classifies with window
                        windowTrainer haarWindow = new windowTrainer(intImgs[r], internalIsFace[amtWindows]);
                        amtWindows++;

                        Data.Add(haarWindow);
                    }
                    else
                    {
                        //loops through intImg
                        for (int i = 0; i < intImgs[r].GetLength(0) - size; i++)
                        {
                            for (int j = 0; j < intImgs[r].GetLength(1) - size; j++)
                            {
                                double[,] windowImg = new double[size, size];

                                //Has to Loop through a section of the intImg to get the window
                                for (int p = 0; p < size; p++)
                                {
                                    for (int q = 0; q < size; q++)
                                    {
                                        windowImg[p, q] = intImgs[r][p + i, q + j];
                                    }
                                }

                                if (isFace[r])
                                {
                                    internalIsFace.Add(true);
                                }
                                else
                                {
                                    internalIsFace.Add(false);
                                }

                                //instantiates and classifies with window
                                windowTrainer haarWindow = new windowTrainer(windowImg, internalIsFace[amtWindows]);
                                amtWindows++;

                                Data.Add(haarWindow);
                            }
                        }
                    }
                    Console.WriteLine("image " + r + " finished");
                }

                Console.WriteLine("all windows created");


                //gets the thresholds                                                                                             <------- check later
                double[][] ValuesPerKernel = getThresholds(ref Data, amtWindows, methods);

                Console.WriteLine("found thresholds");

                for (int i = 0; i < Data.Count; i++)
                {
                    Data[i].sampleWeight = Decimal.Divide(1, Data.Count);
                    Data[i].getClassificationTrain();

                    //tallys any error for each individual feature
                    for (int k = 0; k < amtFeatures; k++)
                    {
                        if (Data[i].WrongPerFeature[k])
                        {
                            wrongPerFeature[k]++;
                            indexOfErrorOfLastFeature[k].Add(i);
                        }
                    }
                }

                Console.WriteLine("found error for first feature");

                //stores index of each finished stump for easy checking
                List<int> finishedStumps = new List<int>();

                //makes first complete stump and stores its index                                                                                      <---------Check
                int idx = CreateStump(amtWindows, wrongPerFeature, ref finishedStumps);

                decimal[] sampleWeightsCumulative = new decimal[amtWindows];

                Console.WriteLine("created first stump, starting the rest");

                //makes rest of stumps, looping until all features have been added to the complete features
                for (int l = 1; l < amtFeatures; l++)
                {
                    double totWeight = 0;

                    for (int windowCount = 0; windowCount < amtWindows; windowCount++)
                    {
                        if (indexOfErrorOfLastFeature[idx].Contains(windowCount))
                        {
                            Data[windowCount].sampleWeight = Decimal.Multiply(Data[windowCount].sampleWeight, (Decimal)Math.Pow(Math.E, (double)features[l].vote));
                            totWeight += (Double)Math.Round(Data[windowCount].sampleWeight, 10);
                        }
                        else
                        {
                            Data[windowCount].sampleWeight = Decimal.Multiply(Data[windowCount].sampleWeight, (Decimal)Math.Pow(Math.E, -(double)features[l].vote));
                            totWeight += (Double)Math.Round(Data[windowCount].sampleWeight, 10);
                        }
                    }
                    Data[0].sampleWeight = Decimal.Divide(Data[0].sampleWeight, (decimal)totWeight);
                    sampleWeightsCumulative[0] = Data[0].sampleWeight;
                    //normalises all sample weights so add to 1
                    for (int windowCount = 1; windowCount < amtWindows; windowCount++)
                    {
                        Data[windowCount].sampleWeight = Decimal.Divide(Data[windowCount].sampleWeight, (decimal)totWeight);

                        sampleWeightsCumulative[windowCount] = Decimal.Add(Data[windowCount].sampleWeight, sampleWeightsCumulative[windowCount - 1]);
                    }

                    //creates a new random set of windows to avoid overfitting
                    Random rand = new Random();
                    windowTrainer[] newData = new windowTrainer[amtWindows];
                    for (int i = 0; i < amtWindows; i++)
                    {
                        double Random = rand.NextDouble();

                        int index = methods.BinarySearch(sampleWeightsCumulative, (decimal)Random); //returns index of or nearest index to the random number value

                        bool invalid = true;
                        while (invalid)
                        {
                            if(index >= 0)
                            {
                                invalid = false;
                            }
                            else
                            {
                                Random = rand.NextDouble();

                                index = methods.BinarySearch(sampleWeightsCumulative, (decimal)Random);
                            }
                        }
                        newData[i] = Data[index];
                        newData[i].sampleWeight = Decimal.Divide(1, amtWindows);
                    }

                    for (int i = 0; i < amtFeatures; i++)
                    {
                        indexOfErrorOfLastFeature[i] = new List<int>();
                        wrongPerFeature[i] = 0;
                    }

                    //builds next stump
                    for (int i = 0; i < amtWindows; i++)
                    {
                        //tallys any error for each individual feature
                        for (int k = 0; k < amtFeatures; k++)
                        {
                            if (Data[i].WrongPerFeature[k])
                            {
                                wrongPerFeature[k]++;
                                indexOfErrorOfLastFeature[k].Add(i);
                            }
                        }
                        Data[i] = newData[i];
                    }

                    idx = CreateStump(amtWindows, wrongPerFeature, ref finishedStumps);
                    Console.WriteLine((l + 1) + ". stump created as stump " + idx);
                }

                Console.WriteLine("training finished");
            }

            public class TestWindow
            {
                double[,] windowIntImg;
                public TestWindow(double[,] windowIntImg)
                {
                    this.windowIntImg = windowIntImg;
                }
                public bool classifyWindow()
                {
                    double pos = 0;
                    double neg = 0;
                    for (int i = 0; i < finishedFeatures.Length; i++)
                    {
                        double temp = finishedFeatures[i].compareToKernel(windowIntImg);
                        if (finishedFeatures[i].threshold >= 0)
                        {
                            if (temp >= finishedFeatures[i].threshold)
                            {
                                pos += (double)Math.Round(finishedFeatures[i].vote, 10);
                            }
                            else
                            {
                                neg += (double)Math.Round(finishedFeatures[i].vote, 10);
                            }
                        }
                        else
                        {
                            if (temp <= finishedFeatures[i].threshold)
                            {
                                pos += (double)Math.Round(finishedFeatures[i].vote, 10);
                            }
                            else
                            {
                                neg += (double)Math.Round(finishedFeatures[i].vote, 10);
                            }
                        }
                        /*if (finishedFeatures[i].vote >= 0)
                        {
                            if (temp >= finishedFeatures[i].threshold)
                            {
                                pos += (double)Math.Round(finishedFeatures[i].vote, 10);
                            }
                            else
                            {
                                neg += (double)Math.Round(finishedFeatures[i].vote, 10);
                            }
                        }
                        else
                        {
                            if (temp <= finishedFeatures[i].threshold)
                            {
                                pos += (double)Math.Round(finishedFeatures[i].vote, 10);
                            }
                            else
                            {
                                neg += (double)Math.Round(finishedFeatures[i].vote, 10);
                            }
                        }*/

                    }

                    if (pos > neg)
                    {
                        Console.WriteLine("confidence = " + (double)(Decimal.Divide((Decimal)pos, (Decimal)(pos + neg))));
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("confidence = " + (double)(Decimal.Divide((Decimal)neg, (Decimal)(pos + neg))));
                        return false;
                    }
                }
            }

            public class windowTrainer
            {
                double[,] windowIntImg;
                public bool[] WrongPerFeature;
                public Decimal sampleWeight;
                List<int> indexOfWrongFeatures;
                public bool isFace;
                public double[] internalKernelValues;

                public windowTrainer(double[,] windowIntImg, bool isFaceIn)
                {
                    WrongPerFeature = new bool[amtFeatures];
                    for (int i = 0; i < amtFeatures; i++)
                    {
                        WrongPerFeature[i] = false;
                    }
                    this.windowIntImg = windowIntImg;
                    isFace = isFaceIn;
                    indexOfWrongFeatures = new List<int>();
                }

                public double[] getKernelValues()
                {
                    double[] res = new double[amtFeatures];
                    for (int i = 0; i < amtFeatures; i++)
                    {
                        res[i] = features[i].compareToKernel(windowIntImg);
                    }
                    internalKernelValues = res;
                    return res;
                }

                public bool[] getClassificationTrain()
                {
                    bool[] res = new bool[amtFeatures];
                    for (int i = 0; i < amtFeatures; i++)
                    {
                        double temp = features[i].compareToKernel(windowIntImg);
                        if (features[i].threshold >= 0)
                        {
                            if (temp >= features[i].threshold)
                            {
                                res[i] = true;
                                if (!this.isFace)
                                {
                                    WrongPerFeature[i] = true;
                                    indexOfWrongFeatures.Add(i);
                                }
                            }
                            else
                            {
                                res[i] = false;
                                if (this.isFace)
                                {
                                    WrongPerFeature[i] = true;
                                    indexOfWrongFeatures.Add(i);
                                }
                            }
                        }
                        else
                        {
                            if (temp <= features[i].threshold)
                            {
                                res[i] = true;
                                if (!this.isFace)
                                {
                                    WrongPerFeature[i] = true;
                                    indexOfWrongFeatures.Add(i);
                                }
                            }
                            else
                            {
                                res[i] = false;
                                if (this.isFace)
                                {
                                    WrongPerFeature[i] = true;
                                    indexOfWrongFeatures.Add(i);
                                }
                            }
                        }
                    }
                    return res;
                }
            }

            public partial class HaarFeatureV2
            {
                public int[] xCoords;
                public int[] yCoords;

                public int[] intImgCoordsX;
                public int[] intImgCoordsY;

                public Decimal vote;
                public Decimal giniCoef;
                public double threshold;
                //    1 _______ 2
                //      |     |
                //      |     |              <-- XYcoords
                //    3 ------- 4

                //    3 _______ 2
                //      |     |
                //      |     |              <-- intImgCoords
                //    1 ------- 0
                public HaarFeatureV2(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4)
                {
                    xCoords = new int[4];
                    xCoords[0] = X1;
                    xCoords[1] = X2;
                    xCoords[2] = X3;
                    xCoords[3] = X4;
                    yCoords = new int[4];
                    yCoords[0] = Y1;
                    yCoords[1] = Y2;
                    yCoords[2] = Y3;
                    yCoords[3] = Y4;

                    intImgCoordsX = new int[4];
                    intImgCoordsX[0] = X4;
                    intImgCoordsX[1] = X3 - 1;
                    intImgCoordsX[2] = X2;
                    intImgCoordsX[3] = X1 - 1;

                    intImgCoordsY = new int[4];
                    intImgCoordsY[0] = Y4;
                    intImgCoordsY[1] = Y3;
                    intImgCoordsY[2] = Y2 - 1;
                    intImgCoordsY[3] = Y1 - 1;
                }
                public void UpdateGiniCoef(int amountCorrect, int amountIncorrect, int amtOfRecords)
                {
                    decimal propCorrect = Decimal.Divide(amountCorrect, amtOfRecords);
                    decimal propIncorrect = Decimal.Divide(amountIncorrect, amtOfRecords);
                    Decimal temp = (Decimal)(1 - Math.Pow((double)propCorrect, 2) - Math.Pow((double)propIncorrect, 2));
                    giniCoef = temp;
                }
                public void UpdateVote(Decimal totError)
                {
                    Decimal temp;
                    if (totError == 0)
                    {
                        temp = Decimal.Divide(Decimal.Add(Decimal.Subtract(1, totError), (Decimal)0.005), Decimal.Add(totError, (Decimal)0.005));
                    }
                    else if (totError == 1)
                    {
                        temp = Decimal.Divide(Decimal.Subtract(1, Decimal.Subtract(totError, (Decimal)0.005)), Decimal.Subtract(totError, (Decimal)0.005));
                    }
                    else
                    {
                        temp = Decimal.Divide(Decimal.Subtract(1, totError), totError);
                    }
                    vote = Decimal.Multiply(Decimal.Divide(1, 2), (Decimal)Math.Log((double)temp));

                }
                public virtual double compareToKernel(double[,] windowIntImg) { return 0; }
                public virtual bool GetBlackLocation()
                {
                    return false;
                }
            }
            public class Type1 : HaarFeatureV2
            {
                public int[] midY;
                public int[] midX;
                bool blackOnRight;

                public Type1(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, bool blackOnRight) : base(X1, Y1, X2, Y2, X3, Y3, X4, Y4)
                {
                    midX = new int[2];
                    midY = new int[2];

                    midX[0] = (int)((X3 + X4) / 2) - 1;
                    midY[0] = Y4;
                    midX[1] = midX[0];
                    midY[1] = Y2 - 1;

                    this.blackOnRight = blackOnRight;
                }

                public override double compareToKernel(double[,] intImgWindow)
                {
                    double blackVal = 0;
                    double whiteVal = 0;

                    if (blackOnRight)
                    {
                        blackVal = intImgWindow[intImgCoordsX[0], intImgCoordsY[0]] - intImgWindow[midX[0], midY[0]] - intImgWindow[intImgCoordsX[2], intImgCoordsY[2]] + intImgWindow[midX[1], midY[1]];
                        whiteVal = intImgWindow[midX[0], midY[0]] - intImgWindow[intImgCoordsX[1], intImgCoordsY[1]] - intImgWindow[midX[1], midY[1]] + intImgWindow[intImgCoordsX[3], intImgCoordsY[3]];
                    }
                    else
                    {
                        whiteVal = intImgWindow[intImgCoordsX[0], intImgCoordsY[0]] - intImgWindow[midX[0], midY[0]] - intImgWindow[intImgCoordsX[2], intImgCoordsY[2]] + intImgWindow[midX[1], midY[1]];
                        blackVal = intImgWindow[midX[0], midY[0]] - intImgWindow[intImgCoordsX[1], intImgCoordsY[1]] - intImgWindow[midX[1], midY[1]] + intImgWindow[intImgCoordsX[3], intImgCoordsY[3]];
                    }

                    return whiteVal - blackVal;
                }
                public override bool GetBlackLocation()
                {
                    return blackOnRight;
                }
            }
            public class Type2 : HaarFeatureV2
            {
                public int[] midY;
                public int[] midX;
                bool blackOnBottom;

                //           1 _______ 2
                //      mid[0] |     | mid[1]
                //             |     |              <-- XYcoords
                //           3 ------- 4


                public Type2(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, bool blackOnBottom) : base(X1, Y1, X2, Y2, X3, Y3, X4, Y4)
                {
                    midX = new int[2];
                    midY = new int[2];

                    midX[0] = X1 - 1;
                    midY[0] = (int)((Y1 + Y3) / 2) - 1;
                    midX[1] = X2;
                    midY[1] = (int)((Y2 + Y3) / 2) - 1;

                    this.blackOnBottom = blackOnBottom;
                }

                public override double compareToKernel(double[,] intImgWindow)
                {
                    
                    double blackVal = 0;
                    double whiteVal = 0;

                    if (blackOnBottom)
                    {
                        blackVal = intImgWindow[intImgCoordsX[0], intImgCoordsY[0]] - intImgWindow[intImgCoordsX[1], intImgCoordsY[1]] - intImgWindow[midX[1], midY[1]] + intImgWindow[midX[0], midY[0]];
                        whiteVal = intImgWindow[midX[1], midY[1]] - intImgWindow[midX[0], midY[0]] - intImgWindow[intImgCoordsX[2], intImgCoordsY[2]] + intImgWindow[intImgCoordsX[3], intImgCoordsY[3]];
                    }
                    else
                    {
                        whiteVal = intImgWindow[intImgCoordsX[0], intImgCoordsY[0]] - intImgWindow[intImgCoordsX[1], intImgCoordsY[1]] - intImgWindow[midX[1], midY[1]] + intImgWindow[midX[0], midY[0]];
                        blackVal = intImgWindow[midX[1], midY[1]] - intImgWindow[midX[0], midY[0]] - intImgWindow[intImgCoordsX[2], intImgCoordsY[2]] + intImgWindow[intImgCoordsX[3], intImgCoordsY[3]];
                    }

                    return whiteVal - blackVal;

                }
                public override bool GetBlackLocation()
                {
                    return blackOnBottom;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap temp = (Bitmap)pictureBox.Image;
            methods.imageToGrayscale(temp);
            pictureBox.Image = temp;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (loadSave.Checked)
            {
                saveFile.Visible = true;
                richTextBox1.Text = "enter save file number in adjacent box.";
            }
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

