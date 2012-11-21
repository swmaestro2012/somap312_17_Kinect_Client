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
using DirectShowLib;

namespace ProjectN_Screen
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.WindowState = System.Windows.WindowState.Maximized;
            foreach (DsDevice div in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            {
                if (div.Name == "Microsoft LifeCam Studio")
                {
                    DsDevice device = div;
                    videoElement.VideoCaptureDevice = device;
                    videoElement.VideoCaptureSource = device.Name;
                }
            }
        }
    }
}
