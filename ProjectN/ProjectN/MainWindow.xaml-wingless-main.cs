using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace ProjectN
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            KinectController.InitializeKinectComponent();
            EventHandler<SkeletonFrameReadyEventArgs> s = SkeletonReady;
            KinectController.ChangeKinectEventHandler(null, null, s, null);
            Action<string> gs = GestureAction;
            KinectController.ChangeGestureEventHandler(gs);
        }

        void SkeletonReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame sf = e.OpenSkeletonFrame();
            if (sf == null) return;

            Skeleton[] data = new Skeleton[sf.SkeletonArrayLength];

            sf.CopySkeletonDataTo(data);

            foreach (Skeleton s in data)
            {
                if (s.TrackingState == SkeletonTrackingState.Tracked)
                {
                    foreach (Joint joint in s.Joints)
                    {
                        if (joint.JointType == JointType.HandLeft)
                        {
                            KinectController.SwipeGesture.Add(joint.Position, KinectController.KS);
                        }
                    }
                }
            }
        }

        void GestureAction(string gesture)
        {
            MessageBox.Show(gesture);
        }
    }
}
