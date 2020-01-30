using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FaceFinderDemo.Camera;
using FaceFinderDemo.FaceDetection;
using FaceFinderDemo.ImageProcessing;
using System.Media;

namespace FaceFinderDemo
{

    public partial class MainWindow : Window
    {
        FaceDetectorDevice faceDetection;
        MainWindowViewModel model;
        CameraDevice camera;
        ImageDevice image;

        public MainWindow()
        {

            model = new MainWindowViewModel();
            DataContext = model;
            faceDetection = new FaceDetectorDevice();
            camera = new CameraDevice();
            image = new ImageDevice();
            faceDetection.ImageAvailable += ImageAvailable;
            faceDetection.FaceDetectorStateChanged += FaceDetectorStateChanged;
            model.LastDetection = "None";

            InitializeComponent();
            SizeToContent = SizeToContent.WidthAndHeight;

            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            model.AvailableCameras = DeviceEnumerator.GetDeviceNames();
            SelectedCamera.SelectedIndex = 0;
        }

        void OnClosed(object sender, EventArgs eventArgs)
        {
            camera.StopCamera();
            image.StopSending();
        }

        void ImageAvailable(object sender, ImageAvailableEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (model.IsCapturing)
                {
                    DetectedImage.Source = EmguWpfHelper.ToBitmapSource(e.Image);    
                }
                else
                {
                    DetectedImage.Source = new BitmapImage(new Uri(@"/Resources/camera_image_placeholder.png", UriKind.Relative));
                }
            }));
        }

        SoundPlayer sf = new SoundPlayer(@"Resources\WOO.wav");
        SoundPlayer sk = new SoundPlayer(@"Resources\BOO.wav");

        int k = 0;
        int u = 0;
        void FaceDetectorStateChanged(object sender, FaceDetectionEventArgs e)
        {
            model.CurrentlyDetecting = e.Starting;
            

            if (!e.Starting)
            { 
                if (e.Faces.Count == 0 )
                {
                    k = 0;
                    u = 0;
                    sf.Stop();
                    sk.Stop();
                }

                
                else if (e.Faces.Count == 1 )
                {
                   
                    if ((e.Faces[0].FaceLocation.Bottom - e.Faces[0].FaceLocation.Y) >= 250)
                    {
                        if (u == 0)
                        {
                            u = 1;
                            sk.PlayLooping();
                        }
                    }

                    else
                    {

                        if (e.Faces[0].FaceLocation.X > model.IeX && (e.Faces[0].FaceLocation.X + e.Faces[0].FaceLocation.Width) < model.IqX && e.Faces[0].FaceLocation.Bottom < model.IoY)
                        {
                           
                            k = 0;
                            u = 0;
                            sf.Stop();
                            sk.Stop();
                            
                        }
                        else
                        {
                            if (k == 0)
                            {
                                k = 1;
                                sf.PlayLooping();

                            }
                        }
                    }
                }
            }  
        }

       

        void Cameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (camera.IsCapturing)
            {
                camera.StopCamera();
                camera.StartCamera(SelectedCamera.SelectedIndex);
            }
        }

        void StopCapturing_OnClick(object sender, RoutedEventArgs e)
        {
            model.IsCapturing = false;
            camera.StopCamera();
            image.StopSending();
            sf.Stop();
            sk.Stop();
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                Dispatcher.Invoke(new Action(() =>
                {
                    DetectedImage.Source = new BitmapImage(new Uri(@"/Resources/camera_image_placeholder.png", UriKind.Relative));
                }));
            });
        }

        void StartCapturing_OnClick(object sender, RoutedEventArgs e)
        {
            if (model.AvailableCameras.Count == 0)
            {
                System.Windows.MessageBox.Show("No camera available!");
                return;
            }

            if (SelectedCamera.SelectedIndex < 0)
            {
                System.Windows.MessageBox.Show("No camera selected!");
                return;
            }
            model.IsCapturing = true;
            faceDetection.DetectionMode = FaceDetectorDevice.DetectionModes.AllFrames;
            faceDetection.DrawDetection = true;
            faceDetection.AttachSource(camera);
            faceDetection.ResetDetections();
            camera.StartCamera(SelectedCamera.SelectedIndex);
        }

    }
}
