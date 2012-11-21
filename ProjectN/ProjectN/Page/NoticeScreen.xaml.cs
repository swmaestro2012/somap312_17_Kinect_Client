using System;
using System.Windows.Controls;

namespace ProjectN
{
    /// <summary>
    /// ScreenSave.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NoticeScreen : UserControl, IDisposable
    {
        public NoticeScreen(string Message)
        {
            InitializeComponent();
            this.lblMessage.Content = Message;
        }

        public void Dispose()
        {

        }
    }
}

