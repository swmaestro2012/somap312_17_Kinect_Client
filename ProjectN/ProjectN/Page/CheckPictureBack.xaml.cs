using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ProjectN
{
    /// <summary>
    /// ScreenSave.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CheckPictureBack : UserControl, IDisposable
    {
        public CheckPictureBack(string TakePicPath, int PictureMode)
        {
            InitializeComponent();
            BitmapImage bimg = new BitmapImage();
            bimg.BeginInit();
            bimg.UriSource = new Uri(TakePicPath);
            bimg.CacheOption = BitmapCacheOption.OnLoad;
            bimg.EndInit();
            imgUserPicture.Source = bimg;
        }

        public void disposeImgUserPictureResource()
        {
            
        }

        public void Dispose()
        {

        }
    }
}

