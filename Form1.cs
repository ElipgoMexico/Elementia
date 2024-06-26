using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Math;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections.Concurrent;


namespace Analitico2_Elementia
{
    public partial class Form1 : Form
    {
        private Mat ImagenOriginal;
        private readonly Mat camara = new();
        private VideoCapture videoCapture;
        readonly VideoCapture streamVideo;

        readonly Point[] area_pts = [
            new(0, 536),
            new(0, 1108),
            new(2688, 1108),
            new(2688, 744)
        ];

        private readonly ActivationNetwork perceptron;
        public double[][] inputs;
        public double[][] targets;

        // API
        //string api_url = "http://vms-preprod.vmonitoring.com/Vmon5API/DAC/DACServer.ashx?objId=4021&objType=VIN&type=Manufacture";
        //readonly string api_url = "http://back-nacobre.vmonitoring.com/DAC/DACServer.ashx?objId=6&objType=VIN&type=Manufacture&awfId=1";

        // Stream Video
        readonly string username = "admin";
        readonly string password = "Elipgo$123";
        readonly string endpoint = "cam/realmonitor?channel=1&subtype=0";
        //string ip = "elipgomexico.ddns.net:10354";
        readonly string ip = "172.16.80.51:554";

        readonly string folderPath = @"C:\Ditran\ErroresManufactura";
        readonly string folderPathPpal = @"C:\Ditran";

        //variable para guardar imagen
        public int k = 0, j = 0;

        public Form1()
        {
            InitializeComponent();

            int inputSize = 90 * 90; // Tamaño de la imagen binarizada
            int hiddenSize = 5;    // Puede ajustarse según sea necesario
            int outputSize = 2;      // Número de clases a clasificar

            perceptron = new ActivationNetwork(new SigmoidFunction(), inputSize, hiddenSize, outputSize);

            InitializeTimer();
            InitializeTimer2();

            streamVideo = new VideoCapture($"rtsp://{username}:{password}@{ip}/{endpoint}");

            ImagenOriginal = new Mat();
            videoCapture = new VideoCapture();

            string[] positiveImagesPaths = Directory.GetFiles(@"C:\Ditran\Imagenes referencia\90 x 90 positivas binarizadas");
            string[] negativeImagesPaths = Directory.GetFiles(@"C:\Ditran\Imagenes referencia\90 x 90 negativas binarizadas");

            inputs = new double[positiveImagesPaths.Length + negativeImagesPaths.Length][]; //  datos de entrada
            targets = new double[positiveImagesPaths.Length + negativeImagesPaths.Length][]; //etiquetas objetivo

            CargarImagenes(inputs, targets, positiveImagesPaths, negativeImagesPaths);

            int epochs = 1000;
            double learningRate = 0.01;
            var teacher = new BackPropagationLearning(perceptron)
            {
                LearningRate = learningRate
            };

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                _ = teacher.RunEpoch(inputs, targets);
            }

            // verificar si existe la carpeta para almacenar la imagen
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            pictureBox1.BackColor = Color.Black;
            pictureBox2.BackColor = Color.Black;
        }


        /*private void InitializePerceptron()
        {
            int inputSize = 90 * 90; // Tamaño de la imagen binarizada
            int hiddenSize = 10;    // Puede ajustarse según sea necesario
            int outputSize = 2;      // Número de clases a clasificar

            perceptron = new ActivationNetwork(new SigmoidFunction(), inputSize, hiddenSize, outputSize);
        }*/
        private void InitializeTimer()
        {
            // Configuración del Timer
            timer1.Enabled = false;
            timer1.Tick += Timer1_Tick;
        }

        private void InitializeTimer2()
        {
            // Configuración del Timer
            timer2.Enabled = false;
            timer2.Tick += Timer2_Tick;

        }

        private void Analisis(Mat imagenAnalisis)
        {
            using Mat recorImage = new();
            using Mat grayImage = new();
            using Mat filterImage = new();
            using Mat thresholdImage = new();
            using Mat cannyEdges = new();
            using VectorOfVectorOfPoint contours = new();

            // Convierte a escala de grises
            CvInvoke.CvtColor(imagenAnalisis, grayImage, ColorConversion.Bgr2Gray);

            // Crear y aplicar enmascaramiento de la zona principal de la toma
            Mat imAux = new(imagenAnalisis.Size, DepthType.Cv8U, 1);
            imAux.SetTo(new MCvScalar(0));
            VectorOfPoint contour = new(area_pts);
            CvInvoke.FillPoly(imAux, contour, new MCvScalar(255));

            // Aplicar la máscara al frame
            CvInvoke.BitwiseAnd(grayImage, grayImage, recorImage, imAux);

            // Aplicar ecualización de imagen
            CvInvoke.Normalize(recorImage, filterImage, 0, 200, NormType.MinMax);

            // Segmentación: Umbralización para resaltar áreas de interés
            CvInvoke.Threshold(filterImage, thresholdImage, 130, 200, ThresholdType.Binary);

            // Detección de bordes usando Canny
            CvInvoke.Canny(thresholdImage, cannyEdges, 100, 200);

            // Encontrar contornos de los rayones
            CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            // Lista para guardar los bounding boxes de los contornos que cumplen los criterios
            ConcurrentBag<Rectangle> positiveBoundingBoxes = [];

            int contador = 0;

            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint currentContour = contours[i];

                // Calcular el rectángulo delimitador para el contorno
                Rectangle boundingBox = CvInvoke.BoundingRectangle(currentContour);

                // Determina el tamaño del área de interés (ROI) para la imagen segmentada
                int size = 90;
                Rectangle roi = new(
                    boundingBox.X + boundingBox.Width / 2 - size / 2,
                    boundingBox.Y + boundingBox.Height / 2 - size / 2,
                    size,
                    size);

                // Asegúrate de que el ROI no se salga de los límites de la imagen original
                roi.Intersect(new Rectangle(Point.Empty, imagenAnalisis.Size));

                // Ajustar ROI para que siempre sea de 60x60 y dentro de los límites
                if (roi.Width != size || roi.Height != size)
                {
                    roi.Width = roi.Height = size;

                    if (roi.X > imagenAnalisis.Width - size)
                        roi.X = imagenAnalisis.Width - size;

                    if (roi.Y > imagenAnalisis.Height - size)
                        roi.Y = imagenAnalisis.Height - size;

                    //roi.Width = Math.Min(size, frame.Width - roi.X);
                    //roi.Height = Math.Min(size, frame.Height - roi.Y);
                }

                // Crea una nueva imagen para el contorno segmentado
                Mat contourImg = new(imagenAnalisis, roi);

                // Guarda la imagen segmentada (aquí podrías añadir la ubicación si es necesario)
                string fileName = $"{contador++}.png";
                contourImg.Save(fileName);

                ///////////////////////// Modelo de IA perceptron
                // Aplicar el modelo de perceptrón al contorno segmentado
                // Convertir Mat a Bitmap
                Bitmap contourBitmap = contourImg.ToBitmap();

                // Aplicar el modelo de perceptrón al contorno segmentado
                double[] output = perceptron.Compute(ConvertImageToDoubleArray(contourBitmap));
                int salida = output.ToList().IndexOf(output.Max());

                if (salida == 1)
                {
                    positiveBoundingBoxes.Add(roi);
                }
            }

            // Agrupar contornos alineados
            List<List<Rectangle>> groupedContours = GroupVerticallyAlignedContours([.. positiveBoundingBoxes], 0.1, 17);

            // Dibujar los contornos agrupados
            Parallel.ForEach(groupedContours, group =>
            {
                if (group.Count >= 10)
                {
                    Rectangle combinedBoundingBox = CombineBoundingBoxes(group);

                    // Dibujar el rectángulo delimitador en la imagen original
                    CvInvoke.Rectangle(imagenAnalisis, combinedBoundingBox, new MCvScalar(0, 255, 0), 2);

                    // Dibujar el resultado de la predicción en la imagen original
                    string label = "positivo";
                    CvInvoke.PutText(imagenAnalisis, label, new Point(combinedBoundingBox.X, combinedBoundingBox.Y - 10),
                        FontFace.HersheySimplex, 0.5, new MCvScalar(0, 255, 0), 2);
                }
            });

            DisplayImage(imagenAnalisis, pictureBox2);

            string image_path = Path.Combine(folderPath, $"{j}.jpg");
            j++;
            CvInvoke.Imwrite(image_path, imagenAnalisis);
        }


        private static void DisplayImage(Mat image, PictureBox pictureBox)
        {
            Size desiredSize = new(1200, 800);
            Mat resizedImage = new();

            // Solo redimensiona si es necesario
            if (image.Width > desiredSize.Width || image.Height > desiredSize.Height)
            {
                CvInvoke.Resize(image, resizedImage, desiredSize);
            }
            else
            {
                resizedImage = image.Clone();
            }

            // Convierte la imagen Mat a un objeto Image para mostrarla en el PictureBox
            using Image<Bgr, byte> imageToShow = resizedImage.ToImage<Bgr, byte>();
            using Bitmap bitmap = imageToShow.ToBitmap();
            // Asegurarse de limpiar la imagen anterior del PictureBox si existe
            pictureBox.Image?.Dispose();

            // Mostrar la imagen en el PictureBox
            pictureBox.Image = bitmap.Clone() as Image;  // Clonar el bitmap para evitar problemas de acceso a recursos
        }

        public static void CargarImagenes(double[][] inputs, double[][] targets, string[] positiveImagesPaths, string[] negativeImagesPaths)
        {
            // Etiquetas para imágenes positivas y negativas
            double[] positiveLabel = [0, 1]; // [0, 1]
            double[] negativeLabel = [1, 0]; // [1, 0]

            // Cargar imágenes positivas
            Parallel.For(0, positiveImagesPaths.Length, i =>
            {
                Bitmap bitmap = new(positiveImagesPaths[i]);
                inputs[i] = ConvertImageToDoubleArray(bitmap);
                targets[i] = positiveLabel;
            });

            // Cargar imágenes negativas
            Parallel.For(0, negativeImagesPaths.Length, i =>
            {
                Bitmap bitmap = new(negativeImagesPaths[i]);
                inputs[i + positiveImagesPaths.Length] = ConvertImageToDoubleArray(bitmap);
                targets[i + positiveImagesPaths.Length] = negativeLabel;
            });
        }

        private static double[] ConvertImageToDoubleArray(Bitmap bmp)
        {
            double[] result = new double[bmp.Width * bmp.Height];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color color = bmp.GetPixel(x, y);
                    result[y * bmp.Width + x] = color.GetBrightness(); // Convertir a escala de grises
                }
            }

            return result;
        }

        private List<List<Rectangle>> GroupVerticallyAlignedContours(List<Rectangle> boundingBoxes, double tolerance, int minCount)
        {
            List<List<Rectangle>> groups = new List<List<Rectangle>>();

            for (int i = 0; i < boundingBoxes.Count; i++)
            {
                Rectangle rect1 = boundingBoxes[i];
                List<Rectangle> group = new List<Rectangle> { rect1 };

                for (int j = i + 1; j < boundingBoxes.Count; j++)
                {
                    Rectangle rect2 = boundingBoxes[j];

                    // Verifica si los contornos están alineados verticalmente dentro de un margen del 2%
                    if (Math.Abs(rect1.X - rect2.X) <= rect1.Width * tolerance)
                    {
                        group.Add(rect2);
                    }
                }

                // Solo añadir el grupo si tiene al menos minCount contornos
                if (group.Count >= minCount)
                {
                    groups.Add(group);
                }
            }

            return groups;
        }

        private Rectangle CombineBoundingBoxes(List<Rectangle> boundingBoxes)
        {
            int x = boundingBoxes.Min(r => r.X);
            int y = boundingBoxes.Min(r => r.Y);
            int width = boundingBoxes.Max(r => r.Right) - x;
            int height = boundingBoxes.Max(r => r.Bottom) - y;

            return new Rectangle(x, y, width, height);
        }

        private async void Btn_anaVideo_Click(object sender, EventArgs e)
        {
            Pause = true;
            if (streamVideo == null)
            {
                return;
            }

            try
            {
                while (Pause)
                {
                    streamVideo.Read(camara);
                    DisplayImage(camara, pictureBox1);
                    string imagePath = Path.Combine(folderPathPpal, $"{k}.jpg");
                    k++;
                    CvInvoke.Imwrite(imagePath, camara);
                    Analisis(camara);

                    double fps = streamVideo.Get(Emgu.CV.CvEnum.CapProp.Fps);
                    await Task.Delay(1000 / Convert.ToInt32(fps));
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error al reproducir video: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Asegurar que se liberen los recursos de video al cerrar la aplicación
            videoCapture?.Dispose();
            streamVideo?.Dispose();
        }

        private void Btn_slowVideo_Click(object sender, EventArgs e)
        {
            Mat video = new();

            OpenFileDialog openFileDialog = new()
            {
                Filter = "Archivos de video|*.mp4;*.avi;*.mkv",
                Title = "Seleccionar video"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string videoPath = openFileDialog.FileName;
                videoCapture = new VideoCapture(videoPath);

                timer2.Enabled = false;
                timer1.Enabled = true;
                videoCapture.Read(video);

                if (!video.IsEmpty)
                {
                    timer1.Interval = 500;
                    timer1.Start();

                }

            }
        }

        private void Timer1_Tick(object? sender, EventArgs e)
        {
            Mat video = new();

            if (videoCapture != null && videoCapture.IsOpened)
            {
                videoCapture.Read(video);

                if (video.IsEmpty)
                {
                    timer1.Stop();
                    videoCapture.Release();
                    return;
                }

                DisplayImage(video, pictureBox1);
                Analisis(video);

            }
        }

        public bool Pause = false;
        private void Timer2_Tick(object? sender, EventArgs e)
        {
            try
            {
                ImagenOriginal = streamVideo.QueryFrame();
                DisplayImage(ImagenOriginal, pictureBox1);

                string imagePath = Path.Combine(folderPathPpal, $"{k}.jpg");
                k++;
                CvInvoke.Imwrite(imagePath, ImagenOriginal);


                Analisis(ImagenOriginal);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private async void Btn_OpenCamara_Click(object sender, EventArgs e)
        {
            Pause = true;
            if (streamVideo == null)
            {
                return;
            }

            try
            {
                while (Pause)
                {
                    streamVideo.Read(camara);
                    DisplayImage(camara, pictureBox1);
                    double fps = streamVideo.Get(Emgu.CV.CvEnum.CapProp.Fps);
                    await Task.Delay(1000 / Convert.ToInt32(fps));
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error al reproducir video: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void Btn_stop_Click(object sender, EventArgs e)
        {
            Pause = false;

            timer2.Stop();
            timer1.Stop();
            streamVideo.Pause();
            pictureBox1.Image = null;
            pictureBox2.Image = null;
        }

    }

}


