using InperStudio.Lib.Bean;
using InperStudio.Lib.Data.Model;
using MathNet.Numerics.LinearAlgebra;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Helper
{
    public class InperTrackingHelper
    {
        /// <summary>
        /// 基础轨迹识别
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static Tracking DetectNo(Mat frame)
        {
            Tracking ans = new Tracking(); // 最后返回的结果

            // BGR图像转灰度图
            Mat frameGray = new Mat();
            Cv2.CvtColor(frame, frameGray, ColorConversionCodes.BGR2GRAY);

            // gamma 增强
            Mat gammaImage = GammaCorrection(frameGray, 0.1);

            // 二值化灰度图像
            double threshValue = 0;
            double maxValue = 255;
            int thresholdType = (int)ThresholdTypes.BinaryInv + (int)ThresholdTypes.Otsu;
            Mat binaryImage = new Mat();
            Cv2.Threshold(gammaImage, binaryImage, threshValue, maxValue, (ThresholdTypes)thresholdType);

            // 形态学运算的核大小
            Mat kernel;
            if (frame.Cols > 1000 || frame.Rows > 1000)
            {
                kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(11, 11));
            }
            else
            {
                kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            }

            // 开运算
            Mat openedImage = new Mat();
            Cv2.MorphologyEx(binaryImage, openedImage, MorphTypes.Open, kernel);

            // 闭运算
            Mat closedImage = new Mat();
            Cv2.MorphologyEx(openedImage, closedImage, MorphTypes.Close, kernel);

            // 对形态学运算的二值化图像进行联通域计算
            Mat labelImage = new Mat();
            Mat stats = new Mat();
            Mat centroids = new Mat();
            int numLabels = Cv2.ConnectedComponentsWithStats(closedImage, labelImage, stats, centroids, PixelConnectivity.Connectivity8);

            // 遍历所有得连通域，保存满足条件的连通域的索引和其与老鼠面积差的绝对值
            List<KeyValuePair<int, int>> indexArea = new List<KeyValuePair<int, int>>();

            for (int i = 1; i < numLabels; i++)
            {
                int left = stats.At<int>(i, (int)ConnectedComponentsTypes.Left);
                int top = stats.At<int>(i, (int)ConnectedComponentsTypes.Top);
                int width = stats.At<int>(i, (int)ConnectedComponentsTypes.Width);
                int height = stats.At<int>(i, (int)ConnectedComponentsTypes.Height);
                int area = stats.At<int>(i, (int)ConnectedComponentsTypes.Area);

                int absArea = System.Math.Abs(area);
                indexArea.Add(new KeyValuePair<int, int>(i, absArea));
            }

            // 根据面积从大到小排序
            indexArea = indexArea.OrderByDescending(pair => pair.Value).ToList();
            if (indexArea.Count <= 0) { return null; }
            // 取连通域面积最大作为老鼠
            int targetLabel = indexArea.First().Key;
            int targetArea = indexArea.First().Value;

            int leftTarget = stats.At<int>(targetLabel, (int)ConnectedComponentsTypes.Left);
            int topTarget = stats.At<int>(targetLabel, (int)ConnectedComponentsTypes.Top);
            int widthTarget = stats.At<int>(targetLabel, (int)ConnectedComponentsTypes.Width);
            int heightTarget = stats.At<int>(targetLabel, (int)ConnectedComponentsTypes.Height);
            int areaTarget = stats.At<int>(targetLabel, (int)ConnectedComponentsTypes.Area);
            Point2d pt = centroids.At<Point2d>(targetLabel, 0);
            float c_x = (float)pt.X;
            float c_y = (float)pt.Y;

            ans.Left = leftTarget;
            ans.Top = topTarget;
            //ans.Add(leftTarget + widthTarget);
            //ans.Add(topTarget + heightTarget); 
            ans.Width = widthTarget;
            ans.Height = heightTarget;
            ans.CenterX = (int)c_x;
            ans.CenterY = (int)c_y;
            ans.CreateTime = DateTime.Now;

            return ans;
        }
        private static Mat GammaCorrection(Mat src, double gamma)
        {
            Mat lutMatrix = new Mat(1, 256, MatType.CV_8U);
            byte[] lut = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                lut[i] = (byte)(System.Math.Pow(i / 255.0, gamma) * 255.0);
            }

            lutMatrix.SetArray(lut); // 修复了SetArray的调用，只传递一个参数即可
            Mat result = new Mat();
            Cv2.LUT(src, lutMatrix, result);

            return result;
        }
    }
    /// <summary>
    /// 模型轨迹识别
    /// </summary>
    public class InperTrackingDnnHelper
    {
        const float INPUT_WIDTH = 640.0f;
        const float INPUT_HEIGHT = 640.0f;
        const float SCORE_THRESHOLD = 0.8f;
        const float NMS_THRESHOLD = 0.6f;
        const float CONFIDENCE_THRESHOLD = 0.8f;
        public struct Detection
        {
            public int class_id;
            public float confidence;
            public Rect box;
        }
        private static Net net;
        public static void LoadNet()
        {
            try
            {
                //string onnxPath = Path.Combine(System.Environment.CurrentDirectory, "ONNX", "best_yolo5m.onnx");
                string onnxPath = Path.Combine(System.Environment.CurrentDirectory, "ONNX", "best_yolo5s.onnx");
                net = CvDnn.ReadNetFromOnnx(onnxPath);
            }
            catch (Exception e)
            {
                InperLogExtentHelper.LogExtent(e, "InperTrackingHelper");
            }
        }
        private static Mat FormatYoloV5(Mat source)
        {
            var col = source.Width;
            var row = source.Height;
            var _max = Math.Max(col, row);
            Mat result = new Mat(_max, _max, MatType.CV_8UC3, Scalar.All(0));
            source.CopyTo(new Mat(result, new Rect(0, 0, col, row)));
            return result;
        }
        public static List<Tracking> Detect(Mat image)
        {
            if (net == null) { return null; }
            //List<string> classNames = new List<string>() { "Mouse" };
            List<Tracking> output = new List<Tracking>();
            var inputImage = FormatYoloV5(image);
            var blob = CvDnn.BlobFromImage(inputImage, 1.0 / 255.0, new Size(INPUT_WIDTH, INPUT_HEIGHT));
            net.SetInput(blob);

            //var outNames = net.GetUnconnectedOutLayersNames();
            var outputs = net.Forward();

            float x_factor = inputImage.Width / INPUT_WIDTH;
            float y_factor = inputImage.Height / INPUT_HEIGHT;

            //var dimensions = 85;
            var rows = 25200;

            var class_ids = new List<int>();
            var confidences = new List<float>();
            var boxes = new List<Rect>();

            if (outputs != null)
            {
                for (int i = 0; i < rows; ++i)
                {
                    float confidence = outputs.At<float>(0, i, 4);

                    if (confidence >= CONFIDENCE_THRESHOLD)
                    {
                        var classes_scores = outputs.At<float>(0, i, 5);

                        Mat scores = new Mat(1, 1, MatType.CV_32FC1, classes_scores);

                        Cv2.MinMaxLoc(scores, out _, out Point maxLoc);
                        if (scores.At<float>(0, maxLoc.X) > SCORE_THRESHOLD)
                        {
                            confidences.Add(confidence);
                            class_ids.Add(maxLoc.X);

                            float x = outputs.At<float>(0, i, 0);
                            float y = outputs.At<float>(0, i, 1);
                            float w = outputs.At<float>(0, i, 2);
                            float h = outputs.At<float>(0, i, 3);
                            int left = (int)((x - 0.5 * w) * x_factor);
                            int top = (int)((y - 0.5 * h) * y_factor);
                            int width = (int)(w * x_factor);
                            int height = (int)(h * y_factor);
                            boxes.Add(new Rect(left, top, width, height));
                        }
                    }
                }
            }

            CvDnn.NMSBoxes(boxes, confidences, SCORE_THRESHOLD, NMS_THRESHOLD, out int[] indices);

            foreach (var idx in indices)
            {
                Tracking result = new Tracking();
                //result.class_id = class_ids[idx];
                //result.confidence = confidences[idx];

                result.Top = boxes[idx].Top;
                result.Left = boxes[idx].Left;
                result.Width = boxes[idx].Width;
                result.Height = boxes[idx].Height;
                result.CenterX = result.Left + result.Width / 2;
                result.CenterY = result.Top + result.Height / 2;
                result.CreateTime = DateTime.Now;
                output.Add(result);
            }
            return output;
        }
    }
}
