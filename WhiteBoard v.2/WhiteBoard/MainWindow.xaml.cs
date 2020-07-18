using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Speech.Recognition;
using System.Speech;
using System.IO;

namespace WhiteBoard
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinect;
        Skeleton[] skeletons;
        SpeechRecognitionEngine motor = new SpeechRecognitionEngine();
        SolidColorBrush colorP = new SolidColorBrush(Color.FromRgb(0,0,0));
       //Image cursor = new Image();
        Image marcador = new Image();
        Image borrador = new Image();
        Image hand = new Image();
        Boolean isErasing = false;
        CanvasSaver cSaver = new CanvasSaver();
        KinectCursor kCursor = new KinectCursor();
        public MainWindow()
        {
            InitializeComponent();
            motor.SetInputToDefaultAudioDevice();
            motor.LoadGrammar(new DictationGrammar());
            motor.SpeechRecognized += Reconocimiento_de_Voz;
            motor.RecognizeAsync(RecognizeMode.Multiple);
            //Carga la imagen del marcador
            BitmapImage srcPintarron = new BitmapImage();
            srcPintarron.BeginInit();
            srcPintarron.UriSource = new Uri("images/hand_painting.png", UriKind.RelativeOrAbsolute);
            srcPintarron.EndInit();
            marcador.Source = srcPintarron;
            marcador.Width = 50;
            marcador.Height = 50;
            hand = marcador;
            //Carga la imagen del Borrador
            BitmapImage srcBorrador = new BitmapImage();
            srcBorrador.BeginInit();
            srcBorrador.UriSource = new Uri("images/borrador.gif", UriKind.RelativeOrAbsolute);
            srcBorrador.EndInit();
            borrador.Source = srcBorrador;
            borrador.Width = 70;
            borrador.Height = 50;
        }
        public void Reconocimiento_de_Voz(object sender, SpeechRecognizedEventArgs voz)
        {
            try
            {
                foreach (RecognizedWordUnit palabra in voz.Result.Words)
                {
                    lblVoz.Content = palabra.Text;
                    switch (palabra.Text)
                    {
                        case "salir":
                            Environment.Exit(0);
                            break;
                        case "limpiar":
                            lienzo.Children.Clear();
                            break;
                        case "borrar":
                            isErasing = true;
                            break;
                        case "pintar":
                            isErasing = false;
                            break;
                        
                    case "guardar":
                            
                        Microsoft.Win32.SaveFileDialog saveD = new Microsoft.Win32.SaveFileDialog();
                        saveD.Filter = "Imágenes (.png)|*.png";
                        saveD.AddExtension = true;
                        saveD.DefaultExt = ".png";
                        saveD.Title = "Guardar imagen";
                        saveD.ShowDialog();
                        if (saveD.ShowDialog() == true)
                        {
                            cSaver.SaveUsingEncoder(lienzo, saveD.FileName);
                        }
                           
                        break;
                         
                        //Opciones para los colores
                        case "negro":
                            colorP = new SolidColorBrush(Color.FromRgb(0,0,0));
                            lblColor.Content="Negro";
                            break;
                        case "rojo":
                            colorP = new SolidColorBrush(Color.FromRgb(255,0,0));
                            lblColor.Content="Rojo";
                            break;
                        case "limón":
                            colorP = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                            lblColor.Content="Verde";
                            break;
                        case "azul":
                            colorP = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                            lblColor.Content="Azul";
                            break;
                        case "morado":
                            colorP = new SolidColorBrush(Color.FromRgb(120, 0, 255));
                            lblColor.Content = "Morado";
                            break;
                        case "amarillo":
                            colorP = new SolidColorBrush(Color.FromRgb(248, 255, 0));
                            lblColor.Content = "Amarillo";
                            break;
                        case "café":
                            colorP = new SolidColorBrush(Color.FromRgb(165, 42, 42));
                            lblColor.Content = "Café";
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinect = KinectSensor.KinectSensors.FirstOrDefault();
            if (kinect == null)
            {
                MessageBox.Show("No se detectó ningun sensor,\nverifique que el Kinect esté conectado correctamente y vuelva a intentarlo");
                Environment.Exit(0);
            }
                
            try
            {
                kinect.Start();
            }
            catch (Exception initK)
            {
                MessageBox.Show(initK.Message);
            }

            kinect.SkeletonStream.Enable();
            kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);
            kinect.ElevationAngle = 10;
            
        }

        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            aux.Children.Clear();//Limpia el canvas donde se encuentra el icono de la mano
                
                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame == null)
                        return;

                    if (skeletons == null || skeletons.Length != skeletonFrame.SkeletonArrayLength)
                        skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];

                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    Skeleton closestSk = (from s in skeletons
                                          where s.TrackingState == SkeletonTrackingState.Tracked &&
                                          s.Joints[JointType.Head].TrackingState == JointTrackingState.Tracked
                                          select s).OrderBy(s => s.Joints[JointType.Head].Position.Z).FirstOrDefault();

                    if (closestSk == null)
                        return;

                    dibujaMarcador(closestSk.Joints[JointType.HandRight], closestSk.Joints[JointType.HandLeft], closestSk.Joints[JointType.Head]);

                }
            
        }

        public void dibujaMarcador(Joint rightHand, Joint leftHand, Joint head)
        {
            
            if (rightHand.TrackingState != JointTrackingState.Tracked ||
                head.TrackingState != JointTrackingState.Tracked ||
                leftHand.TrackingState != JointTrackingState.Tracked)
                return;
            SkeletonPoint headPos = head.Position;
            SkeletonPoint rightHandPos = rightHand.Position;
            SkeletonPoint leftHandPos = rightHand.Position;
            /**Comprueba si el estado es pintando o borrando para establecer el color del "cursor"*/
            if (leftHand.Position.X > (headPos.X - 0.45))
            {
                isErasing = false;
                hand = marcador;
            }
            else
            {
                isErasing = true;
                hand = borrador;
            }

           var cursorP = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(rightHand.Position, kinect.ColorStream.Format);    //Obtiene la posición de la mano
          
            aux.Children.Add(hand); //Agrega el icono de la mano (marcador o borrador) al canvas
            Canvas.SetLeft(hand, cursorP.X - hand.ActualWidth / 2);   //Establece la posición del icono de la mano en el eje X
            Canvas.SetTop(hand, cursorP.Y - hand.ActualHeight / 2);   //Establece la posición del icono de la mano en el eje X
            
            kCursor.MoveCursor(Convert.ToInt32( cursorP.X - hand.ActualWidth / 2),Convert.ToInt32( cursorP.Y - hand.ActualWidth / 2));
            if (rightHandPos.Z < (headPos.Z-0.35))
            {
                Ellipse tool = new Ellipse();
                if (isErasing)
                    tool.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                else
                    tool.Fill = colorP;

                tool.Width = 30;
                tool.Height = 30;
                lienzo.Children.Add(tool);  //Añade el elipse al canvas
                var point = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(rightHand.Position, kinect.ColorStream.Format);    //Obtiene la posición de la mano
                
                Canvas.SetLeft(tool, point.X - tool.ActualWidth / 2);   //Establece la posición del marcador en el eje X
                Canvas.SetTop(tool, point.Y - tool.ActualHeight / 2);   //Establece la posición del marcador en el eje Y
            }
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (kinect != null)
            {
                kinect.Stop();
                kinect.Dispose();
                kinect = null;
            }
        }

        public void guardarImagen(Canvas canvas,String nombre)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                                (int)canvas.Width, (int)canvas.Height,
                                96d, 96d, PixelFormats.Pbgra32);
            canvas.Measure(new Size((int)canvas.Width, (int)canvas.Height));
            canvas.Arrange(new Rect(new Size((int)canvas.Width, (int)canvas.Height)));
            renderBitmap.Render(canvas);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using (FileStream file = File.Create(nombre))
            {
                encoder.Save(file);
            }
        }
        
    }
}
