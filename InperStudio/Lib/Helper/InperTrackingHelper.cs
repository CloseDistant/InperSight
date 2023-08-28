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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using OpenVinoSharp;

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
    /// <summary>
    /// opnevino 轨迹识别 intel  2023/8/1 
    /// </summary>
    public class InperTrackingOpenvinoHelper
    {
        //读取模型
        OpenVinoSharp.Core core = new OpenVinoSharp.Core();
        Model model;
        InferRequest inferRequest;
        readonly string classer_path = Path.Combine(System.Environment.CurrentDirectory, "ONNX", "label.txt");
        public void LoadModel()
        {
            //model = core.read_model(Path.Combine(System.Environment.CurrentDirectory, "ONNX", "yolo5", "yolov5s_int8.xml"));
            model = core.read_model(Path.Combine(System.Environment.CurrentDirectory, "ONNX", "Models", "best_model.xml"));
            CompiledModel compiled_model = core.compiled_model(model, "AUTO");
            inferRequest = compiled_model.create_infer_request();
        }
        public List<Tracking> Detect(Mat image)
        {
            int max_image_length = image.Cols > image.Rows ? image.Cols : image.Rows;
            Mat max_image = Mat.Zeros(new OpenCvSharp.Size(max_image_length, max_image_length), MatType.CV_8UC3);
            Rect roi = new Rect(0, 0, image.Cols, image.Rows);
            image.CopyTo(new Mat(max_image, roi));

            float x_scale = max_image.Cols / 640.0f;
            float y_scale = max_image.Rows / 640.0f;


            // -------- Step 6. Set up input --------
            Tensor input_tensor = inferRequest.get_input_tensor();
            Shape input_shape = input_tensor.get_shape();


            Mat input_mat = CvDnn.BlobFromImage(max_image, 1.0 / 255.0, new Size(input_shape[2], input_shape[3]), 0, true, false);


            float[] input_data = new float[input_shape[1] * input_shape[2] * input_shape[3]];
            Marshal.Copy(input_mat.Ptr(0), input_data, 0, input_data.Length);
            input_tensor.set_data<float>(input_data);
            // -------- Step 7. Do inference synchronously --------
            inferRequest.infer();

            Tensor output_tensor = inferRequest.get_output_tensor();
            int output_length = (int)output_tensor.get_size();
            float[] output_data = output_tensor.get_data<float>(output_length);

            Mat result_data = new Mat(25200, 6, MatType.CV_32F, output_data);

            // 存放结果list
            List<Rect> position_boxes = new List<Rect>();
            List<int> class_ids = new List<int>();
            List<float> confidences = new List<float>();

            // 预处理输出结果
            for (int i = 0; i < result_data.Rows; i++)
            {
                // 获取置信值
                float confidence = result_data.At<float>(i, 4);
                if (confidence > 0.6)
                {
                    Mat classes_scores = result_data.Row(i).ColRange(5, 6);//GetArray(i, 5, classes_scores);
                    //Console.WriteLine(classes_scores.Cols+" "+classes_scores.Rows);
                    OpenCvSharp.Point max_classId_point, min_classId_point;
                    double max_score, min_score;
                    // 获取一组数据中最大值及其位置
                    Cv2.MinMaxLoc(classes_scores, out min_score, out max_score,
                        out min_classId_point, out max_classId_point);
                    //Console.WriteLine("socre:"+ max_score + " " + min_score);
                    //Console.WriteLine("max_score_id:"+ max_classId_point);

                    if (max_score > 0.7)
                    {
                        float cx = result_data.At<float>(i, 0);
                        float cy = result_data.At<float>(i, 1);
                        float ow = result_data.At<float>(i, 2);
                        float oh = result_data.At<float>(i, 3);

                        int x = (int)((cx - 0.5 * ow) * x_scale);
                        int y = (int)((cy - 0.5 * oh) * y_scale);
                        int width = (int)(ow * x_scale);
                        int height = (int)(oh * y_scale);

                        Rect box = new Rect();
                        box.X = x;
                        box.Y = y;
                        box.Width = width;
                        box.Height = height;

                        position_boxes.Add(box);
                        confidences.Add(confidence);
                        class_ids.Add(max_classId_point.X);
                    }


                }
            }
            CvDnn.NMSBoxes(position_boxes, confidences, 0.6f, 0.5f, out int[] indexes);
            var output = new List<Tracking>();
            // 将识别结果绘制到图片上
            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                output.Add(new Tracking()
                {
                    Left = position_boxes[index].Left,
                    Top = position_boxes[index].Top,
                    CenterX = position_boxes[index].Left + position_boxes[index].Width / 2,
                    CenterY = position_boxes[index].Top + position_boxes[index].Height / 2,
                    CreateTime = DateTime.Now,
                    Width = position_boxes[index].Width,
                    Height = position_boxes[index].Height,
                    Max_score = confidences[index]
                });
            }

            //if (output_length != 0)
            //{

            //    ResultYolov5 _result = new ResultYolov5();
            //    // 读取本地模型类别信息
            //    _result.read_class_names(classer_path);
            //    // 图片加载缩放比例
            //    _result.factor[0] = (float)image.Cols / (float)640;
            //    _result.factor[1] = (float)image.Rows / (float)640;
            //    // 处理输出数据
            //    var result = _result.process_resule(image, output_data);
            //    return result;
            //}
            return output;
        }
    }
    class ResultYolov5
    {
        // 识别结果类型
        public string[] class_names;
        // 图片放缩比例
        public float[] factor = new float[2];

        /// <summary>
        /// 读取本地识别结果类型文件到内存
        /// </summary>
        /// <param name="path">文件路径</param>
        public void read_class_names(string path)
        {

            List<string> str = new List<string>();
            StreamReader sr = new StreamReader(path);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                str.Add(line);
            }

            class_names = str.ToArray();

        }
        /// <summary>
        /// 处理yolov5模型结果
        /// </summary>
        /// <param name="image">原图片</param>
        /// <param name="result">识别结果</param>
        /// <returns>处理后的图片</returns>
        public List<Tracking> process_resule(Mat image, float[] result)
        {
            List<Tracking> output = new List<Tracking>();

            Mat result_data = new Mat(25200, 6, MatType.CV_32F, result);

            // 存放结果list
            List<Rect> position_boxes = new List<Rect>();
            List<int> class_ids = new List<int>();
            List<float> confidences = new List<float>();
            // 预处理输出结果
            for (int i = 0; i < result_data.Rows; i++)
            {
                // 获取置信值
                float confidence = result_data.At<float>(i, 4);
                if (confidence < 0.8)
                {
                    continue;
                }

                Mat classes_scores = result_data.Row(i).ColRange(5, 6);//GetArray(i, 5, classes_scores);
                Point max_classId_point, min_classId_point;
                double max_score, min_score;
                // 获取一组数据中最大值及其位置
                Cv2.MinMaxLoc(classes_scores, out min_score, out max_score,
                    out min_classId_point, out max_classId_point);
                // 置信度 0～1之间
                // 获取识别框信息
                if (max_score > 0.8)
                {
                    float cx = result_data.At<float>(i, 0);
                    float cy = result_data.At<float>(i, 1);
                    float ow = result_data.At<float>(i, 2);
                    float oh = result_data.At<float>(i, 3);
                    int x = (int)((cx - 0.5 * ow) * factor[0]);
                    int y = (int)((cy - 0.5 * oh) * factor[1]);
                    int width = (int)(ow * factor[0]);
                    int height = (int)(oh * factor[1]);
                    Rect box = new Rect();
                    box.X = x;
                    box.Y = y;
                    box.Width = width;
                    box.Height = height;

                    position_boxes.Add(box);
                    class_ids.Add(max_classId_point.X);
                    confidences.Add((float)max_score);
                }
            }

            // NMS非极大值抑制
            int[] indexes = new int[position_boxes.Count];
            CvDnn.NMSBoxes(position_boxes, confidences, 0.7f, 0.5f, out indexes);
            // 将识别结果绘制到图片上
            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                output.Add(new Tracking()
                {
                    Left = position_boxes[index].Left,
                    Top = position_boxes[index].Top,
                    CenterX = position_boxes[index].Left + position_boxes[index].Width / 2,
                    CenterY = position_boxes[index].Top + position_boxes[index].Height / 2,
                    CreateTime = DateTime.Now,
                    Width = position_boxes[index].Width,
                    Height = position_boxes[index].Height,
                    Max_score = confidences[index]
                });
            }

            //Cv2.ImShow("C# + TensorRT + Yolov5 推理结果", result_image);
            //Cv2.WaitKey();
            result_data.Dispose();
            return output;

        }
    }
}
