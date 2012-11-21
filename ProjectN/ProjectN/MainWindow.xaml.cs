using System;
using System.Windows;

using Microsoft.Kinect;
using Kinect.Toolbox;
using System.Diagnostics;

namespace ProjectN
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int ScreenWidth;
        public static int ScreenHeight;

        private const float ClickHoldingRectThreshold = 0.05f;
        private Rect _clickHoldingLastRect;
        private readonly Stopwatch _clickHoldingTimer;

        private const float SkeletonMaxX = 0.60f;
        private const float SkeletonMaxY = 0.40f;
        
        public MainWindow()
        {
            InitializeComponent();
            this.Topmost = true;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.WindowState = System.Windows.WindowState.Maximized;

            ScreenWidth = Convert.ToInt32(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Height.ToString());
            ScreenHeight = Convert.ToInt32(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width.ToString());
            KinectController.InitializeKinectComponent();
            EventHandler<SkeletonFrameReadyEventArgs> s = SkeletonReady;
            KinectController.ChangeKinectEventHandler(null, null, s, null);

            _clickHoldingTimer = new Stopwatch();

            PageControl.MainThread = this;

            Main m = new Main();

            MainGrid.Width = 1440;
            MainGrid.Height = 2560;
            
            PageControl.registerUserControl(m);

            MainGrid.Children.Add(m);
            PageControl.getGrid(Window.GetWindow(this)).Children.Add(Main.MainPageControl);
            Window WindowObject = Window.GetWindow(this);
        }

        void SkeletonReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (!((KinectSensor)sender).SkeletonStream.IsEnabled)
            {
                return;
            }

            SkeletonFrame sf = e.OpenSkeletonFrame();
            if (sf == null) return;

            Skeleton[] data = new Skeleton[sf.SkeletonArrayLength];
            Action<string> gs = GestureAction;
 
            sf.CopySkeletonDataTo(data);

            foreach (Skeleton s in data)
            {
                if (s.TrackingState == SkeletonTrackingState.Tracked)
                {
                    if (s.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                    {
                        var wristRight = s.Joints[JointType.WristRight];
                        var leftShoulder = s.Joints[JointType.ShoulderLeft];
                        var rightShoulder = s.Joints[JointType.ShoulderRight];
                        var rightHand = s.Joints[JointType.HandRight];
                        var head = s.Joints[JointType.Head];
                        var rightHip = s.Joints[JointType.HipRight];
                        double xScaled = (wristRight.Position.X - leftShoulder.Position.X) / ((rightShoulder.Position.X - leftShoulder.Position.X) * 2) * SystemParameters.PrimaryScreenWidth;
                        double yScaled = (rightHand.Position.Y - head.Position.Y) / (rightHip.Position.Y - head.Position.Y) * SystemParameters.PrimaryScreenHeight;


                        var cursorX = (int)xScaled + 1.5;
                        var cursorY = (int)yScaled + 1.5;

                        var leftClick = CheckForClickHold(xScaled, yScaled);
                        NativeMethods.SendMouseInput(Convert.ToInt32(cursorX), Convert.ToInt32(cursorY), (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick);
                    }

                    foreach (Joint joint in s.Joints)
                    {
                        if (joint.JointType == JointType.HandLeft)
                        {
                            KinectController.LeftHandSwipeGesture.Add(joint.Position, KinectController.KS);
                            if (KinectController.KinectMouseEnabled == true)
                            {
                                MouseController.Current.SetHandPosition(KinectController.KS, joint, s);
                            }
                        }
                        else if (joint.JointType == JointType.HandRight)
                        {
                            KinectController.RightHandSwipeGesture.Add(joint.Position, KinectController.KS);
                        }
                        
                    }
                }
            }
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

        #region defineActionFunctions

        void PostureAction(string posture)
        {
            /*
             * 
             *  Posture Action
             *   - HandsJoined
             *   - LeftHandOverHead
             *   - RightHandOverHead
             *   - LeftHello
             *   - RightHello
             *   
             * */
            switch (posture)
            {
                case "HandsJoined": break;
                case "LeftHandOverHead": break;
                case "RightHandOverHead": break;
                case "LeftHello": break;
                case "RightHello": break;
            }
            MessageBox.Show(posture);
        }

        void GestureAction(string gesture)
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
            MessageBox.Show(gesture);
        }

        #endregion
    }
}
