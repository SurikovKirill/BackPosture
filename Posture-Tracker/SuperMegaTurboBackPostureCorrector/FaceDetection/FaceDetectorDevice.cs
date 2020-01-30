using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Timers;
using Emgu.CV;
using FaceFinderDemo.ImageProcessing;

namespace FaceFinderDemo.FaceDetection
{
    public class FaceDetectorDevice : ImageProcessor, IDisposable
    {

        public DetectionModes DetectionMode
        {
            get { return detectionMode; }
            set
            {
                detectionMode = value;
                DetectionPeriod = detectionPeriod;
            }
        }

        public TimeSpan DetectionPeriod
        {
            get { return detectionPeriod; }
            set
            {
                detectionPeriod = value;
                detectionNotifyTimer.Stop();
            }
        }

        public bool DrawDetection { get; set; }

        public bool DrawProbableAreas { get; set; }

        public EventHandler<FaceDetectionEventArgs> FaceDetectorStateChanged;

        public enum DetectionModes { Disabled, AllFrames}

        bool disposed = false;
        CascadeClassifier faceClassifier;
        CascadeClassifier eyeClassifier;
        CascadeClassifier mouthClassifier;
        CascadeClassifier noseClassifier;

        List<FaceFeatures> lastDetectedFaces = new List<FaceFeatures>();
        bool detectingInProgress;
        bool detectNextFrame;
        DetectionModes detectionMode;
        Timer detectionNotifyTimer;
        TimeSpan detectionPeriod;

        public FaceDetectorDevice()
        {
            CvInvoke.UseOpenCL = false;
            faceClassifier = new CascadeClassifier(@"Resources\haarcascades\frontalface_alt.xml");
            eyeClassifier = new CascadeClassifier(@"Resources\haarcascades\eye.xml");
            mouthClassifier = new CascadeClassifier(@"Resources\haarcascades\mouth.xml");
            noseClassifier = new CascadeClassifier(@"Resources\haarcascades\nose.xml");
            detectionNotifyTimer = new Timer();
            detectionNotifyTimer.Elapsed += DetectionNotifyTimerOnElapsed;
            DetectionMode = DetectionModes.Disabled;
            DetectionPeriod = TimeSpan.FromMilliseconds(500);
        }

        void DetectionNotifyTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            detectNextFrame = true;
        }

        public void ManualDetect()
        {
            detectNextFrame = true;
        }

        protected override void OnImageReceived(Mat image)
        {

            if (DetectionMode == DetectionModes.AllFrames)
            {
                DetectFaces(image);
            }
            else if (DetectionMode == DetectionModes.Disabled)
            {
                lastDetectedFaces.Clear();
            }

            if (DrawDetection)
            {
                foreach (FaceFeatures f in lastDetectedFaces)
                    f.DrawToImage(image, DrawProbableAreas);
            }
            OnImageAvailable(image);
        }

        private void DetectFaces(Mat image)
        {
            OnFaceDetectionStateChanged(true);
            detectingInProgress = true;
            List<FaceFeatures> detectedFaces = new List<FaceFeatures>();

            var watch = Stopwatch.StartNew();
            using (var grayImage = new UMat())
            {
                CvInvoke.CvtColor(image, grayImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.EqualizeHist(grayImage, grayImage);

                Rectangle[] facesDetected = faceClassifier.DetectMultiScale(grayImage, 1.1, 3, new Size(40, 40));

                foreach (Rectangle facerect in facesDetected)
                {
                    FaceFeatures face = new FaceFeatures(facerect, grayImage.Cols, grayImage.Rows);
                    using (var probableNoseRegion = new UMat(grayImage, face.ProbableNoseLocation))
                    {
                        face.AddNose(noseClassifier.DetectMultiScale(probableNoseRegion, 1.13, 3, new Size(10, 10)));
                    }

                    using (var probableEyesRegion = new UMat(grayImage, face.ProbableEyeLocation))
                    {
                        face.AddEyes(eyeClassifier.DetectMultiScale(probableEyesRegion, 1.13, 3, new Size(10, 10)));
                    }

                    using (var probableMouthRegion = new UMat(grayImage, face.ProbableMouthLocation))
                    {
                        face.AddMouth(mouthClassifier.DetectMultiScale(probableMouthRegion, 1.13, 3, new Size(10, 20)));
                    }

                    detectedFaces.Add(face);
                }
            }

            watch.Stop();
            lastDetectedFaces = detectedFaces;
            OnFaceDetectionStateChanged(false, detectedFaces, (int)watch.ElapsedMilliseconds);
            detectingInProgress = false;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            faceClassifier.Dispose();
            eyeClassifier.Dispose();
            mouthClassifier.Dispose();
            noseClassifier.Dispose();

            disposed = true;
        }

        private void OnFaceDetectionStateChanged(bool starting, List<FaceFeatures> faces = null, int detectionTime = 0)
        {
            EventHandler<FaceDetectionEventArgs> handler = FaceDetectorStateChanged;
            if (handler != null)
            {
                handler(this, new FaceDetectionEventArgs(starting, faces, detectionTime));
            }
        }

        public void ResetDetections()
        {
            lastDetectedFaces.Clear();
        }
    }
}