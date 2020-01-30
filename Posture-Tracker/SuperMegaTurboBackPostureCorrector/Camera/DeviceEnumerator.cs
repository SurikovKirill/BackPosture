using System.Collections.Generic;
using AForge.Video.DirectShow;

namespace FaceFinderDemo.Camera
{
    public static class DeviceEnumerator
    {
        public static List<string> GetDeviceNames()
        {
            var devices = new List<string>();
            FilterInfoCollection videoDevices = new FilterInfoCollection(
                        FilterCategory.VideoInputDevice);
            for (int i = 0; i != videoDevices.Count; i++)
            {
                var dev = videoDevices[i];
                devices.Add(dev.Name);
            }
            devices.Reverse();
            return devices;
        }
    }
}
