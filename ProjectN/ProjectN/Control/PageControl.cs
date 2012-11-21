using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace ProjectN
{
    class PageControl
    {
        public static MainWindow MainThread;
        public static UserControl CurrentControl;
        public static Window thisWindow;

        public static Grid removeControl(Window Parent)
        {
            
            /*
             * 
             * WPF Page를 제거하고 해당 페이지가 들어있던 Grid를 반환한다.
             * 
             * */
            try
            {
                Grid MainGrid = (Grid)Parent.FindName("MainGrid");
                MainGrid.Children.Remove(MainGrid.Children[0]);
                return MainGrid;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Grid getGrid(Window Parent)
        {
            return (Grid)Parent.FindName("MainGrid");
        }

        public static UserControl registerUserControl(UserControl Page)
        {
            CurrentControl = Page;

            return CurrentControl;
        }

        public static void disposeControl(UserControl control)
        {
            Grid MainGrid = (Grid)thisWindow.FindName("MainGrid");
            MainGrid.Children.Remove(CurrentControl);
            CurrentControl = control;
        }
    }
}
