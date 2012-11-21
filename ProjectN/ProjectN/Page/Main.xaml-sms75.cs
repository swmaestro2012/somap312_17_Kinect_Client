using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using WPFMediaKit;
using WPFMediaKit.DirectShow;
using WPFMediaKit.DirectShow.Controls;
using WPFMediaKit.DirectShow.MediaPlayers;
using DirectShowLib;
using WPFMediaKit.DirectX;

namespace ProjectN
{
    /// <summary>
    /// ScreenSave.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Main : UserControl
    {

        #region ControlList

        public static MainPageComponent MainPageControl = new MainPageComponent();
        ReadBarcode barcodeControl = null;
        BarcodeProductInfo bProductInfoControl = null;
        TakePicture TakePictureControl = null;
        CheckPicture CheckPictureControl = null;
        ViewStyleset ViewStylesetControl = null;
        StylesetProductInfo sProductInfoControl = null;
        SaleCupon SaleCuponControl = null;

        #endregion

        CameraControl CameraController = new CameraControl();

        #region CommonControlVariables

        RenderTargetBitmap bmp = null;

        string FrontPicturePath = null;
        string BackPicturePath = null;
        string TopBarcode = null;
        string DownBarcode = null;

        #endregion

        #region MainControlVariables

        ThreadStart getCameraTS;
        Thread getCameraThread;

        #endregion

        #region BarcodeReaderVariables

        BarcodeProcessing BarcodeProcessor = new BarcodeProcessing();

        ThreadStart getBarcodeTS;
        Thread getBarcodeThread;

        System.Timers.Timer bitmapTimer = new System.Timers.Timer();
        System.Timers.Timer barcodeTimer = new System.Timers.Timer();

        RenderTargetBitmap RenderBitmap = null;

        BitmapEncoder BmpEncoder;
        MemoryStream ms;

        int CurrentBarcodeReadMode = 0;

        #endregion

        #region TakePictureVariables

        DispatcherTimer TakePictureTimer = new DispatcherTimer();
        int TimeSec = 5;
        int CurrentPictureMode = 0;

        #endregion

        #region BarcodeProductInfoVariables



        #endregion

        public Main()
        {
            InitializeComponent();
            foreach (DsDevice div in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            {
                if (div.Name == "Microsoft LifeCam Studio")
                {
                    DsDevice device = div;
                    videoElement.VideoCaptureDevice = device;
                    videoElement.VideoCaptureSource = device.Name;
                }
            }
            Action<string> PostureEventHandler = _Main_PostureAction;
            KinectController.ChangePostureEventHandler(PostureEventHandler);
        }

        private void initVideoElement()
        {
            PageControl.getGrid(Window.GetWindow(this)).Children.Remove(videoElement);
            videoElement = new VideoCaptureElement();
            videoElement.DesiredPixelWidth = 1920;
            videoElement.DesiredPixelHeight = 1080;
            videoElement.LoadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Play;
            videoElement.FPS = 30;
            videoElement.RenderTransformOrigin = new Point(0.5, 0.5);
            videoElement.Margin = new Thickness(-423, 423, -423, 421);

            ScaleTransform videoElementScale = new ScaleTransform();
            videoElementScale.ScaleY = -1;
            RotateTransform videoElementRotate = new RotateTransform();
            videoElementRotate.Angle = 90;

            TransformGroup videoElementTransform = new TransformGroup();
            videoElementTransform.Children.Add(videoElementScale);
            videoElementTransform.Children.Add(videoElementRotate);

            PageControl.getGrid(Window.GetWindow(this)).Children.Add(videoElement);

            foreach (DsDevice div in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            {
                if (div.Name == "Microsoft LifeCam Studio")
                {
                    DsDevice device = div;
                    videoElement.VideoCaptureDevice = device;
                    videoElement.VideoCaptureSource = device.Name;
                }
            }
        }

        #region MainPageFunc
        
        private void _Main_Initialize()
        {
            PageControl.getGrid(Window.GetWindow(this)).Children.Add(MainPageControl);
        }

        private void _Main_P_RightHandUp(object sender, RoutedEventArgs e)
        {
            barcodeControl = new ReadBarcode();
            this._Global_C_RemoveControl(MainPageControl);
            MainPageControl.Dispose();
            MainPageControl = null;
            this._Global_C_AddControl(barcodeControl);
            _Barcode_Initialize();
        }

        private void _Main_PostureAction(string posture)
        {
            /*
             * 
             *  Posture Action
             *   - HandsJoined
             *   - LeftHandOverHead
             *   - RightHandOverHead
             *   - LeftHello
             *   - RightHello
             *   
             * */
            switch (posture)
            {
                case "HandsJoined": break;
                case "LeftHandOverHead": break;
                case "RightHandOverHead": break;
                case "LeftHello":  break;
                case "RightHello": this._Main_P_RightHandUp(null, null); break;
            }
        }

        #endregion


        #region BarcodeReaderFunc

        private void _Barcode_Initialize()
        {
            getCameraTS = new ThreadStart(this._Barcode_TS_setTimer);
            getCameraThread = new Thread(getCameraTS);
            getCameraThread.Start();

            getBarcodeTS = new ThreadStart(this._Barcode_TS_setBarcodeTimer);
            getBarcodeThread = new Thread(getBarcodeTS);
            getBarcodeThread.Start();

            Action<string> PostureEventHandler = _Barcode_PostureAction;
            KinectController.ChangePostureEventHandler(PostureEventHandler); 
        }

        private void _Barcode_TS_setTimer()
        {
            bitmapTimer.Interval = 1000;
            bitmapTimer.Elapsed += new System.Timers.ElapsedEventHandler(this._Barcode_T_setBitmap);
            bitmapTimer.Enabled = true;
        }

        private void _Barcode_TS_setBarcodeTimer()
        {
            barcodeTimer.Interval = 1000;
            barcodeTimer.Elapsed += new System.Timers.ElapsedEventHandler(this._Barcode_T_getBarcodeImage);
            barcodeTimer.Enabled = true;
        }

        private void _Barcode_T_setBitmap(object sender, EventArgs e)
        {
            /*
            int width = 0, height = 0;
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(delegate
            {
                ms = new MemoryStream();
                width = (int)videoElement.ActualHeight;
                height = (int)videoElement.ActualWidth;
            }));
            
            VideoCaptureElement videoTemp = null;
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(delegate
            {
                BmpEncoder = new PngBitmapEncoder();
                RenderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
                videoTemp = new VideoCaptureElement { DataContext = videoElement.DataContext };
                RenderBitmap.Render(videoTemp);
                BmpEncoder.Frames.Add(BitmapFrame.Create(RenderBitmap));
                BmpEncoder.Save(ms);
            }));
             * */
        }

        private void _Barcode_T_getBarcodeImage(object sender, EventArgs e)
        {
            /*
            if (RenderBitmap != null)
            {
                System.Drawing.Bitmap bm = null;
                try
                {
                    bm = new System.Drawing.Bitmap(ms);
                    string ReadResult = BarcodeProcessor.ReadBarcode(bm);

                    if (ReadResult != "NoBarcode" && ReadResult != "Fail")
                    {
                        _Global_C_RemoveControl(barcodeControl);
                        bProductInfoControl = new BarcodeProductInfo();
                        _Global_C_AddControl(bProductInfoControl);
                        bitmapTimer.Elapsed -= this._Barcode_T_setBitmap;
                        barcodeTimer.Elapsed -= this._Barcode_T_getBarcodeImage;
                        bitmapTimer.Stop();
                        bitmapTimer.Dispose();
                        getCameraThread.Interrupt();
                        getCameraThread.Join();
                        getCameraThread.Abort();
                        barcodeTimer.Stop();
                        barcodeTimer.Dispose();
                        Console.WriteLine(ReadResult);
                        getBarcodeThread.Interrupt();
                        getBarcodeThread.Join();
                        getBarcodeThread.Abort();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("MemoryStream is empty");
                }
            }
             * **/
        }

        private void _Barcode_D_Dispose(string barcode)
        {
            _bProductInf_Initialize(barcode);
        }

        private void _Barcode_PostureAction(string posture)
        {
            /*
             * 
             *  Posture Action
             *   - HandsJoined
             *   - LeftHandOverHead
             *   - RightHandOverHead
             *   - LeftHello
             *   - RightHello
             *   
             * */
            switch (posture)
            {
                case "HandsJoined": break;
                case "LeftHandOverHead": break;
                case "RightHandOverHead": break;
                case "LeftHello": break;
                case "RightHello": this._Barcode_D_Dispose("temp"); break;
            }
        }

        #endregion

        #region BarcodeProductInfoFunc

        private void _bProductInf_Initialize(string barcode)
        {
            _Global_C_RemoveControl(barcodeControl);
            barcodeControl.Dispose();
            barcodeControl = null;
            RESTful Network = new RESTful();
            //Network.RESTfulSingleParmRequest();
            bProductInfoControl = new BarcodeProductInfo();
            _Global_C_AddControl(bProductInfoControl);

            Action<string> PostureEventHandler = _bProductInf_PostureAction;
            KinectController.ChangePostureEventHandler(PostureEventHandler);
            if (CurrentBarcodeReadMode == 1)
                bProductInfoControl.hiddenUiAddProduct();
        }

        private void _bProductInf_PostureAction(string posture)
        {
            /*
             * 
             *  Posture Action
             *   - HandsJoined
             *   - LeftHandOverHead
             *   - RightHandOverHead
             *   - LeftHello
             *   - RightHello
             *   
             * */
            switch (posture)
            {
                case "HandsJoined": break;
                case "LeftHandOverHead": this._bProductInf_P_RightHandUp(null, null); break;
                case "RightHandOverHead": this._bProductInf_P_RightHandUp(null, null); break;
                case "LeftHello": this._bProductInf_P_LeftHandUp(null, null); break;
                case "RightHello": this._bProductInf_P_RightHandUp(null, null); break;
            }
        }

        private void _bProductInf_P_LeftHandUp(object sender, EventArgs e)
        {
            //act --> 바코드 다시찍기
            barcodeControl = new ReadBarcode();
            CurrentBarcodeReadMode = 0;
            _Global_C_AddControl(barcodeControl);
            _Global_C_RemoveControl(bProductInfoControl);
            bProductInfoControl.Dispose();
            bProductInfoControl = null;
        }

        private void _bProductInf_P_RightHandUp(object sender, EventArgs e)
        {
            //act --> 사진 찍기
            TakePictureControl = new TakePicture();
            _Global_C_AddControl(TakePictureControl);
            _TakePicture_Initialize(0);
            _Global_C_RemoveControl(bProductInfoControl);
            bProductInfoControl.Dispose();
            bProductInfoControl = null;
        }

        private void _bProductInf_P_HandOverHead(object sender, EventArgs e)
        {
            //act --> 바코드 더 찍기
            barcodeControl = new ReadBarcode();
            CurrentBarcodeReadMode = 1;
            _Global_C_AddControl(barcodeControl);
            _Global_C_RemoveControl(bProductInfoControl);
            bProductInfoControl.Dispose();
            bProductInfoControl = null;
        }

        #endregion

        #region TakePictureFunc

        private void _TakePicture_Initialize(int Mode)
        {
            CurrentPictureMode = Mode;
            if (Mode == 0)
                TimeSec = 5;
            else
                TimeSec = 7;
            TakePictureTimer.Stop();
            TakePictureTimer.Interval = new TimeSpan(0, 0, 1);
            TakePictureTimer.Tick += new EventHandler(_TakePicture_T_TakePictureStart);

            Action<string> PostureEventHandler = _TakePicture_PostureAction;
            KinectController.ChangePostureEventHandler(PostureEventHandler); 
        }

        private void _TakePicture_T_TakePictureStart(object sender, EventArgs e)
        {
            TimeSec--;
            if (TimeSec == 0)
            {
                TakePictureTimer.Stop();
                getCameraTS = new ThreadStart(this._TakePicture_T_getCameraImage);
                getCameraThread = new Thread(getCameraTS);
                getCameraThread.Start();
            }
        }

        private void _TakePicture_T_getCameraImage()
        {
            int width = 0, height = 0;
            string path = null;
            bool flag = true;
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
            {
                width = (int)videoElement.ActualHeight;
                height = (int)videoElement.ActualWidth;
                bmp =  ConverterBitmapImage(videoElement);
                path = CameraController.procImage(bmp);
                while (flag)
                {
                    if (path != null)
                    {
                        flag = false;
                        if (CurrentPictureMode == 0)
                            FrontPicturePath = path;
                        else if (CurrentPictureMode == 1)
                            BackPicturePath = path;

                        TakePictureTimer = new DispatcherTimer();
                        CheckPictureControl = new CheckPicture(path, CurrentPictureMode);
                        _Global_C_RemoveControl(TakePictureControl);
                        TakePictureControl.Dispose();
                        TakePictureControl = null;
                        _Global_C_AddControl(CheckPictureControl);
                        _CheckPicture_Initialize();

                    }
                }
            }));

        }

        private static RenderTargetBitmap ConverterBitmapImage(FrameworkElement element)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingContext.DrawRectangle(new VisualBrush(element), null,
                new Rect(new Point(0, 0), new Point(element.ActualWidth, element.ActualHeight)));
            drawingContext.Close();

            RenderTargetBitmap target =
                new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight,
                96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            target.Render(drawingVisual);
            return target;
        }

        private void _TakePicture_P_LeftHandUp(object sender, EventArgs e)
        {
            //act --> 바코드 다시찍기
            barcodeControl = new ReadBarcode();
            CurrentBarcodeReadMode = 0;
            _Global_C_AddControl(barcodeControl);
            _Global_C_RemoveControl(TakePictureControl);
            TakePictureControl.Dispose();
            TakePictureControl = null;
        }

        private void _TakePicture_P_RightHandUp(object sender, EventArgs e)
        {
            //act --> 사진 촬영 시작
            Action<string> PostureEventHandler = _TakePicture_PostureAction_OnRightHandUp;
            KinectController.ChangePostureEventHandler(PostureEventHandler);
            TakePictureControl.hiddenUiButtonComponent();
            TakePictureTimer.Start();
        }


        private void _TakePicture_PostureAction(string posture)
        {
            /*
             * 
             *  Posture Action
             *   - HandsJoined
             *   - LeftHandOverHead
             *   - RightHandOverHead
             *   - LeftHello
             *   - RightHello
             *   
             * */
            switch (posture)
            {
                case "HandsJoined": break;
                case "LeftHandOverHead": break;
                case "RightHandOverHead": break;
                case "LeftHello": this._TakePicture_P_LeftHandUp(null, null); break;
                case "RightHello": this._TakePicture_P_RightHandUp(null, null); break;
            }
        }

        private void _TakePicture_PostureAction_OnRightHandUp(string posture)
        {
            /*
             * 
             *  Posture Action
             *   - HandsJoined
             *   - LeftHandOverHead
             *   - RightHandOverHead
             *   - LeftHello
             *   - RightHello
             *   
             * */
            switch (posture)
            {
                case "HandsJoined": break;
                case "LeftHandOverHead": break;
                case "RightHandOverHead": break;
                case "LeftHello": break;
                case "RightHello": break;
            }
        }

        #endregion

        #region CheckPictureFunc

        private void _CheckPicture_Initialize()
        {
          //  initVideoElement();
            switch (CurrentPictureMode)
            {
                case 0 : 
                    Action<string> PostureEventHandlerM0 = _CheckPicture_PostureAction_Mode0;
                KinectController.ChangePostureEventHandler(PostureEventHandlerM0); break;
                case 1 : 
                    Action<string> PostureEventHandlerM1 = _CheckPicture_PostureAction_Mode1;
                KinectController.ChangePostureEventHandler(PostureEventHandlerM1); break;
            }
        }

        private void _CheckPicture_P_RightHandUp_Mode0(object sender, EventArgs e)
        {

            _Global_C_RemoveControl(CheckPictureControl);
            CheckPictureControl.Dispose();
            CheckPictureControl = null;
            _TakePicture_Initialize(1);
            TakePictureControl = new TakePicture();
            _Global_C_AddControl(TakePictureControl);
            
        }

        private void _CheckPicture_P_RightHandUp_Mode1(object sender, EventArgs e)
        {
            ViewStylesetControl = new ViewStyleset();
            _Global_C_AddControl(ViewStylesetControl);
            _Global_C_RemoveControl(CheckPictureControl);
            CheckPictureControl.Dispose();
            CheckPictureControl = null;
        }

        private void _CheckPicture_P_LeftHandUp_Mode0(object sender, EventArgs e)
        {
            _TakePicture_Initialize(0);
            TakePictureControl = new TakePicture();
            _Global_C_AddControl(TakePictureControl);
            _Global_C_RemoveControl(CheckPictureControl);
            CheckPictureControl.Dispose();
            CheckPictureControl = null;
        }

        private void _CheckPicture_P_LeftHandUp_Mode1(object sender, EventArgs e)
        {
            _TakePicture_Initialize(1);
            TakePictureControl = new TakePicture();
            _Global_C_AddControl(TakePictureControl);
            _Global_C_RemoveControl(CheckPictureControl);
            CheckPictureControl.Dispose();
            CheckPictureControl = null;
        }

        private void _CheckPicture_PostureAction_Mode0(string posture)
        {
            /*
             * 
             *  Posture Action
             *   - HandsJoined
             *   - LeftHandOverHead
             *   - RightHandOverHead
             *   - LeftHello
             *   - RightHello
             *   
             * */
            switch (posture)
            {
                case "HandsJoined": break;
                case "LeftHandOverHead": break;
                case "RightHandOverHead": break;
                case "LeftHello": this._CheckPicture_P_LeftHandUp_Mode0(null, null);  break;
                case "RightHello": this._CheckPicture_P_RightHandUp_Mode0(null, null); break;
            }
        }

        private void _CheckPicture_PostureAction_Mode1(string posture)
        {
            /*
             * 
             *  Posture Action
             *   - HandsJoined
             *   - LeftHandOverHead
             *   - RightHandOverHead
             *   - LeftHello
             *   - RightHello
             *   
             * */
            switch (posture)
            {
                case "HandsJoined": break;
                case "LeftHandOverHead": break;
                case "RightHandOverHead": break;
                case "LeftHello": this._CheckPicture_P_LeftHandUp_Mode1(null, null); break;
                case "RightHello": this._CheckPicture_P_RightHandUp_Mode1(null, null); break;
            }
        }

        #endregion 

        private void _Global_C_AddControl(UserControl targetControl)
        {
            PageControl.thisWindow = Window.GetWindow(this);
            Grid MainGrid = PageControl.getGrid(Window.GetWindow(this));
            
            MainGrid.Children.Add(targetControl);
        }

        private void _Global_C_RemoveControl(UserControl targetControl)
        {
            Grid MainGrid = PageControl.getGrid(Window.GetWindow(this));

            MainGrid.Children.Remove(targetControl);
        }     
    }
}

