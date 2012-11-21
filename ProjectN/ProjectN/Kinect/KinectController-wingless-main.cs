using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using Microsoft.Kinect;
using Kinect.Toolbox;
using Kinect.Toolbox.Gestures;


namespace ProjectN
{
    class KinectController
    {
        public static KinectSensor KS = null;
        public static SwipeGestureDetector SwipeGesture = new SwipeGestureDetector();
        public static AlgorithmicPostureDetector PostureDetector = new AlgorithmicPostureDetector();

        public static void InitializeKinectComponent()
        {
            try
            {
                KS = KinectSensor.KinectSensors[0];
                KS.DepthStream.Enable();
                KS.SkeletonStream.Enable();
            }
            catch (Exception)
            {
                MessageBox.Show("Kinect 장치를 찾을 수 없습니다.", "huh?", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }

        public static void ChangeKinectEventHandler(EventHandler<ColorImageFrameReadyEventArgs> ColorFrameReady,
            EventHandler<DepthImageFrameReadyEventArgs> DepthFrameReady,
            EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady,
            EventHandler<AllFramesReadyEventArgs> AllFrameReady)
        {
            /*
             * 
             * 키넥트의 이벤트 핸들러를 초기화하고 Page에서 만든 이벤트 핸들러를 삽입한다.
             * 
             * */
            KS.Stop();
            if (ColorFrameReady != null)
                KS.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorFrameReady);
            if (DepthFrameReady != null)
                KS.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthFrameReady);
            if (SkeletonFrameReady != null)
                KS.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            if (AllFrameReady != null)
                KS.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(AllFrameReady);
            KS.Start();
        }

        public static void ChangeGestureEventHandler(Action<string> GestureEvent)
        {
            SwipeGesture.OnGestureDetected += GestureEvent;
        }

        
    }
}
