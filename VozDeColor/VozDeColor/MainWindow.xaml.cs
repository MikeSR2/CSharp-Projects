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
using Microsoft.Speech;
using Microsoft.Speech.Recognition;
using System.Threading;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;

namespace VozDeColor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinect;
        SpeechRecognitionEngine motor;
        public MainWindow()
        {
            InitializeComponent();

            

           
        }

        void _Recognition_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            MessageBox.Show("nada");
            
                    BitmapImage src = new BitmapImage();

                    switch (e.Result.Semantics.Value.ToString())
                    {
                        case "CERRAR":
                            Environment.Exit(1);
                            break;
                        case "amarillo":
                            src.BeginInit();
                            src.UriSource = new Uri("amarillo.jpg", UriKind.Relative);
                            src.EndInit();
                            image1.Source = src;
                            break;

                        case "azul":
                            src.BeginInit();
                            src.UriSource = new Uri("azul.jpg", UriKind.Relative);
                            src.EndInit();
                            image1.Source = src;
                            break;

                        case "blanco":

                            src.BeginInit();
                            src.UriSource = new Uri("blanco.jpg", UriKind.Relative);
                            src.EndInit();
                            image1.Source = src;
                            break;

                        case "negro":
                            src.BeginInit();
                            src.UriSource = new Uri("negro.jpg", UriKind.Relative);
                            src.EndInit();
                            image1.Source = src;
                            break;

                        case "rojo":

                            src.BeginInit();
                            src.UriSource = new Uri("rojo.jpg", UriKind.Relative);
                            src.EndInit();
                            image1.Source = src;
                            break;
                        case "rosa":

                            src.BeginInit();
                            src.UriSource = new Uri("rosa.jpg", UriKind.Relative);
                            src.EndInit();
                            image1.Source = src;
                            break;
                        case "verde":

                            src.BeginInit();
                            src.UriSource = new Uri("verde.jpg", UriKind.Relative);
                            src.EndInit();
                            image1.Source = src;
                            break;
                        default:
                            break;

                    }
                  
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinect = KinectSensor.KinectSensors.FirstOrDefault();
            kinect.Start();
            kinect.AudioSource.Start();
            RecognizerInfo ri = GetKinectRecognizer();
            motor = new SpeechRecognitionEngine(ri.Id);
            var directions = new Choices();
            directions.Add(new SemanticResultValue("cerrar", "CERRAR"));
            directions.Add(new SemanticResultValue("forwards", "FORWARD"));
            directions.Add(new SemanticResultValue("straight", "FORWARD"));
            directions.Add(new SemanticResultValue("backward", "BACKWARD"));
            directions.Add(new SemanticResultValue("backwards", "BACKWARD"));
            directions.Add(new SemanticResultValue("back", "BACKWARD"));
            directions.Add(new SemanticResultValue("turn left", "LEFT"));
            directions.Add(new SemanticResultValue("turn right", "RIGHT"));

            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(directions);

            var g = new Grammar(gb);
            motor.LoadGrammar(g);
            motor.SpeechRecognized += _Recognition_SpeechRecognized;
            motor.SetInputToAudioStream(
                    kinect.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            motor.RecognizeAsync(RecognizeMode.Multiple);
        }
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en_US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

       
    }        
}
    

