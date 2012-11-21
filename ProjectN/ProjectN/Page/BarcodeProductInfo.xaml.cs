using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RestSharp;

namespace ProjectN
{
    /// <summary>
    /// ScreenSave.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BarcodeProductInfo : UserControl, IDisposable
    {
        public BarcodeProductInfo()
        {
            InitializeComponent();
        }

        public void hiddenUiAddProduct()
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
            {
                uiAddProduct.Visibility = Visibility.Hidden;
            }));
        }

        public void setProductInfo(IRestResponse<_REST_ProductInfo> ProductInfo)
        {
            imgProduct.Source = new BitmapImage(new Uri(ProductInfo.Data.imageUrl));
            lblProductName.Content = ProductInfo.Data.name;
            lblProductInfo2.Content = ProductInfo.Data.price;
            lblProductInfo3.Content = ProductInfo.Data.price;
            string season = "";
            string lookType = "";

            switch (ProductInfo.Data.season)
            {
                case 0: season = "봄"; break;
                case 1: season = "여름"; break;
                case 2: season = "가을"; break;
                case 3: season = "겨울"; break;
            }

            if (ProductInfo.Data.lookType == 1)
                lookType = "하의";
            else
                lookType = "상의";

            lblProductInfo1.Content = ProductInfo.Data.year + " " + season + "시즌 상품";
            lblProductInfo1.Content += "\n" + lookType;
            lblProductInfo1.Content += "\n" + ProductInfo.Data.shotCount + "번 찍힘";
        }

        public void Dispose()
        {

        }
    }
}

