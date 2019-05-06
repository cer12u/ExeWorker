using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OcvSharpWorker
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OCVtest ot = new OCVtest();
            //ot.EdgeDetect("test.png");
            Console.WriteLine(ot.MatchLevel("test.png", "test4.png", true));
            Console.WriteLine(ot.MatchLevel("test.png", "test4.png"));


        }

        ScreenCompare sc1 = new ScreenCompare();

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            //Console.WriteLine(sc1.Append("test.png"));
            //sc1.Append("test.png");
            //Console.WriteLine(sc1.Append("test4.png"));
            //Console.WriteLine(sc1.Append("test.png"));
            //Console.WriteLine(sc1.Append("test.png"));
            //Console.WriteLine(sc1.Append("test3.png"));

            //Mat m = Mat.FromStream(sc1.GetDataStream(0), ImreadModes.Color);
            //Cv2.ImShow("1", m);
            //Mat m = new Mat((IntPtr))

            //Console.WriteLine("Res = " + sc1.CompareFile("test2.png"));

            //Console.WriteLine(sc1.Append("test2.png"));
            foreach(int i in Enumerable.Range(0, 100))
            {
                int j = sc1.Append("test.jpg");
                sc1.Remove(j);
                
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class ScreenCompare
        {
            static private int ListCounter = 0;
            private static int MaxFileSize = 30 * 1024 * 1024;

            public double thresh { get; set; } = 0.8;

            private class ImageInfo :IDisposable
            {

                public int id = -1;
                public string FullName { get; set; } = string.Empty;
                public string Descript { get; set; } = string.Empty;

                public int bufferSize = 1024 * 1024;
                public byte[] hash = new byte[] { };
                public byte[] data = new byte[] { };
                public bool OnMemory { get; private set; } = true;

                public System.IO.Stream DataStream { get; private set; } = null;
                
                public string Name
                {
                    get
                    {
                        return System.IO.Path.GetFileName(FullName);
                    }
                }

                public bool SetData(string fileName)
                {
                    if (DataStream != null)
                        return false;
                    if (string.IsNullOrEmpty(fileName))
                        return false;
                    if (!System.IO.File.Exists(fileName))
                        return false;

                    if(string.IsNullOrEmpty(FullName))
                        FullName = System.IO.Path.GetFullPath(fileName);

                    System.IO.FileInfo fi = new System.IO.FileInfo(fileName);

                    if (fi.Length > MaxFileSize)
                        OnMemory = false;

                    try
                    {
                        if (OnMemory)
                        {
                            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open))
                            {
                                int SeekPos = 0;
                                data = new byte[fs.Length];
                                while (fs.CanRead)
                                {
                                    int readSize = fs.Read(data, SeekPos, bufferSize > ((int)fs.Length - SeekPos) ? ((int)fs.Length - SeekPos) : bufferSize);
                                    if (readSize <= 0)
                                        break;
                                    SeekPos += readSize;
                                }
                                DataStream = new System.IO.MemoryStream(data);
                            }
                        }
                        else
                        {
                            DataStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open);
                        }

                        System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
                        hash = sha1.ComputeHash(DataStream);

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                public void Dispose()
                {
                    if (DataStream != null)
                        DataStream.Dispose();
                    data = new byte[] { };

                }
            }

            private List<ImageInfo> ImageStack = new List<ImageInfo>();

            public int Append(string fileName, string description = null)
            {
                if (string.IsNullOrEmpty(fileName))
                    return -1;
                if (!System.IO.File.Exists(fileName))
                    return -2;

                ImageInfo ii = new ImageInfo();
                ii.FullName = System.IO.Path.GetFullPath(fileName);
                ii.SetData(fileName);

                if (string.IsNullOrEmpty(description))
                    ii.Descript = description;

                int ret = CompareHash(ii);

                if (ret >= 0)
                    return ret;


                ii.id = ListCounter++;
                ImageStack.Add(ii);
                return ii.id;
            }


            public bool Remove(string filename)
            {
                string fn = System.IO.Path.GetFullPath(filename);
                foreach(ImageInfo ii in ImageStack)
                {
                    if (ii.FullName.SequenceEqual(filename))
                    {
                        ImageStack.Remove(ii);
                        return true;
                    }
                }
                return false;
            }
            public bool Remove(int index)
            {
                foreach (ImageInfo ii in ImageStack)
                {
                    if (ii.id == index)
                    {
                        ImageStack.Remove(ii);
                        return true;
                    }
                }
                return false;
            }
            
            private int CompareHash(ImageInfo imageInfo)
            {
                foreach (ImageInfo ii in ImageStack)
                {
                    if (ii.hash.SequenceEqual(imageInfo.hash))
                        return ii.id;
                }
                return -1;
            }


            public void IndexNormalize()
            {
                int tp = 0;
                foreach(ImageInfo ii in ImageStack)
                {
                    ii.id = tp++;
                }
            }

            public System.IO.Stream GetDataStream(int id)
            {
                foreach(ImageInfo ii in ImageStack)
                    if (ii.id == id)
                        return ii.DataStream;
                return null;
            }



            public int CompareFile(string fileName, bool fullScan = false)
            {
                ImageInfo isrc = new ImageInfo();
                if (!isrc.SetData(fileName))
                    return -1;

                //ハッシュレベルで一致ならそのまま返却
                if (!fullScan)
                {
                    int itp = CompareHash(isrc);
                    if(itp >= 0)
                    {
                        isrc.Dispose();
                        return itp;
                    }
                }

                int idx = -1;
                double maxVal = 0;
                Mat src = Mat.FromStream(isrc.DataStream, ImreadModes.AnyColor);
                
                foreach (ImageInfo ii in ImageStack)
                {
                    OpenCvSharp.Mat m = OpenCvSharp.Mat.FromStream(ii.DataStream, ImreadModes.AnyColor);

                    OpenCvSharp.Mat roi = m[0, src.Height > m.Height ? m.Height : src.Height, 0, src.Width > m.Width ? m.Width : src.Width];
                    OpenCvSharp.Mat res = new Mat();

                    Cv2.MatchTemplate(src, roi, res, TemplateMatchModes.CCoeffNormed);
                    double min, max;
                    Cv2.MinMaxLoc(res, out min, out max);
                    

                    if (maxVal < max)
                    {
                        idx = ii.id;
                        maxVal = max;
                    }

                    if (!fullScan && max > thresh)
                    {
                        src.Dispose();
                        return ii.id;
                    }

                    roi.Dispose();
                    m.Dispose();
                }
                
                src.Dispose();
                isrc.Dispose();
                return idx;
            }

        }

        public class OCVtest
        {
            //

            public int LastError = 0;

            public void EdgeDetect(string fileName)
            {

                Mat srcMat = Cv2.ImRead(fileName);

                Mat srcBlue = srcMat.Split()[0];

                Mat cannyMat = new Mat();
                Mat sobelMat = new Mat();
                Mat LaplasMat = new Mat();

                Cv2.Canny(srcBlue, cannyMat, 50, 200, 3);

                //Cv2.ConvertScaleAbs()

                Cv2.Sobel(srcBlue, sobelMat, sobelMat.Type(), 1, 1, 1);

                Cv2.Laplacian(srcBlue, LaplasMat, 3);
                Cv2.ConvertScaleAbs(LaplasMat, LaplasMat, 1, 0);

                ImShow(srcBlue);
                ImShow(cannyMat);
                ImShow(sobelMat);
                ImShow(LaplasMat);

                Mat merge = new Mat(srcMat.Size(),MatType.CV_8U);
                foreach(Mat tm in srcMat.Split())
                {
                    Mat tmm = new Mat();
                    //Cv2.Canny(tm, tmm, 10, 250, 3);
                    Cv2.Laplacian(tm, tmm, 3);
                    Cv2.ConvertScaleAbs(tmm, tmm, 1, 0);
                    ImShow(tmm);
                    Cv2.Threshold(tmm, tmm, 0.5, 255, ThresholdTypes.Binary);

                    Cv2.BitwiseOr(merge, tmm, merge);

                }

                Cv2.MorphologyEx(merge, merge, MorphTypes.Erode, Cv2.GetStructuringElement(MorphShapes.Rect,new OpenCvSharp.Size(3,3), new OpenCvSharp.Point(1,1)));

                ImShow(merge);

                Cv2.WaitKey();



            }

            int Incl = 0;
            public void ImShow(Mat mat)
            {
                Cv2.ImShow(string.Format("{0}", Incl++), mat);
            }
            



            private int splitCountX = 5;
            private int splitCountY = 5;

            /// <summary>
            /// 同じサイズの画像を比較
            /// サイズが違う場合は0
            /// </summary>
            /// <param name="srcPath1"></param>
            /// <param name="srcPath2"></param>
            /// <param name="deep">分割比較の切り替え</param>
            /// <returns>Tuple(maxValue, minValue)</returns>
            public Tuple<double, double> MatchLevel(string srcPath1, string srcPath2, bool deep = false)
            {
                Tuple<double, double> retVal = new Tuple<double, double>(-1,-1);

                if (string.IsNullOrEmpty(srcPath1) || string.IsNullOrEmpty(srcPath2))
                    return retVal;
                if (!System.IO.File.Exists(srcPath1) || !System.IO.File.Exists(srcPath2))
                    return retVal;

                Mat srcMat1 = null;
                Mat srcMat2 = null;

                try
                {
                    srcMat1 = new Mat(srcPath1);
                    srcMat2 = new Mat(srcPath2);

                    if (!srcMat1.Size().Equals(srcMat2.Size()))
                        return retVal;

                    if (!deep) {
                        Mat dst = new Mat();
                        Cv2.MatchTemplate(srcMat1, srcMat2, dst, TemplateMatchModes.CCoeffNormed);

                        double min, max;
                        Cv2.MinMaxLoc(dst, out min, out max);
                        retVal = new Tuple<double, double>(max, max);
                        dst.Dispose();
                    }
                    else
                    {
                        double totalMax = 0;
                        double totalMin = 1;
                        int chunkSizeX = srcMat1.Size().Width / splitCountX;
                        int chunkSizeY = srcMat1.Size().Height / splitCountY;
                        foreach (int x in Enumerable.Range(0, splitCountX))
                        {
                            foreach(int y in Enumerable.Range(0, splitCountY))
                            {
                                double max, min;
                                Mat s1Roi = srcMat1[chunkSizeY * y, chunkSizeY * (y + 1), chunkSizeX * x, chunkSizeX * (x + 1)];
                                Mat s2Roi = srcMat2[chunkSizeY * y, chunkSizeY * (y + 1), chunkSizeX * x, chunkSizeX * (x + 1)];
                                Mat dst = new Mat();
                                Cv2.MatchTemplate(s1Roi, s2Roi, dst, TemplateMatchModes.CCoeffNormed);

                                Cv2.MinMaxLoc(dst, out min, out max);

                                if (totalMax < max)
                                    totalMax = max;

                                if (totalMin > max)
                                    totalMin = max;

                                //Console.WriteLine("{0} -> {1}, {2}", max, totalMax, totalMin);
                                dst.Dispose();
                                s1Roi.Dispose();
                                s2Roi.Dispose();
                            }
                        }

                        retVal = new Tuple<double, double>(totalMax, totalMin);
                    }
                }
                catch
                {
                    LastError = -900;
                }
                finally
                {
                    srcMat1.Dispose();
                    srcMat2.Dispose();
                }
                return retVal;

            }



            public void CheckTrim(string srcPath1, string srcPath2)
            {
                if (string.IsNullOrEmpty(srcPath1) || string.IsNullOrEmpty(srcPath2))
                    return;
                if (!System.IO.File.Exists(srcPath1) || !System.IO.File.Exists(srcPath2))
                    return;

                Mat s1 = new Mat(srcPath1);
                Mat s2 = new Mat(srcPath2);

                if (s1.Size().Width < s2.Size().Width && s1.Size().Height < s2.Size().Height)
                {
                    Mat swp = s1;
                    s1 = s2;
                    s2 = swp;
                }else if(s1.Size().Width > s2.Size().Width && s1.Size().Height > s2.Size().Height)
                {

                }
                else
                {
                    Console.WriteLine("Cannot Compare");
                    return;
                }

                Mat res = new Mat();


                Cv2.MatchTemplate(s1, s2, res, TemplateMatchModes.CCorrNormed);

                double min, max;

                Cv2.MinMaxLoc(res, out min, out max);


                Console.WriteLine("{0}, {1}", min, max);

                //System.IO.FileStream fs1 = null;
                //System.IO.FileStream fs2 = null;
                //try
                //{
                //    fs1 = new System.IO.FileStream(srcPath1, System.IO.FileMode.Open);
                //    fs2 = new System.IO.FileStream(srcPath2, System.IO.FileMode.Open);

                //    FullMatch(fs1, fs2);

                //}
                //finally
                //{
                //    if (fs1 != null)
                //        fs1.Dispose();
                //    if (fs2 != null)
                //        fs2.Dispose();
                //}
            }

            //public void FullMatch(System.IO.Stream src1, System.IO.Stream src2)
            //{
            //}


        }

    }
}
