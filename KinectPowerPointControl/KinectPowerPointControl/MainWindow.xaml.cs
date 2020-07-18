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
using System.Threading;
using System.IO;

namespace KinectPowerPointControl {
    public partial class MainWindow : Window {
        KinectSensor sensor;
        Skeleton[] skeletons;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //seleccionar el primer sensor 
            sensor = KinectSensor.KinectSensors.FirstOrDefault();

            //si no detecta sensor salir
            if (sensor == null) {
                MessageBox.Show("No hay sensor Kinect.");
                this.Close();
            }

            //iniciar el sensor 
            sensor.Start();

            //iniciar los strams que se van a usar 
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            sensor.SkeletonStream.Enable();

            //evento que corre cada que se actualiza un frame
            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);

            //establece la inclinacion del sensor
            sensor.ElevationAngle = 15;
        }

        //evento para cerrar el sensor
        private void Window_Closed(object sender, EventArgs e) {
            if (sensor != null) {
                sensor.AudioSource.Stop();
                sensor.Stop();
                sensor.Dispose();
                sensor = null;
            }
        }

        //corre cada que hay un nuevo frame
        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e) {
            //dibuja la camara rgb
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) {
                if (colorFrame == null)
                    return;

                byte[] colorData = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(colorData);

                kinectVideo.Source = BitmapSource.Create(colorFrame.Width,
                                                            colorFrame.Height,
                                                            96,
                                                            96,
                                                            PixelFormats.Bgr32,
                                                            null,
                                                            colorData,
                                                            colorFrame.Width * colorFrame.BytesPerPixel);

            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) {
                //salir si no hay frame
                if (skeletonFrame == null)
                    return;
                
                //si cambian los esqueletos actualizar la variable
                if (skeletons == null ||
                    skeletons.Length != skeletonFrame.SkeletonArrayLength) {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                }
                //copiar esqueletos a la matriz
                skeletonFrame.CopySkeletonDataTo(skeletons);

                // seleccionar el esqueleto mas cercano
                Skeleton closestSkeleton = (from s in skeletons
                                            where s.TrackingState == SkeletonTrackingState.Tracked &&
                                                  s.Joints[JointType.Head].TrackingState == JointTrackingState.Tracked
                                            select s).OrderBy(s => s.Joints[JointType.Head].Position.Z)
                                                    .FirstOrDefault();
                //si no hay esqueleto cercano salir
                if (closestSkeleton == null)
                    return;

                //asignar los joints a una variable para facil identificacion
                Joint head = closestSkeleton.Joints[JointType.Head];
                Joint rightHand = closestSkeleton.Joints[JointType.HandRight];
                Joint leftHand = closestSkeleton.Joints[JointType.HandLeft];

                //si no se detectan bien los 3 joints salir 
                if (head.TrackingState != JointTrackingState.Tracked ||
                    rightHand.TrackingState != JointTrackingState.Tracked ||
                    leftHand.TrackingState != JointTrackingState.Tracked) {
                    //Don't have a good read on the joints so we cannot process gestures
                    return;
                }

                //actualizar la posicion de las elipses 
                SetEllipsePosition(ellipseHead, head, false);
                SetEllipsePosition(ellipseLeftHand, leftHand, isBackGestureActive);
                SetEllipsePosition(ellipseRightHand, rightHand, isForwardGestureActive);

                //checar si se activa el gesto o no 
                ProcessForwardBackGesture(head, rightHand, leftHand);
            }
        }

        //banderas para evitar qe se activen 2 gestos al mismo tiempo
        bool isForwardGestureActive = false;
        bool isBackGestureActive = false;

        //verificar si se activa un gesto 
        private void ProcessForwardBackGesture(Joint head, Joint rightHand, Joint leftHand) {
            //si la mano derecha se aleja de la cabeza se activa el gesto derecho
            if (rightHand.Position.X > head.Position.X + 0.45) {
                if (!isBackGestureActive && !isForwardGestureActive) {
                    isForwardGestureActive = true;//actualizar la bandera
                    System.Windows.Forms.SendKeys.SendWait("{Right}");//enviar la tecla derecha a la aplicacion activa
                }
            } else {
                isForwardGestureActive = false;//actualizar la bandera
            }

            //si la mano izq se aleja de la cabeza se activa el gesto derecho
            if (leftHand.Position.X < head.Position.X - 0.45) {
                if (!isBackGestureActive && !isForwardGestureActive) {
                    isBackGestureActive = true;//actualizar la bandera
                    System.Windows.Forms.SendKeys.SendWait("{Left}");//enviar la tecla izq a la aplicacion activa
                }
            } else {
                isBackGestureActive = false;//actualizar la bandera
            }
        }


        SolidColorBrush activeBrush = new SolidColorBrush(Colors.Green);
        SolidColorBrush inactiveBrush = new SolidColorBrush(Colors.Red);

        private void SetEllipsePosition(Ellipse ellipse, Joint joint, bool isHighlighted) {
            var point = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, sensor.ColorStream.Format);

            if (isHighlighted) {
                ellipse.Width = 60;
                ellipse.Height = 60;
                ellipse.Fill = activeBrush;
            } else {
                ellipse.Width = 20;
                ellipse.Height = 20;
                ellipse.Fill = inactiveBrush;
            }

            Canvas.SetLeft(ellipse, point.X - ellipse.ActualWidth / 2);
            Canvas.SetTop(ellipse, point.Y - ellipse.ActualHeight / 2);
        }

    }
}
