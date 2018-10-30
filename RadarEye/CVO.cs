using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RadarEye
{
    public class Circle {
       public int X, Y, Radius;
        public Circle(double x, double y, double r)
        {
            X = (int)x;
            Y = (int)y;
            Radius = (int)r;
        }
    }
    public class CVO
    {
        public bool stop = false;
        public int kk = 0;
        public bool isRecordingCamera = false;
        
        public CvCapture cap;
        public Image wpfImage;
        public Label label;
        ConcurrentQueue<IplImage> recordFrames;
        ConcurrentQueue<IplImage> showFrames;
        Window clickWindow;
        int fps = Conf.fps;
        double fx = 766, fy = 766, cx = 350, cy = 255;
        //double fx = 766, fy = 766, cx = 335, cy = 275;

        public CVO() { }
        public CVO(CvCapture cvCap, Image image)
        {
            cap = cvCap;
            wpfImage = image;

        }
        
        private void UpdateImage(Image im, WriteableBitmap wb)
        {
            im.Source = wb;
            

        }
        public void CollectFrames()
        {
            int k = 0;
            recordFrames = new ConcurrentQueue<IplImage>();
            showFrames = new ConcurrentQueue<IplImage>();
            while (!stop)
            {
                IplImage img;
                img = cap.QueryFrame();
                showFrames.Enqueue(img);
                if (isRecordingCamera)
                    recordFrames.Enqueue(img);
                Thread.Sleep(1000 / fps);
                k++;

            }
            Console.WriteLine(k);
        }
        public void showCamera()
        {
            new Thread(CollectFrames).Start();

            new Thread(() =>
            {
                int k = 0;
                Thread.Sleep(100);
                while (!stop)
                {

                    IplImage img;
                    bool suc = showFrames.TryDequeue(out img);
                    
                    
                    k++;
                    if (suc)
                        wpfImage.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                UpdateImage(wpfImage, WriteableBitmapConverter.ToWriteableBitmap(img, PixelFormats.Bgr24));
                            }
                            catch (Exception e) { }


                        }));
                    //stop = true;
                    Thread.Sleep(1000 / fps);
                }
                cap.Dispose();
                Console.WriteLine(k);


            }).Start();





        }
        public void recordCamera()
        {
            isRecordingCamera = true;
            int k = 0;

            Thread.Sleep(100);

            new Thread(() =>
            {
                IplImage img;
                recordFrames.TryPeek(out img);




                OpenCvSharp.CPlusPlus.Size dsize = new OpenCvSharp.CPlusPlus.Size(cap.FrameWidth, cap.FrameHeight);
                VideoWriter writer = new VideoWriter(Conf.filepath + "1.avi", FourCC.MJPG, fps, dsize, true);
                while (true)
                {

                    Mat gray = new Mat();
                    Mat canny = new Mat();
                    Mat dst = new Mat();
                    try
                    {
                        bool suc = recordFrames.TryDequeue(out img);
                        if (suc)
                        {
                            Mat frame = new Mat(img, true);
                            //Cv2.CvtColor(frame, gray, ColorConversion.BgrToGray);
                            //Cv2.Canny(gray, canny, 100, 180);
                            //Cv2.Resize(canny, dst, dsize, 0, 0, Interpolation.Linear);
                            // Write mat to VideoWriter
                            writer.Write((frame));
                            k++;
                        }
                        else
                        {
                            Console.WriteLine("ffff");
                            if (isRecordingCamera == false)
                                break;
                        }

                        
                        //stop = true;
                        Thread.Sleep(1000 / fps -10);
                    }
                    catch (Exception e)
                    {
                        label.Content = e.ToString();

                    }
                }
                Console.WriteLine(k);
                writer.Release();
                return;
                //  writer.Dispose();
                // cap.Dispose();


            }).Start();
        }
        public void StopRecord()
        {
            isRecordingCamera = false;
        }
        public void DrawDLInVedio(String videoFile, String outputFile, String targetFile,String indexFile) {
            VideoCapture capture = new VideoCapture(videoFile);
            Mat image = new Mat();
            OpenCvSharp.CPlusPlus.Size dsize = new OpenCvSharp.CPlusPlus.Size(capture.FrameWidth, capture.FrameHeight);
            VideoWriter writer = new VideoWriter(outputFile, FourCC.MJPG, fps, dsize, true);
            int k = 0;
            List<string> targetList = IOTools.ReadListFromTxt(targetFile);
            List<string> indexList = IOTools.ReadListFromTxt(indexFile);
            int t = 0;
            while (capture.Read(image))
            {
                String[] ss = targetList[t].Split(' ');
                
                if (t< indexList.Count-1 && k == int.Parse(indexList[t]))
                {
                    Cv2.Circle(image, (int)double.Parse(ss[0]), (int)double.Parse(ss[1]), 10, new Scalar(0, 0, 0),
                 2
                 );

                    t++;
                }
                writer.Write(image);
                k++;

            };
            writer.Release();
            
        }
        public void TrackRedVedio(String videoFile,String outputFile,String targetFile) {
            VideoCapture capture = new VideoCapture(videoFile);
            Mat image = new Mat();
            OpenCvSharp.CPlusPlus.Size dsize = new OpenCvSharp.CPlusPlus.Size(capture.FrameWidth, capture.FrameHeight);
            VideoWriter writer = new VideoWriter(outputFile, FourCC.MJPG, fps, dsize, true);
            int k = 0;
            List<string> targetList = new List<string>();
            while (capture.Read(image)) {
                if (k % 1 == 0) {
                    double[] target;
                    Mat res = Track(image, out target);
                    writer.Write(res);
                    targetList.Add(k + " " + target[0] + " " + target[1]);
                }
                
                k++;

            };
            writer.Release();
            Console.WriteLine(k);
            IOTools.WriteListToTxt(targetList, targetFile);
        }
        public List<Rect> InnerRect(List<Rect> inL,List<Rect> outL) {
            List<Rect> result = new List<Rect>();
            foreach (Rect inr in inL) {
                foreach (Rect outr in outL) {
                    if (inr.Left > outr.Left && inr.Right < outr.Right && inr.Top > outr.Top && inr.Bottom < outr.Bottom)
                        result.Add(inr);
                    
                }
            }
            return result;
        }
        public List<Circle> InnerCircle(List<Circle> inC, List<Rect> outL)
        {
            List<Circle> result = new List<Circle>();
            foreach (Circle inc in inC)
            {
                foreach (Rect outr in outL)
                {
                    if (inc.X - inc.Radius > outr.Left && inc.X + inc.Radius < outr.Right && inc.Y - inc.Radius > outr.Top && inc.Y + inc.Radius < outr.Bottom)
                        result.Add(inc);

                }
            }
            return result;
        }
        public List<Rect> FindBounds(Mat mask) {
            List<Rect> result = new List<Rect>();
            Cv2.Erode(mask, mask, new Mat());
            Cv2.Dilate(mask, mask, new Mat());
            Point[][] contours; //vector<vector<Point>> contours;
            HierarchyIndex[] hierarchyIndexes; //vector<Vec4i> hierarchy;
            Cv2.FindContours(
                mask,
                out contours,
                out hierarchyIndexes,
                ContourRetrieval.External,
                ContourChain.ApproxSimple
                );
            var contourIndex = 0;

            while ((contourIndex >= 0) && contours.Length != 0)
            {
                var contour = contours[contourIndex];
                
                var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour
                result.Add(boundingRect);
                contourIndex = hierarchyIndexes[contourIndex].Next;
            }
            return result;
        }
        public List<Circle> FindCircleBounds(Mat mask)
        {
            List<Circle> result = new List<Circle>();
            Cv2.Erode(mask, mask, new Mat());
           // Cv2.Dilate(mask, mask, new Mat());
            Point[][] contours; //vector<vector<Point>> contours;
            HierarchyIndex[] hierarchyIndexes; //vector<Vec4i> hierarchy;
            Cv2.FindContours(
                mask,
                out contours,
                out hierarchyIndexes,
                ContourRetrieval.External,
                ContourChain.ApproxSimple
                );
            var contourIndex = 0;

            while ((contourIndex >= 0) && contours.Length != 0)
            {
                var contour = contours[contourIndex];
                Point2f center = new Point2f();
                float radius;
                Cv2.MinEnclosingCircle(contour,out center,out radius); //Find bounding rect for each contour
                result.Add(new Circle (center.X,center.Y,radius ));
                contourIndex = hierarchyIndexes[contourIndex].Next;
            }
            return result;
        }
        public List<Circle>[] FindNearbyCircles(List<Circle> circles1,List<Circle> circles2) {
            List<Circle>[] result = new List<Circle>[2];
            result[0] = new List<Circle>();
            result[1] = new List<Circle>();
            foreach (Circle c1 in circles1) {
                foreach (Circle c2 in circles2)
                {
                    if (Math.Pow(c1.X - c2.X, 2) + Math.Pow(c1.Y - c2.Y, 2) < Math.Pow(c1.Radius + c2.Radius, 2)*25// &&Math.Pow(c1.X - c2.X, 2) + Math.Pow(c1.Y - c2.Y, 2) > Math.Pow(c1.Radius + c2.Radius, 2) * 0.64
                        && c1.Radius > 0.4*c2.Radius && c2.Radius > 0.4 * c1.Radius)
                    {
                        result[0].Add(c1);
                        result[1].Add(c2);
                    }
                } }

            return result;
        }
        public Mat Track(Mat source,out double[] target) {
            Mat hsv = new Mat();
            Cv2.CvtColor(source, hsv, ColorConversion.BgrToHsv);
            List<Circle> redCircles = new List<Circle>();
            List<Circle> greenCircles = new List<Circle>();
            List<Rect> whiteRects = new List<Rect>();
            Mat mask = new Mat();
            Mat mask2 = new Mat();
           
             Cv2.InRange(hsv, new Scalar(0, 70, 50), new Scalar(10, 255, 255), mask);
             Cv2.InRange(hsv, new Scalar(170, 70, 50), new Scalar(180, 255, 255), mask2);
             mask = mask | mask2;
            redCircles = FindCircleBounds(mask);
            Cv2.InRange(hsv, new Scalar(0, 0, 150), new Scalar(180, 30, 255), mask);
            whiteRects = FindBounds(mask);
            Cv2.InRange(hsv, new Scalar(50, 43, 5), new Scalar(90, 255, 255), mask);//green
            //Cv2.InRange(hsv, new Scalar(60, 43, 46), new Scalar(90, 255, 255), mask);//green
                                                                                     //Cv2.InRange(hsv, new Scalar(26, 43, 46), new Scalar(34, 255, 255), mask);
                                                                                     // Cv2.InRange(hsv, new Scalar(78, 43, 46), new Scalar(124, 255, 255), mask);
            greenCircles = FindCircleBounds(mask);

           // redCircles = InnerCircle(redCircles, whiteRects);
            //yellowCircles = InnerCircle(yellowCircles, whiteRects);
            List<Circle>[] finalCircles = FindNearbyCircles(redCircles,greenCircles);
            Mat res = source;
            foreach (Circle boundingCircle in redCircles)
                Cv2.Circle(res,boundingCircle.X,boundingCircle.Y,boundingCircle.Radius, new Scalar(0, 0, 255),
                    2
                    );
            foreach (Circle boundingCircle in greenCircles)
                Cv2.Circle(res, boundingCircle.X, boundingCircle.Y, boundingCircle.Radius, new Scalar(0, 255, 0),
                    2
                    );

            /* foreach (Rect boundingRect in whiteRects) {
                 Cv2.Rectangle(res,
                     new Point(boundingRect.X, boundingRect.Y),
                     new Point(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height),
                     new Scalar(0, 0, 255),
                     2);
             }*/
           // Console.WriteLine(greenCircles.Count);
            target = new double[] { 10000, 10000 };
            if (finalCircles[0].Count == 1 && finalCircles[1].Count == 1)
            {
                target[0] = (finalCircles[0][0].X + finalCircles[1][0].X) / 2.0;
                target[1] = (finalCircles[0][0].Y + finalCircles[1][0].Y) / 2.0;

            }
            else {
                
            }

            Console.WriteLine(finalCircles[0].Count + " " + finalCircles[1].Count);

            return res;
        }
        
        public Mat TrackAll(Mat source)
        {
            Mat hsv = new Mat();
            Cv2.CvtColor(source, hsv, ColorConversion.BgrToHsv);
            Mat mask = new Mat();
            Mat mask2 = new Mat();
            // Cv2.InRange(hsv, new Scalar(35, 43, 46), new Scalar(77, 255, 255), mask);
            Cv2.InRange(hsv, new Scalar(0, 70, 50), new Scalar(10, 255, 255), mask);
            Cv2.InRange(hsv, new Scalar(170, 70, 50), new Scalar(180, 255, 255), mask2);
            mask = mask | mask2;
            Cv2.Erode(mask, mask, new Mat());
            Cv2.Dilate(mask, mask, new Mat());
            Mat res = source;
            // Cv2.BitwiseAnd(source, source, res, mask);

            Point[][] contours; //vector<vector<Point>> contours;
            HierarchyIndex[] hierarchyIndexes; //vector<Vec4i> hierarchy;
            Cv2.FindContours(
                mask,
                out contours,
                out hierarchyIndexes,
                ContourRetrieval.External,
                ContourChain.ApproxSimple
                );
            var contourIndex = 0;

            while ((contourIndex >= 0) && contours.Length != 0)
            {
                var contour = contours[contourIndex];

                var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour
                Cv2.Rectangle(res,
                    new Point(boundingRect.X, boundingRect.Y),
                    new Point(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height),
                    new Scalar(0, 0, 255),
                    2);
                contourIndex = hierarchyIndexes[contourIndex].Next;
            }


            Cv2.InRange(hsv, new Scalar(0, 0, 221), new Scalar(180, 30, 255), mask);
            //Cv2.InRange(hsv, new Scalar(35, 43, 46), new Scalar(77, 255, 255), mask);
            Cv2.Erode(mask, mask, new Mat());
            Cv2.Dilate(mask, mask, new Mat());
            Cv2.FindContours(
               mask,
               out contours,
               out hierarchyIndexes,
               ContourRetrieval.External,
               ContourChain.ApproxSimple
               );
            contourIndex = 0;

            while ((contourIndex >= 0) && contours.Length != 0)
            {
                var contour = contours[contourIndex];

                var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour
                Cv2.Rectangle(res,
                    new Point(boundingRect.X, boundingRect.Y),
                    new Point(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height),
                    new Scalar(0, 0, 255),
                    2);
                contourIndex = hierarchyIndexes[contourIndex].Next;
            }
            return res;
        }



    }
}
