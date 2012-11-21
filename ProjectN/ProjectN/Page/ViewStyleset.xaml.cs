using System;
using System.Windows.Controls;

namespace ProjectN
{
    /// <summary>
    /// ScreenSave.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewStyleset : UserControl, IDisposable
    {
        public long SelectedStyleset = 0;

        public ViewStyleset()
        {
            InitializeComponent();
        }

        public void Dispose()
        {

        }

    }
}
