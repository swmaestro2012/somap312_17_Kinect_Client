using System;
using System.Windows.Controls;

namespace ProjectN
{
    /// <summary>
    /// ScreenSave.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StylesetProductInfo : UserControl, IDisposable
    {

        public long Product;

        public StylesetProductInfo(long ProductID)
        {
            InitializeComponent();
            Product = ProductID;
        }

        public void LikeStyleset()
        {
          
        }

        public void Dispose()
        {

        }

    }
}

