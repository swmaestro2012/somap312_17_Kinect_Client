using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Windows;

using Microsoft.Kinect;
using Kinect.Toolbox;
using Kinect.Toolbox.Gestures;


namespace ProjectN
{
    class KinectController
    {
        public static KinectSensor KS = null;
        public static SwipeGestureDetector LeftHandSwipeGesture = new SwipeGestureDetector(10);
        public static SwipeGestureDetector RightHandSwipeGesture = new SwipeGestureDetector(10);
        
        public static AlgorithmicPostureDetector Posture = new AlgorithmicPostureDetector();
        public static Boolean KinectMouseEnabled = false;

        private static EventHandler<ColorImageFrameReadyEventArgs> CurrentColorImageFrameEvent;
        private static EventHandler<DepthImageFrameReadyEventArgs> CurrentDepthFrameEvent;
        private static EventHandler<SkeletonFrameReadyEventArgs> CurrentSkeletonFrameEvent;
        private static EventHandler<AllFramesReadyEventArgs> CurrentAllFrameEvent;

        private static Action<string> _PostureAction;

        private static Action<string> _J_RightHandSwipe;
        private static Action<string> _J_LeftHandSwipe;

        private const float ClickHoldingRectThreshold = 0.05f;
        private Rect _clickHoldingLastRect;
        private readonly Stopwatch _clickHoldingTimer;

        private const float SkeletonMaxX = 0.60f;
        private const float SkeletonMaxY = 0.40f;
        
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

        public static void StopKinect()
        {
            KS.Stop();
        }

        public static void ChangeKinectEventHandler(EventHandler<ColorImageFrameReadyEventArgs> ColorFrameReady,
            EventHandler<DepthImageFrameReadyEventArgs> DepthFrameReady,
            EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady,
            EventHandler<AllFramesReadyEventArgs> AllFrameReady)
        {
            /*
             * 
             * 키넥트의 이벤트 핸들러를 초기화하고 Page에서 만든 이벤트 핸들러를 변경한다.
             * 
             * */
            KS.Stop();
            if (ColorFrameReady != null)
            {
                if (CurrentColorImageFrameEvent != null)
                    KS.ColorFrameReady -= CurrentColorImageFrameEvent;
                CurrentColorImageFrameEvent = ColorFrameReady;
                
                KS.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorFrameReady);
            }
            if (DepthFrameReady != null)
            {
                if (CurrentDepthFrameEvent != null)
                    KS.DepthFrameReady -= CurrentDepthFrameEvent;
                CurrentDepthFrameEvent = DepthFrameReady;

                KS.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthFrameReady);
            }
            if (SkeletonFrameReady != null)
            {
                if (CurrentSkeletonFrameEvent != null)
                    KS.SkeletonFrameReady -= CurrentSkeletonFrameEvent;
                CurrentSkeletonFrameEvent = SkeletonFrameReady;
                
                KS.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            }
            if (AllFrameReady != null)
            {
                if(CurrentAllFrameEvent != null)
                    KS.AllFramesReady -= CurrentAllFrameEvent;
                CurrentAllFrameEvent = AllFrameReady;
                
                KS.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(AllFrameReady);
            }
            KS.Start();
        }

        public static void RemoveGestureEventHandler()
        {
            Action<string> GestureEventHandler = GestureAction;
            LeftHandSwipeGesture.OnGestureDetected -= _J_LeftHandSwipe;
            RightHandSwipeGesture.OnGestureDetected -= _J_RightHandSwipe;
            _J_LeftHandSwipe = null;
            _J_RightHandSwipe = null;
            LeftHandSwipeGesture.OnGestureDetected += GestureEventHandler;
            RightHandSwipeGesture.OnGestureDetected += GestureEventHandler;
        }

        private static void GestureAction(string gesture)
        {
            /*
             * 
             *  Gesture Action
             *   - SwipeToLeft
             *   - SwipeToRight
             *   
             * */
            switch (gesture)
            {
                case "SwipeToLeft": break;
                case "SwipeToRight": break;
            }
        }

        public static void ChangeGestureEventHandler(Action<string> GestureEvent, string HandType)
        {
            /*
             * 
             *  Gesture에 대한 이벤트 핸들러를 변경한다.
             * 
             * */
            if (HandType == "L")
            {
                if (_J_LeftHandSwipe != null)
                    LeftHandSwipeGesture.OnGestureDetected -= _J_LeftHandSwipe;
                _J_LeftHandSwipe = GestureEvent;
                LeftHandSwipeGesture.OnGestureDetected += GestureEvent;
            }
            else if (HandType == "R")
            {
                if (_J_RightHandSwipe != null)
                    RightHandSwipeGesture.OnGestureDetected -= _J_RightHandSwipe;
                _J_RightHandSwipe = GestureEvent;
                RightHandSwipeGesture.OnGestureDetected += GestureEvent;
            }
            else
            {
                if (_J_RightHandSwipe != null)
                    RightHandSwipeGesture.OnGestureDetected -= _J_RightHandSwipe;
                _J_RightHandSwipe = GestureEvent;
                if (_J_LeftHandSwipe != null)
                    LeftHandSwipeGesture.OnGestureDetected -= _J_LeftHandSwipe;
                _J_LeftHandSwipe = GestureEvent;
                LeftHandSwipeGesture.OnGestureDetected += GestureEvent;
                RightHandSwipeGesture.OnGestureDetected += GestureEvent;
            }
        }

        public static void ChangePostureEventHandler(Action<string> PostureEvent)
        {
            /*
             * 
             *  Posture에 대한 이벤트 핸들러를 변경한다.
             * 
             * 
             * */
            if (_PostureAction != null)
            {
                Posture.PostureDetected -= _PostureAction;
            }
            _PostureAction = PostureEvent;
            Posture.PostureDetected += PostureEvent;
        }

        void SensorSkeletonFrameReady(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return;
                }

                var allSkeletons = new Skeleton[skeletonFrameData.SkeletonArrayLength];

                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                foreach (Skeleton sd in allSkeletons)
                {
                    // the first found/tracked skeleton moves the mouse cursor
                    if (sd.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        // make sure both hands are tracked
                        if (sd.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                        {
                            var wristRight = sd.Joints[JointType.WristRight];
                            var leftShoulder = sd.Joints[JointType.ShoulderLeft];
                            var rightShoulder = sd.Joints[JointType.ShoulderRight];
                            var rightHand = sd.Joints[JointType.HandRight];
                            var head = sd.Joints[JointType.Head];
                            var rightHip = sd.Joints[JointType.HipRight];
                            double xScaled = (wristRight.Position.X - leftShoulder.Position.X) / ((rightShoulder.Position.X - leftShoulder.Position.X) * 2) * SystemParameters.PrimaryScreenWidth;
                            double yScaled = (rightHand.Position.Y - head.Position.Y) / (rightHip.Position.Y - head.Position.Y) * SystemParameters.PrimaryScreenHeight;


                            var cursorX = (int)xScaled + 1.5;
                            var cursorY = (int)yScaled + 1.5;

                            var leftClick = CheckForClickHold(xScaled, yScaled);
                            NativeMethods.SendMouseInput(Convert.ToInt32(cursorX), Convert.ToInt32(cursorY), (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick);
                        }
                    }
                }
            }
        }

        public static void ChangeMouseClickGestureEventHandler(Action<string> GestureEvent)
        {
            /*
             * 
             *  MouseControl Mode에서 클릭 이벤트에 대한 이벤트 핸들러를 변경한다.
             * 
             * */

            MouseController.Current.ClickGestureDetector.OnGestureDetected += GestureEvent;
        }

        private bool CheckForClickHold(double x, double y)
        {
            var screenwidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenheight = (int)SystemParameters.PrimaryScreenHeight;
            var clickwidth = (int)(screenwidth * ClickHoldingRectThreshold);
            var clickheight = (int)(screenheight * ClickHoldingRectThreshold);

            var newClickHold = new Rect(x - clickwidth, y - clickheight, clickwidth * 2, clickheight * 2);

            if (_clickHoldingLastRect != Rect.Empty)
            {
                if (newClickHold.IntersectsWith(_clickHoldingLastRect))
                {
                    if ((int)_clickHoldingTimer.ElapsedMilliseconds > (1 * 1000))
                    {
                        _clickHoldingTimer.Stop();
                        _clickHoldingLastRect = Rect.Empty;
                        return true;
                    }

                    if (!_clickHoldingTimer.IsRunning)
                    {
                        _clickHoldingTimer.Reset();
                        _clickHoldingTimer.Start();
                    }
                    return false;
                }

                _clickHoldingTimer.Stop();
                _clickHoldingLastRect = newClickHold;
                return false;
            }

            _clickHoldingLastRect = newClickHold;
            if (!_clickHoldingTimer.IsRunning)
            {
                _clickHoldingTimer.Reset();
                _clickHoldingTimer.Start();
            }
            return false;
        }
    }
}
