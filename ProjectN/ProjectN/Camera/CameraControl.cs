using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ProjectN
{
    class CameraControl
    {
        public string SaveImage(RenderTargetBitmap bitmapData)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapData));
            

            string tempPath = Environment.CurrentDirectory.ToString() + "\\temp";
            string photoPath = tempPath + "\\photo\\";

            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(photoPath);

            string filePath = photoPath + getPhotoFileName();
            FileStream fstream = new FileStream(filePath, FileMode.Create);
            encoder.Save(fstream);
            fstream.Close();
            
            return filePath;
        }

        public string SaveImage(Bitmap bitmapData)
        {
            string tempPath = Environment.CurrentDirectory.ToString() + "\\temp";
            string photoPath = tempPath + "\\photo\\";

            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(photoPath);

            string filePath = photoPath + getPhotoFileName();

            bitmapData.Save(filePath, ImageFormat.Jpeg);


            return filePath;
        }

        public string procImage(RenderTargetBitmap bitmapData)
        {
            string filePath = SaveImage(bitmapData);

            FaceProcessing FaceProcessor = new FaceProcessing();

            IplImage IplObject = FaceProcessor.BitmapToIpImage(new Bitmap(filePath));
            Bitmap processImage = FaceProcessor.FaceDetect(IplObject);

            processImage.Save(filePath.Insert(filePath.Length - 4, "_proc"), ImageFormat.Jpeg);

            return filePath;
        }

        public string procImage(Bitmap bitmapData)
        {
            string filePath = SaveImage(bitmapData);

            FaceProcessing FaceProcessor = new FaceProcessing();

            IplImage IplObject = FaceProcessor.BitmapToIpImage(new Bitmap(filePath));
            Bitmap processImage = FaceProcessor.FaceDetect(IplObject);

            processImage.Save(filePath.Insert(filePath.Length - 4, "_proc"), ImageFormat.Jpeg);

            return filePath;
        }

        private string getPhotoFileName()
        {
            string fileName = null;

            fileName = DateTime.Now.ToFileTime().ToString() + ".jpg";

            return fileName;
        }
    }
}
