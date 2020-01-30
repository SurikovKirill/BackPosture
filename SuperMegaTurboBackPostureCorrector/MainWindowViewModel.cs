using System.Collections.Generic;
using System.ComponentModel;

namespace FaceFinderDemo
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public List<string> AvailableCameras
        {
            get { return availableCameras; }
            set
            {
                availableCameras = value;
                OnPropertyChanged("AvailableCameras");
            }
        }

        public bool IsCapturing
        {
            get { return isCapturing; }
            set
            {
                isCapturing = value;
                OnPropertyChanged("IsCapturing");
            }
        }

        public bool CurrentlyDetecting
        {
            get { return currentlyDetecting; }
            set
            {
                currentlyDetecting = value;
                OnPropertyChanged("CurrentlyDetecting");
            }
        }

        public string LastDetection
        {
            get { return lastDetection; }
            set
            {
                lastDetection = value;
                OnPropertyChanged("LastDetection");
            }
        }

        public int IeX
        {
            get { return (int)eX; }
            set
            {
                eX = value;
                OnPropertyChanged("eX");
            }
        }

        public int IqX
        {
            get { return (int)qX; }
            set
            {
                qX = value;
                OnPropertyChanged("qX");
            }
        }

        public int IoY
        {
            get { return (int)oY; }
            set
            {
                oY = value;
                OnPropertyChanged("oY");
            }
        }

        List<string> availableCameras;
        bool isCapturing;
        bool currentlyDetecting;
        string lastDetection;
        int eX = 178;
        int qX = 488;
        int oY = 388;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
