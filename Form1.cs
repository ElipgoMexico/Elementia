using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Math;
using Accord.Statistics;
using System.Drawing.Imaging;


namespace Analitico2_Elementia
{
    public partial class Form1 : Form
    {
        private Mat ImagenOriginal;
        private VideoCapture videoCapture;
        private bool isVideoPlaying = false;

        readonly Point[] area_pts = [
            new(0, 536),
            new(0, 1108),
            new(2688, 1108),
            new(2688, 744)
        ];

        private ActivationNetwork perceptron;
        public double[][] inputs;
        public double[][] targets;

        public Form1()
        {
            InitializeComponent();
            InitializePerceptron();

            string[] positiveImagesPaths = Directory.GetFiles(@"C:\Ditran\Imagenes referencia\90 x 90 positivas binarizadas");
            string[] negativeImagesPaths = Directory.GetFiles(@"C:\Ditran\Imagenes referencia\90 x 90 negativas binarizadas");

            inputs = new double[positiveImagesPaths.Length + negativeImagesPaths.Length][]; //  datos de entrada
            targets = new double[positiveImagesPaths.Length + negativeImagesPaths.Length][]; //etiquetas objetivo

            CargarImagenes(inputs, targets, positiveImagesPaths, negativeImagesPaths);

            int epochs = 1000;
            double learningRate = 0.02;
            var teacher = new BackPropagationLearning(perceptron)
            {
                LearningRate = learningRate
            };

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double error = teacher.RunEpoch(inputs, targets);
                //   label2.Text = $"Epoch {epoch + 1}, Error: {error}";
            }

        }

        private void btn_cargarImagen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new()
            {
                Filter = "Archivos de imagen|*.jpeg;*.png;*.jpg",
                Title = "Seleccionar imagen"
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string path_image = openFileDialog1.FileName;

                    ImagenOriginal = CvInvoke.Imread(path_image, ImreadModes.Color);
                    DisplayImage(ImagenOriginal, pictureBox1);

                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    MessageBox.Show("Error al seleccionar la imagen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btn_analisis_Click(object sender, EventArgs e)
        {

            // Crea un nuevo contexto ML
            var mlContext = new MLContext();

            // Verifica si la imagen original ha sido cargada
            if (ImagenOriginal != null)
            {

                using Mat recorImage = new Mat();
                using Mat grayImage = new Mat();
                using Mat gaussianImage = new Mat();
                using Mat filterImage = new Mat();
                using Mat thresholdImage = new Mat();
                using Mat cannyEdges = new Mat();
                using VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

                //Convierte a escala de grises
                CvInvoke.CvtColor(ImagenOriginal, grayImage, ColorConversion.Bgr2Gray);

                // Crear y aplicar enmascaramiento de la zona principal de la toma
                Mat imAux = new(ImagenOriginal.Size, DepthType.Cv8U, 1);
                imAux.SetTo(new MCvScalar(0));
                VectorOfPoint contour = new(area_pts);
                CvInvoke.FillPoly(imAux, contour, new MCvScalar(255));

                // Aplicar la máscara al frame
                CvInvoke.BitwiseAnd(grayImage, grayImage, recorImage, imAux);

                //Aplicar ecualizacion de imagen
                CvInvoke.Normalize(recorImage, filterImage, 0, 200, NormType.MinMax);

                // Segmentación: Umbralización para resaltar áreas de interés
                CvInvoke.Threshold(filterImage, thresholdImage, 130, 200, ThresholdType.Binary);

                // Detección de bordes usando Canny
                CvInvoke.Canny(thresholdImage, cannyEdges, 100, 200);

                // Encontrar contornos de los rayones
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                // Lista para guardar los bounding boxes de los contornos que cumplen los criterios
                List<Rectangle> positiveBoundingBoxes = new List<Rectangle>();

                int contador = 0;
                for (int i = 0; i < contours.Size; i++)
                {
                    VectorOfPoint currentContour = contours[i];

                    // Calcular el rectángulo delimitador para el contorno
                    Rectangle boundingBox = CvInvoke.BoundingRectangle(currentContour);

                    // Determina el tamaño del área de interés (ROI) para la imagen segmentada
                    int size = 90;
                    Rectangle roi = new Rectangle(
                        boundingBox.X + boundingBox.Width / 2 - size / 2,
                        boundingBox.Y + boundingBox.Height / 2 - size / 2,
                        size,
                        size);

                    // Asegúrate de que el ROI no se salga de los límites de la imagen original
                    roi.Intersect(new Rectangle(Point.Empty, ImagenOriginal.Size));

                    // Ajustar ROI para que siempre sea de 90x90 y dentro de los límites
                    if (roi.Width != size || roi.Height != size)
                    {
                        roi.Width = Math.Min(size, ImagenOriginal.Width - roi.X);
                        roi.Height = Math.Min(size, ImagenOriginal.Height - roi.Y);
                    }

                    // Crea una nueva imagen para el contorno segmentado
                    Mat contourImg = new Mat(ImagenOriginal, roi);

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
                List<List<Rectangle>> groupedContours = GroupVerticallyAlignedContours(positiveBoundingBoxes, 0.1, 15);

                // Dibujar los contornos agrupados
                foreach (var group in groupedContours)
                {
                    if (group.Count >= 7)
                    {
                        Rectangle combinedBoundingBox = CombineBoundingBoxes(group);

                        // Dibujar el rectángulo delimitador en la imagen original
                        CvInvoke.Rectangle(ImagenOriginal, combinedBoundingBox, new MCvScalar(0, 255, 0), 2);

                        // Dibujar el resultado de la predicción en la imagen original
                        string label = "positivo";
                        CvInvoke.PutText(ImagenOriginal, label, new Point(combinedBoundingBox.X, combinedBoundingBox.Y - 10),
                            FontFace.HersheySimplex, 0.5, new MCvScalar(0, 255, 0), 2);
                    }
                }

                DisplayImage(ImagenOriginal, pictureBox2);
            }
            else
            {
                MessageBox.Show("Primero debe cargar una imagen.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void InitializePerceptron()
        {
            int inputSize = 90 * 90; // Tamaño de la imagen binarizada
            int hiddenSize = 5;    // Puede ajustarse según sea necesario
            int outputSize = 2;      // Número de clases a clasificar

            perceptron = new ActivationNetwork(new SigmoidFunction(), inputSize, hiddenSize, outputSize);
        }

        private void DisplayImage(Mat image, PictureBox pictureBox)
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
            using (Bitmap bitmap = imageToShow.ToBitmap())
            {
                // Asegurarse de limpiar la imagen anterior del PictureBox si existe
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                }

                // Mostrar la imagen en el PictureBox
                pictureBox.Image = bitmap.Clone() as Image;  // Clonar el bitmap para evitar problemas de acceso a recursos
            }
        }

        private void CargarImagenes(double[][] inputs, double[][] targets, string[] positiveImagesPaths, string[] negativeImagesPaths)
        {
            int index = 0;

            foreach (var path in positiveImagesPaths)
            {
                inputs[index] = LoadImage(path);
                targets[index] = new double[] { 1, 0 }; // Etiqueta positiva
                index++;
            }

            foreach (var path in negativeImagesPaths)
            {
                inputs[index] = LoadImage(path);
                targets[index] = new double[] { 0, 1 }; // Etiqueta negativa
                index++;
            }
        }

        private double[] LoadImage(string filePath)
        {
            Bitmap bmp = new Bitmap(filePath);
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

        private double[] ConvertImageToDoubleArray(Bitmap bmp)
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

        private void btn_cargarVideo_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "Archivos de video|*.avi;*.mp4;*.mov",
                Title = "Seleccionar video"
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Detener la captura de video actual, si está en progreso
                    if (videoCapture != null && isVideoPlaying)
                    {
                        videoCapture.Stop();
                    }

                    string path_video = openFileDialog1.FileName;
                    videoCapture = new VideoCapture(path_video);

                    // Iniciar el procesamiento de video
                    isVideoPlaying = true;
                    ProcessVideoFrames();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar el video: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ProcessVideoFrames()
        {
            Mat frame = new Mat();
            while (isVideoPlaying)
            {
                videoCapture.Read(frame);

                if (!frame.IsEmpty)
                {
                    DisplayImage(frame, pictureBox1);
                    using (Mat recorImage = new Mat())
                    using (Mat grayImage = new Mat())
                    using (Mat gaussianImage = new Mat())
                    using (Mat filterImage = new Mat())
                    using (Mat thresholdImage = new Mat())
                    using (Mat cannyEdges = new Mat())
                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                    {
                        // Convierte a escala de grises
                        CvInvoke.CvtColor(frame, grayImage, ColorConversion.Bgr2Gray);

                        // Aplicar la máscara de zona principal de la toma
                        Mat imAux = new Mat(frame.Size, DepthType.Cv8U, 1);
                        imAux.SetTo(new MCvScalar(0));
                        VectorOfPoint contour = new VectorOfPoint(area_pts);
                        CvInvoke.FillPoly(imAux, contour, new MCvScalar(255));

                        // Aplicar la máscara al fotograma
                        CvInvoke.BitwiseAnd(grayImage, grayImage, recorImage, imAux);

                        // Aplicar ecualización de histograma
                        CvInvoke.Normalize(recorImage, filterImage, 0, 200, NormType.MinMax);

                        // Segmentación: Umbralización para resaltar áreas de interés
                        CvInvoke.Threshold(filterImage, thresholdImage, 130, 200, ThresholdType.Binary);

                        // Detección de bordes usando Canny
                        CvInvoke.Canny(thresholdImage, cannyEdges, 100, 200);

                        // Encontrar contornos
                        CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                        // Lista para guardar los bounding boxes de los contornos que cumplen los criterios
                        List<Rectangle> positiveBoundingBoxes = new List<Rectangle>();

                        for (int i = 0; i < contours.Size; i++)
                        {
                            VectorOfPoint currentContour = contours[i];

                            // Calcular el rectángulo delimitador para el contorno
                            Rectangle boundingBox = CvInvoke.BoundingRectangle(currentContour);

                            // Determina el tamaño del área de interés (ROI) para la imagen segmentada
                            int size = 90;
                            Rectangle roi = new Rectangle(
                                boundingBox.X + boundingBox.Width / 2 - size / 2,
                                boundingBox.Y + boundingBox.Height / 2 - size / 2,
                                size,
                                size);

                            // Asegúrate de que el ROI no se salga de los límites del fotograma original
                            roi.Intersect(new Rectangle(Point.Empty, frame.Size));

                            // Ajustar ROI para que siempre sea de 60x60 y dentro de los límites
                            if (roi.Width != size || roi.Height != size)
                            {
                                roi.Width = roi.Height = size;

                                if (roi.X > frame.Width - size)
                                    roi.X = frame.Width - size;

                                if (roi.Y > frame.Height - size)
                                    roi.Y = frame.Height - size;

                                //roi.Width = Math.Min(size, frame.Width - roi.X);
                                //roi.Height = Math.Min(size, frame.Height - roi.Y);
                            }

                            // Crea una nueva imagen para el contorno segmentado
                            Mat contourImg = new Mat(frame, roi);

                            // Modelo de IA perceptrón
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

                        // Dibujar los contornos agrupados en el fotograma original
                        foreach (Rectangle roi in positiveBoundingBoxes)
                        {
                            CvInvoke.Rectangle(frame, roi, new MCvScalar(0, 255, 0), 2);
                            CvInvoke.PutText(frame, "positivo", new Point(roi.X, roi.Y - 10),
                                FontFace.HersheySimplex, 0.5, new MCvScalar(0, 255, 0), 2);
                        }

                        // Mostrar el fotograma procesado en el PictureBox
                        DisplayImage(frame, pictureBox2);
                    }
                }
                else
                {
                    isVideoPlaying = false;
                }
            }

            // Liberar recursos al finalizar la reproducción del video
            videoCapture.Dispose();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Asegurar que se liberen los recursos de video al cerrar la aplicación
            if (videoCapture != null)
            {
                videoCapture.Dispose();
            }
        }
    }

}


