using System;
using System.Windows.Controls;

namespace ProjectN
{
    /// <summary>
    /// ScreenSave.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TakePicture : UserControl, IDisposable
    {
        public TakePicture()
        {
            InitializeComponent();
        }

        public void hiddenUiButtonComponent()
        {
            btnBack.Visibility = System.Windows.Visibility.Hidden;
            btnTakePicture.Visibility = System.Windows.Visibility.Hidden;
            uiTakePicture.Visibility = System.Windows.Visibility.Hidden;
            uiBack.Visibility = System.Windows.Visibility.Hidden;
            uiTakePictureMode.Visibility = System.Windows.Visibility.Hidden;
        }

        private void uiBack_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        public void Dispose()
        {

        }
    }
}

