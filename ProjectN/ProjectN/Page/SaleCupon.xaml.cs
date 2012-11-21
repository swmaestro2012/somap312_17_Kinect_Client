using System;
using System.Windows.Controls;
namespace ProjectN
{
    /// <summary>
    /// ScreenSave.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SaleCupon : UserControl, IDisposable
    {
        public SaleCupon(long ProductId)
        {
            InitializeComponent();
        }

        public void Dispose()
        {

        }
    }
}

