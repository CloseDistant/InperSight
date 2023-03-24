using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace InperStudio.Lib.Helper.ImageRecognition
{
    public class AutoFocusHelper
    {
        public static Bitmap WriteableBitmapToBitmap(WriteableBitmap wBitmap)
        {
            Bitmap bmp = new Bitmap(wBitmap.PixelWidth, wBitmap.PixelHeight);
            int rPixelBytes = wBitmap.PixelWidth * wBitmap.PixelHeight;   //字节数，计算方式是幅宽乘以高度像素
            //注意，像素格式根据实际情况
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, wBitmap.PixelWidth, wBitmap.PixelHeight), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            wBitmap.Lock();

            unsafe
            {
                Buffer.MemoryCopy(wBitmap.BackBuffer.ToPointer(), data.Scan0.ToPointer(), rPixelBytes, rPixelBytes);
            }
            //Buffer.MemoryCopy需要在.net 4.6版本或更高版本上才可以使用，.net4.5不存在该方法。
            wBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)wBitmap.Width, (int)wBitmap.Height));
            wBitmap.Unlock();
            bmp.UnlockBits(data);
            return bmp;
        }
        public static CircleSegment[] GetCirclesCenterPoint(Mat mat)
        {
            try
            {
                List<Point2f> points = new List<Point2f>();
                mat.ConvertTo(mat, MatType.CV_8UC1);

                Mat gus = new Mat();
                Cv2.GaussianBlur(mat, gus, new OpenCvSharp.Size(5, 5), 0);

                Mat _img = new Mat();
                Cv2.MedianBlur(gus, _img, 5);

                CircleSegment[] circles = Cv2.HoughCircles(_img, HoughModes.Gradient, 1.8, 100, 150);

                //Console.WriteLine(circles.Count());
                //for (int i = 0; i < circles.Count(); i++)
                //{
                //    //Cv2.Circle(cimg, (int)circles[i].Center.X, (int)circles[i].Center.Y, 3, Scalar.Red, 2, LineTypes.AntiAlias);
                //    points.Add(circles[i].Center);
                //}
                return circles;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, "AutoFocusHelper");
            }
            return default;
        }
    }

}
