using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Drawing;

using com.google.zxing;
using com.google.zxing.common;
using com.google.zxing.client;
using com.google.zxing.client.result;
using com.google.zxing.qrcode;

namespace ProjectN
{
    class BarcodeProcessing
    {
        public BarcodeProcessing()
        {

        }

        public string ReadBarcode(Bitmap BarcodeImage)
        {
            RGBLuminanceSource BarcodeSource = new RGBLuminanceSource(BarcodeImage, BarcodeImage.Width, BarcodeImage.Height);
            BinaryBitmap BarcodeImageBin = new BinaryBitmap(new GlobalHistogramBinarizer(BarcodeSource));
            MultiFormatReader BarcodeReader = new MultiFormatReader();
            try
            {
                Result BarcodeDecodeResult;
                BarcodeDecodeResult = BarcodeReader.decode(BarcodeImageBin);
                if (BarcodeDecodeResult.Text != null)
                    return BarcodeDecodeResult.Text;
                else
                    return "NoBarcode";

            }
            catch (Exception e)
            {
                return "Fail";
            }
        }
    }
}
