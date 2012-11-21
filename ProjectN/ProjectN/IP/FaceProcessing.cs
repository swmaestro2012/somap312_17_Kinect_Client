using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenCvSharp;
using System.Runtime.InteropServices;

namespace ProjectN
{
    class FaceProcessing
    {
        #region CopyMemory
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);
        #endregion

        IplImage FindFace;

        public IplImage BitmapToIpImage(System.Drawing.Bitmap bitmapImg)
        {
            /*
             * 
             *  카메라로부터 받아온 비트맵 이미지를 OpenCV용 IplImage 화상으로 변환한다.
             * 
             * */

            IplImage retImage = Cv.CreateImage(new CvSize(bitmapImg.Width, bitmapImg.Height), BitDepth.U8, 3);

            System.Drawing.Imaging.BitmapData bmpData = bitmapImg.LockBits(
                                    new System.Drawing.Rectangle(0, 0, bitmapImg.Width, bitmapImg.Height),
                                    System.Drawing.Imaging.ImageLockMode.ReadWrite, 
                                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //변환한 비트맵 데이터를 IplImage 스트림으로 옮긴다.
            CopyMemory(retImage.ImageData, bmpData.Scan0, bmpData.Stride * bmpData.Height);

            bitmapImg.UnlockBits(bmpData);

            return retImage;
        }

        public System.Drawing.Bitmap FaceDetect(IplImage src)
        {
            
            // CvHaarClassifierCascade, cvHaarDetectObjects
            // 얼굴을 검출하기 위해서 Haar 분류기의 캐스케이드를 이용한다

            CvColor[] colors = new CvColor[]{
                new CvColor(0,0,255),
                new CvColor(0,128,255),
                new CvColor(0,255,255),
                new CvColor(0,255,0),
                new CvColor(255,128,0),
                new CvColor(255,255,0),
                new CvColor(255,0,0),
                new CvColor(255,0,255),
            };

            const double scale = 1.04;
            const double scaleFactor = 1.139;
            const int minNeighbors = 1;

            using (IplImage img = src.Clone())
            using (IplImage smallImg = new IplImage(new CvSize(Cv.Round(img.Width / scale), Cv.Round(img.Height / scale)), BitDepth.U8, 1))
            {
                // 얼굴 검출을 위한 화상을 생성한다.
                using (IplImage gray = new IplImage(img.Size, BitDepth.U8, 1))
                {
                    Cv.CvtColor(img, gray, ColorConversion.BgrToGray);
                    Cv.Resize(gray, smallImg, Interpolation.Linear);
                    Cv.EqualizeHist(smallImg, smallImg);
                }

                using (CvHaarClassifierCascade cascade = CvHaarClassifierCascade.FromFile(Environment.CurrentDirectory + "\\" + "haarcascade_frontalface_alt.xml"))
                using (CvMemStorage storage = new CvMemStorage())
                {
                    storage.Clear();

                    // 얼굴을 검출한다.
                    CvSeq<CvAvgComp> faces = Cv.HaarDetectObjects(smallImg, cascade, storage, scaleFactor, minNeighbors, 0, new CvSize(20, 20));

                    // 검출한 얼굴에 검은색 원을 덮어씌운다.
                    for (int i = 0; i < faces.Total; i++)
                    {
                        CvRect r = faces[i].Value.Rect;
                        CvPoint center = new CvPoint
                        {
                            X = Cv.Round((r.X + r.Width * 0.5) * scale),
                            Y = Cv.Round((r.Y + r.Height * 0.5) * scale)
                        };
                        int radius = Cv.Round((r.Width + r.Height) * 0.25 * scale);
                        img.Circle(center, radius, new CvColor(0, 0, 0), -1, LineType.Link8, 0);
                    }
                }
                FindFace = img.Clone();

                //생성한 IplImage 화상을 비트맵으로 변환해 반환한다.
                return FindFace.ToBitmap(System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            }
        }


        public void Dispose()
        {
            if (FindFace != null) FindFace.Dispose();
        }
    }
}
