using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media;
using System.Runtime.InteropServices;
using RestSharp;
using com.google.zxing;
using com.google.zxing.common;
using com.google.zxing.qrcode;

namespace ProjectN
{
    /// <summary>
    /// ScreenSave.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Main : UserControl
    {
        #region TakeImageUserAPIImport
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, System.Drawing.CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);

        #endregion

        #region ControlList

        public static MainPageComponent MainPageControl = new MainPageComponent();
        ReadMembership membershipControl = null;
        ReadBarcode barcodeControl = null;
        BarcodeProductInfo bProductInfoControl = null;
        TakePicture TakePictureControl = null;
        CheckPictureAll CheckPictureAControl = null;
        CheckPicture CheckPictureControl = null;
        CheckPictureBack CheckPictureBackControl = null;
        ViewStyleset ViewStylesetControl = null;
        StylesetProductInfo sProductInfoControl = null;
        StylesetProductInfo sProductExtraInfoControl = null;
        SaleCupon SaleCuponControl = null;

        NoticeScreen NoticeControl = null;

        protected Thread NetworkThread;
        protected ParameterizedThreadStart NetworkPTS;
        protected ThreadStart NetworkTS;

        #endregion

        CameraControl CameraController = new CameraControl();

        #region CommonControlVariables

        string FrontPicturePath = null;
        string BackPicturePath = null;
        string TopBarcode = null;
        long TopLookId = 0;
        string DownBarcode = null;
        long DownLookId = 0;
        long membershipCode = 0;

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

        int CurrentBarcodeReadMode = 0;

        #endregion

        #region TakePictureVariables

        DispatcherTimer TakePictureTimer = new DispatcherTimer();
        int TimeSec = 5;
        int CurrentPictureMode = 0;

        #endregion

        #region BarcodeProductInfoVariables



        #endregion

        #region CuponVariables
        IRestResponse<_REST_StylesetHashInfo> cuponhash;
        #endregion

        #region ViewStylesetVariables

        int StyletIdexLast = 0;
        int CurrentStylesetIdx = 0;
        _REST_StyleSetInfo[] StyleSetData = null;
        long CurrentStylesetUid = 0;
        _REST_StyleSetInfo[] UserLookList = null;

        #endregion

        #region NoticeVariables

        DispatcherTimer NoticeTimer = new DispatcherTimer();
        int NoticeTimeSec = 3;

        #endregion

        public Main()
        {
            InitializeComponent();
            _Main_Initialize();
        }

        #region MainPageFunc
        
        private void _Main_Initialize()
        {
            MainPageControl.btnStart.Click += new RoutedEventHandler(this._Main_P_RightHandUp);
        }

        private void _Main_P_RightHandUp(object sender, EventArgs e)
        {
            membershipControl = new ReadMembership();
            this._Global_C_RemoveControl(MainPageControl);
            MainPageControl.Dispose();
            MainPageControl = null;
            this._Global_C_AddControl(membershipControl);
            _Membership_Initialize();
        }

        private void _Membership_P_RightHandUp(object sender, EventArgs e)
        {
            barcodeControl = new ReadBarcode();
            this._Global_C_RemoveControl(membershipControl);
            membershipControl.Dispose();
            membershipControl = null;
            this._Global_C_AddControl(barcodeControl);
            _Barcode_Initialize();
        }

        private void _Membership_Initialize()
        {
            this.txtBarcodeInput.Visibility = System.Windows.Visibility.Visible;
            this.txtBarcodeInput.Text = "";
            this.txtBarcodeInput.Focus();
            System.Windows.Input.Keyboard.Focus(this.txtBarcodeInput);
        }

        private void _Membership_D_Dispose(string barcode)
        {
            membershipCode = Convert.ToInt64(barcode);
            barcodeControl = new ReadBarcode();
            this._Global_C_RemoveControl(membershipControl);
            membershipControl.Dispose();
            membershipControl = null;
            this._Global_C_AddControl(barcodeControl);
            NetworkPTS = new ParameterizedThreadStart(this._Barcode_Initialize);
            NetworkThread = new Thread(NetworkPTS);
            NetworkThread.Start(barcode);
            txtBarcodeInput.Text = "";
            txtBarcodeInput.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion

        #region BarcodeReaderFunc

        private void _Barcode_Initialize(object membershipId)
        {   
            RESTful RestObj = new RESTful();

            IRestResponse<_REST_MembershipInfo> RESTMember = RestObj.RESTgetMember(Convert.ToInt64(membershipId));

            if (RESTMember.Data.membershipId == "0")
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    _NoticeScreen_Initialize("회원정보를 찾을 수 없습니다.");
                    _Global_C_ShowWaitingScreen();
                }));
                _Main_Initialize();
                MainPageControl = new MainPageComponent();
                _Global_C_AddControl(MainPageControl);
                _Global_C_RemoveControl(barcodeControl);

            }
            else
            {
                getCameraTS = new ThreadStart(this._Barcode_TS_setTimer);
                getCameraThread = new Thread(getCameraTS);
                getCameraThread.Start();

                getBarcodeTS = new ThreadStart(this._Barcode_TS_setBarcodeTimer);
                getBarcodeThread = new Thread(getBarcodeTS);
                getBarcodeThread.Start();

               

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    _NoticeScreen_Initialize(RESTMember.Data.name + "님, 안녕하세요?");
                    this.txtBarcodeInput.Visibility = System.Windows.Visibility.Visible;
                    this.txtBarcodeInput.Text = "";
                    this.txtBarcodeInput.Focus();
                    System.Windows.Input.Keyboard.Focus(this.txtBarcodeInput);
                    _Global_C_ShowWaitingScreen();
                }));
            }
        }

        private void _Barcode_Initialize()
        {
            getCameraTS = new ThreadStart(this._Barcode_TS_setTimer);
            getCameraThread = new Thread(getCameraTS);
            getCameraThread.Start();

            getBarcodeTS = new ThreadStart(this._Barcode_TS_setBarcodeTimer);
            getBarcodeThread = new Thread(getBarcodeTS);
            getBarcodeThread.Start();

            this.txtBarcodeInput.Visibility = System.Windows.Visibility.Visible;
            this.txtBarcodeInput.Text = "";
            this.txtBarcodeInput.Focus();
            System.Windows.Input.Keyboard.Focus(this.txtBarcodeInput);
        }

        private void _Barcode_TS_setTimer()
        {
            bitmapTimer.Interval = 1000;
            bitmapTimer.Enabled = true;
        }

        private void _Barcode_TS_setBarcodeTimer()
        {
            barcodeTimer.Interval = 1000;
            barcodeTimer.Enabled = true;
        }

        private void txtBarcodeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (this.txtBarcodeInput.Text.Length == 13 && barcodeControl != null)
            {
                MultiFormatWriter barcodeWriter = new MultiFormatWriter();
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    _Global_C_ShowWaitingScreen();
                    ByteMatrix barcodeData = barcodeWriter.encode(txtBarcodeInput.Text, BarcodeFormat.EAN_13, (int)(barcodeControl.imgBarcodeImg.Width), (int)(barcodeControl.imgBarcodeImg.Height * 0.8));
                    barcodeControl.imgBarcodeImg.Source = getImageSource(barcodeData.ToBitmap());
                }));
                _Barcode_D_Dispose(txtBarcodeInput.Text);
            }
            else if (this.txtBarcodeInput.Text.Length == 16 && membershipControl != null)
            {
                MultiFormatWriter barcodeWriter = new MultiFormatWriter();
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    _Global_C_ShowWaitingScreen();
                }));
                _Membership_D_Dispose(txtBarcodeInput.Text);
            }
        }

        private ImageSource getImageSource(System.Drawing.Bitmap image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage tour1 = new BitmapImage();
            tour1.BeginInit();
            tour1.StreamSource = ms;
            tour1.EndInit();

            return tour1;
        }

        private void _Barcode_D_Dispose(string barcode)
        {
            NetworkPTS = new ParameterizedThreadStart(this._bProductInf_Initialize);
            NetworkThread = new Thread(NetworkPTS);
            NetworkThread.Start(barcode);
            txtBarcodeInput.Text = "";
        }

        #endregion

        #region BarcodeProductInfoFunc

        private void _bProductInf_Initialize(object barcode)
        {
            _Global_C_RemoveControl(barcodeControl);
            barcodeControl.Dispose();
            barcodeControl = null;
            
            RESTful NetworkObj = new RESTful();
            IRestResponse<_REST_ProductInfo> Product = NetworkObj.RESTgetProductByBarCode((string)barcode);
            if (Product.Data == null)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                    {
                        _NoticeScreen_Initialize("상품정보를 찾을 수 없습니다.");
                        _bProductInf_P_LeftHandUp(null, null);
                    }));
            }
            else
            {
                if (Product.Data.lookType == 0)
                {
                    if (TopLookId == 0)
                        TopLookId = Product.Data.id;
                }
                else if (Product.Data.lookType == 1)
                {
                    if (DownLookId == 0)
                        DownLookId = Product.Data.id;
                }

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                    {
                        bProductInfoControl = new BarcodeProductInfo();

                        bProductInfoControl.setProductInfo(Product);


                        _Global_C_AddControl(bProductInfoControl);
                    }));
                bProductInfoControl.btnNext.Click += new RoutedEventHandler(_bProductInf_P_RightHandUp);
                bProductInfoControl.btnReTakePicture.Click += new RoutedEventHandler(_bProductInf_P_LeftHandUp);
                bProductInfoControl.btnAddProduct.Click += new RoutedEventHandler(_bProductInf_P_HandOverHead);
                if (CurrentBarcodeReadMode == 1)
                {
                    bProductInfoControl.hiddenUiAddProduct();
                }
                _Global_C_ShowWaitingScreen();
            }
        }

        private void _bProductInf_P_LeftHandUp(object sender, EventArgs e)
        {
            barcodeControl = new ReadBarcode();
            _Barcode_Initialize();
            CurrentBarcodeReadMode = 0;
            _Global_C_RemoveControl(bProductInfoControl);
            bProductInfoControl.Dispose();
            bProductInfoControl = null;
            _Global_C_AddControl(barcodeControl);
            
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
            _Barcode_Initialize();
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
            {
                TakePictureControl.imgNotice.Source = new BitmapImage(new Uri("/ProjectN;component/Images/text.countdown7.png", UriKind.Relative));
                TakePictureControl.uiTakePictureMode.Source = new BitmapImage(new Uri("/ProjectN;component/Images/BackMode.png", UriKind.Relative));
                TimeSec = 7;
            }
            TakePictureTimer.Stop();
            TakePictureTimer.Interval = new TimeSpan(0, 0, 1);
            TakePictureTimer.Tick += new EventHandler(_TakePicture_T_TakePictureStart);

            TakePictureControl.btnBack.Click += new RoutedEventHandler(_TakePicture_P_LeftHandUp);
            TakePictureControl.btnTakePicture.Click += new RoutedEventHandler(_TakePicture_P_RightHandUp);

            TimeCountdown.LoadedBehavior = MediaState.Manual;
        }

        private void _TakePicture_T_TakePictureStart(object sender, EventArgs e)
        {
            if (TimeSec == 5)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    TimeCountdown.Play();
                }));
            }

            if (TimeSec == 2)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    TakePictureControl.imgNotice.Visibility = System.Windows.Visibility.Hidden;
                }));
            }

            if (TimeSec == 0)
            {
                TimeShot.Play();
                TakePictureTimer.Stop();
                getCameraTS = new ThreadStart(this._TakePicture_T_getCameraImage);
                getCameraThread = new Thread(getCameraTS);
                getCameraThread.Start();
            }
            TimeSec--;
        }

        private void _TakePicture_T_getCameraImage()
        {
            string path = null;

            System.Windows.Size sz = new System.Windows.Size(1080, 1920);
            IntPtr hDesk = GetDesktopWindow();
            IntPtr hScre = GetWindowDC(hDesk);
            IntPtr hDest = CreateCompatibleDC(hScre);
            IntPtr hBitmap = CreateCompatibleBitmap(hScre, (int)sz.Width, (int)sz.Height);
            IntPtr hOldBit = SelectObject(hDest, hBitmap);

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
            {
                System.Windows.Point CurrentWindowPoint = this.PointToScreen(new System.Windows.Point());

                BitBlt(hDest, 0, 0, (int)sz.Width, (int)sz.Height, hScre, (int)CurrentWindowPoint.X, 
                    (int)CurrentWindowPoint.Y, System.Drawing.CopyPixelOperation.SourceCopy | System.Drawing.CopyPixelOperation.CaptureBlt);
                
                System.Drawing.Bitmap bitmap = System.Drawing.Bitmap.FromHbitmap(hBitmap);
                SelectObject(hDest, hOldBit);
                DeleteObject(hBitmap);
                DeleteDC(hDest);
                ReleaseDC(hDesk, hScre);

                path = CameraController.procImage(bitmap);
                
                if (CurrentPictureMode == 0)
                    FrontPicturePath = path;
                else if (CurrentPictureMode == 1)
                    BackPicturePath = path;
                
                TakePictureTimer = new DispatcherTimer();
                if (CurrentPictureMode == 0)
                {
                    _Global_C_RemoveControl(TakePictureControl);
                    TakePictureControl.Dispose();
                    TakePictureControl = null;
                    TakePictureControl = new TakePicture();
                    _TakePicture_Initialize(1);
                    _Global_C_AddControl(TakePictureControl);
                }
                else
                {
                    CheckPictureAControl = new CheckPictureAll();
                    _Global_C_RemoveControl(TakePictureControl);
                    TakePictureControl.Dispose();
                    TakePictureControl = null;
                    _Global_C_AddControl(CheckPictureAControl);
                    _CheckPictureA_Initialize();
                }
            }));
        }

        private void _TakePicture_P_LeftHandUp(object sender, EventArgs e)
        {
            //act --> 바코드 다시찍기
            barcodeControl = new ReadBarcode();
            _Barcode_Initialize();
            CurrentBarcodeReadMode = 0;
            _Global_C_AddControl(barcodeControl);
            _Global_C_RemoveControl(TakePictureControl);
            TakePictureControl.Dispose();
            TakePictureControl = null;
        }

        private void _TakePicture_P_RightHandUp(object sender, EventArgs e)
        {
            TakePictureControl.hiddenUiButtonComponent();
            TakePictureTimer.Start();
        }

        #endregion

        #region CheckPictureAllFunc

        private void _CheckPictureA_Initialize()
        {
            Action<string> RightSwipeGestureHandler = _CheckPictureA_SwipeGestureRight;
            Action<string> LeftSwipeGestureHandler = _CheckPictureA_SwipeGestureLeft;
            KinectController.ChangeGestureEventHandler(RightSwipeGestureHandler, "R");
            KinectController.ChangeGestureEventHandler(LeftSwipeGestureHandler, "L");

            CheckPictureAControl.btnReTakePicture.Click += new RoutedEventHandler(_CheckPictureA_P_LeftHandUp_Mode1);
            CheckPictureAControl.btnViewStyleset.Click += new RoutedEventHandler(_CheckPictureA_P_RightHandUp_Mode1);
        }

        private void _CheckPictureA_SwipeGestureRight(string gesture)
        {
            switch (gesture)
            {
                case "SwipeToLeft": _CheckPictureA_Control(0); break;
                case "SwipeToRight": break;
            }
        }

        private void _CheckPictureA_SwipeGestureLeft(string gesture)
        {
            switch (gesture)
            {
                case "SwipeToLeft": break;
                case "SwipeToRight": _CheckPictureA_Control(1); break;
            }
        }

        private void _CheckPictureA_Control(int act)
        {
            if (act != 0)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    CheckPictureAControl.imgGuideline.Visibility = System.Windows.Visibility.Hidden;
                }));
            }
            if (act == 1)
                CheckPictureAControl.imgUserPicture.Source = new BitmapImage(new Uri(FrontPicturePath));
            else
                CheckPictureAControl.imgUserPicture.Source = new BitmapImage(new Uri(BackPicturePath));
        }

        private void _CheckPictureA_P_LeftHandUp_Mode1(object sender, EventArgs e)
        {
            _Global_C_RemoveControl(CheckPictureAControl);
            CheckPictureAControl.Dispose();
            CheckPictureAControl = null;
            TakePictureControl = new TakePicture();
            _TakePicture_Initialize(0);
            _Global_C_AddControl(TakePictureControl);
        }

        private void _CheckPictureA_P_RightHandUp_Mode1(object sender, EventArgs e)
        {
            _Global_C_ShowWaitingScreen();
            _Global_C_RemoveControl(CheckPictureAControl);
            ViewStylesetControl = new ViewStyleset();
            _Global_C_AddControl(ViewStylesetControl);
            NetworkTS = new ThreadStart(_ViewStyleset_Initialize);
            NetworkThread = new Thread(NetworkTS);
            NetworkThread.Start();
            CheckPictureAControl.Dispose();
            CheckPictureAControl = null;
        }

        #endregion

        #region CheckPictureFunc

        private void _CheckPicture_Initialize()
        {
            switch (CurrentPictureMode)
            {
                case 0 : 
                    CheckPictureControl.btnReTakePicture.Click += new RoutedEventHandler(_CheckPicture_P_LeftHandUp_Mode0);
                    CheckPictureControl.btnTakeBackPicture.Click += new RoutedEventHandler(_CheckPicture_P_RightHandUp_Mode0);
                    break;
                case 1 : 
                    CheckPictureBackControl.btnReTakePicture.Click += new RoutedEventHandler(_CheckPicture_P_LeftHandUp_Mode1);
                    CheckPictureBackControl.btnTakeBackPicture.Click += new RoutedEventHandler(_CheckPicture_P_RightHandUp_Mode1);
                    break;
            }
        }

        private void _CheckPicture_P_RightHandUp_Mode0(object sender, EventArgs e)
        {
            _Global_C_RemoveControl(CheckPictureControl);
            CheckPictureControl.Dispose();
            CheckPictureControl = null;
            TakePictureControl = new TakePicture();
            _TakePicture_Initialize(1);
            _Global_C_AddControl(TakePictureControl);
        }

        private void _CheckPicture_P_RightHandUp_Mode1(object sender, EventArgs e)
        {
            _Global_C_ShowWaitingScreen();
            _Global_C_RemoveControl(CheckPictureBackControl);
            ViewStylesetControl = new ViewStyleset();
            _Global_C_AddControl(ViewStylesetControl);
            NetworkTS = new ThreadStart(_ViewStyleset_Initialize);
            NetworkThread = new Thread(NetworkTS);
            NetworkThread.Start();
            CheckPictureBackControl.Dispose();
            CheckPictureBackControl = null;
        }

        private void _CheckPicture_P_LeftHandUp_Mode0(object sender, EventArgs e)
        {
            
            TakePictureControl = new TakePicture();
            _TakePicture_Initialize(0);
            _Global_C_AddControl(TakePictureControl);
            _Global_C_RemoveControl(CheckPictureControl);
            CheckPictureControl.Dispose();
            CheckPictureControl = null;
        }

        private void _CheckPicture_P_LeftHandUp_Mode1(object sender, EventArgs e)
        {
            TakePictureControl = new TakePicture();
            _TakePicture_Initialize(1);
            _Global_C_AddControl(TakePictureControl);
            _Global_C_RemoveControl(CheckPictureBackControl);
            CheckPictureBackControl.Dispose();
            CheckPictureBackControl = null;
        }

        #endregion 

        #region ViewStylesetFunc

        private void _ViewStyleset_Initialize()
        {
            IRestResponse<_REST_StyleSetListInfo> TopStyleset = null;
            IRestResponse<_REST_StyleSetListInfo> DownStyleset = null;
            
            RESTful RESTObj = new RESTful();
            if(TopLookId != 0)
                cuponhash = RESTObj.RESTUploadUserLook(TopLookId, DownLookId, FrontPicturePath, FrontPicturePath.Insert(FrontPicturePath.Length -4, "_proc"), BackPicturePath, "2011003539244269");
            if(DownLookId != 0)
                cuponhash = RESTObj.RESTUploadUserLook(DownLookId, TopLookId, FrontPicturePath, FrontPicturePath.Insert(FrontPicturePath.Length - 4, "_proc"), BackPicturePath, "2011003539244269");

            if(TopLookId != 0)
                TopStyleset = RESTObj.RESTgetStyleSetListById(TopLookId);
            if (DownLookId != 0)
                DownStyleset = RESTObj.RESTgetStyleSetListById(DownLookId);

            if (TopStyleset != null && DownStyleset != null)
            {
                if (TopStyleset.Data != null && DownStyleset.Data != null)
                {
                    StyleSetData = TopStyleset.Data.ToArray();
                    _REST_StyleSetInfo[] DownStylesetData = DownStyleset.Data.ToArray();
                    DownStylesetData.CopyTo(StyleSetData, StyleSetData.Length);
                }
            }
            else if (TopStyleset != null)
            {
                if (TopStyleset.Data != null)
                {
                    StyleSetData = TopStyleset.Data.ToArray();
                }
            }
            else if (DownStyleset != null)
            {
                if (DownStyleset.Data != null)
                {
                    StyleSetData = DownStyleset.Data.ToArray();
                }
            }

            StyletIdexLast = StyleSetData.Length -1;
            _ViewStyleset_Control(-1);

            Action<string> RightSwipeGestureHandler = _ViewStyleset_SwipeGestureRight;
            Action<string> LeftSwipeGestureHandler = _ViewStyleset_SwipeGestureLeft;
            KinectController.ChangeGestureEventHandler(RightSwipeGestureHandler, "R");
            KinectController.ChangeGestureEventHandler(LeftSwipeGestureHandler, "L");

            _Global_C_ShowWaitingScreen();
            ViewStylesetControl.btnProductDetail.Click += new RoutedEventHandler(_ViewStyleset_P_RightHandUp);
            ViewStylesetControl.btnRetakePicture.Click += new RoutedEventHandler(_ViewStyleset_P_LeftHandUp);
            ViewStylesetControl.btnTakeCupon.Click += new RoutedEventHandler(_ViewStyleset_P_HeadUp);
        }

        private void _ViewStyleset_Control(int act)
        {
            if (act != 0)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    ViewStylesetControl.imgGuideline.Visibility = System.Windows.Visibility.Hidden;
                }));
            }
            CurrentStylesetIdx += act;
            if (CurrentStylesetIdx < 0)
                CurrentStylesetIdx = StyletIdexLast;
            if (CurrentStylesetIdx > StyletIdexLast)
                CurrentStylesetIdx = 0;
            _Global_C_ShowWaitingScreen();
            RESTful RestObj = new RESTful();
            IRestResponse<_REST_StyleSetListInfo> RESTUserLookList = RestObj.RESTgetStyleSetListByUserId(StyleSetData[CurrentStylesetIdx].id);
            if (RESTUserLookList.Data != null)
            {
                UserLookList = RESTUserLookList.Data.ToArray();
                ViewStylesetControl.imgUserLook1.MouseDown += new System.Windows.Input.MouseButtonEventHandler(_ViewStyleset_C_UserLook1);
                ViewStylesetControl.imgUserLook2.MouseDown += new System.Windows.Input.MouseButtonEventHandler(_ViewStyleset_C_UserLook2);
                ViewStylesetControl.imgUserLook3.MouseDown += new System.Windows.Input.MouseButtonEventHandler(_ViewStyleset_C_UserLook3);
            }

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
            {
                ViewStylesetControl.imgSelectedStyleset.Source = new BitmapImage(new Uri(StyleSetData[CurrentStylesetIdx].imageUrl));
                ViewStylesetControl.lblLikeCount.Content = StyleSetData[CurrentStylesetIdx].likeCount;
                if (UserLookList != null)
                {
                    ViewStylesetControl.imgUserLook1.Source = new BitmapImage(new Uri(UserLookList[0].imageUrl));
                    ViewStylesetControl.imgUserLook2.Source = new BitmapImage(new Uri(UserLookList[1].imageUrl));
                    ViewStylesetControl.imgUserLook3.Source = new BitmapImage(new Uri(UserLookList[2].imageUrl));
                }
                else
                {
                    ViewStylesetControl.imgUserLook1.Source = new BitmapImage();
                    ViewStylesetControl.imgUserLook2.Source = new BitmapImage();
                    ViewStylesetControl.imgUserLook3.Source = new BitmapImage();
                }
                if(CurrentStylesetIdx + 1 > StyletIdexLast)
                    ViewStylesetControl.imgimgRightStyleset.Source = new BitmapImage(new Uri(StyleSetData[0].imageUrl));
                else
                    ViewStylesetControl.imgimgRightStyleset.Source = new BitmapImage(new Uri(StyleSetData[CurrentStylesetIdx + 1].imageUrl));
                if (CurrentStylesetIdx -1 < 0)
                    ViewStylesetControl.imgLeftStyleset.Source = new BitmapImage(new Uri(StyleSetData[0].imageUrl));
                else
                    ViewStylesetControl.imgLeftStyleset.Source = new BitmapImage(new Uri(StyleSetData[CurrentStylesetIdx - 1].imageUrl));
            }));
            _Global_C_ShowWaitingScreen();
        }

        private void _ViewStyleset_C_UserLook1(object sender, EventArgs e)
        {
            if (PageControl.getGrid(Window.GetWindow(this)).Children[PageControl.getGrid(Window.GetWindow(this)).Children.Count - 1] != sProductExtraInfoControl)
            {
                KinectController.RemoveGestureEventHandler();
                sProductExtraInfoControl = new StylesetProductInfo(UserLookList[0].id);
                _sProductExtraInfo_Initialize(UserLookList[0]);
                _Global_C_AddControl(sProductExtraInfoControl);
            }
        }

        private void _ViewStyleset_C_UserLook2(object sender, EventArgs e)
        {
            if (PageControl.getGrid(Window.GetWindow(this)).Children[PageControl.getGrid(Window.GetWindow(this)).Children.Count - 1] != sProductExtraInfoControl)
            {
                KinectController.RemoveGestureEventHandler();
                sProductExtraInfoControl = new StylesetProductInfo(UserLookList[1].id);
                _sProductExtraInfo_Initialize(UserLookList[1]);
                _Global_C_AddControl(sProductExtraInfoControl);
            }
        }

        private void _ViewStyleset_C_UserLook3(object sender, EventArgs e)
        {
            if (PageControl.getGrid(Window.GetWindow(this)).Children[PageControl.getGrid(Window.GetWindow(this)).Children.Count - 1] != sProductExtraInfoControl)
            {
                KinectController.RemoveGestureEventHandler();
                sProductExtraInfoControl = new StylesetProductInfo(UserLookList[2].id);
                _sProductExtraInfo_Initialize(UserLookList[2]);
                _Global_C_AddControl(sProductExtraInfoControl);
            }
        }

        private void _ViewStyleset_SwipeGestureRight(string gesture)
        {
            /*
             * 
             *  Gesture Action
             *   - SwipeToLeft
             *   - SwipeToRight
             *   
             * */
            switch (gesture)
            {
                case "SwipeToLeft": _ViewStyleset_Control(1); break;
                case "SwipeToRight": break;
            }
        }

        private void _ViewStyleset_SwipeGestureLeft(string gesture)
        {
            /*
             * 
             *  Gesture Action
             *   - SwipeToLeft
             *   - SwipeToRight
             *   
             * */
            switch (gesture)
            {
                case "SwipeToLeft": break;
                case "SwipeToRight": _ViewStyleset_Control(-1); break;
            }
        }

        private void _ViewStyleset_P_LeftHandUp(object sender, EventArgs e)
        {
            KinectController.RemoveGestureEventHandler();
            TakePictureControl = new TakePicture();
            _Global_C_AddControl(TakePictureControl);
            _TakePicture_Initialize(0);
            _Global_C_RemoveControl(ViewStylesetControl);
            ViewStylesetControl.Dispose();
            ViewStylesetControl = null;
        }

        private void _ViewStyleset_P_RightHandUp(object sender, EventArgs e)
        {
            KinectController.RemoveGestureEventHandler();
            sProductInfoControl = new StylesetProductInfo(CurrentStylesetIdx);
            _sProductInfo_Initialize(StyleSetData[CurrentStylesetIdx]);
            _Global_C_AddControl(sProductInfoControl);
        }

        private void _ViewStyleset_P_HeadUp(object sender, EventArgs e)
        {
            KinectController.RemoveGestureEventHandler();
            SaleCuponControl = new SaleCupon(ViewStylesetControl.SelectedStyleset);
            _SaleCupon_Initialize();
            _Global_C_AddControl(SaleCuponControl);
        }

        #endregion

        #region StylesetProductInfoFunc

        private void _sProductInfo_Initialize(_REST_StyleSetInfo MainStyleset)
        {
            CurrentStylesetUid = MainStyleset.id;
            RESTful RestObj = new RESTful();
            _REST_ProductInfo product1 = MainStyleset.look;
            _REST_ProductInfo product2 = null;
            IRestResponse<_REST_StyleSetInfo> SubStyleset;
            _REST_ProductInfo MainProduct = product1;
            _REST_ProductInfo SubProduct = null;
            if (MainStyleset.matchUserLookId != 0)
            {
                SubStyleset = RestObj.RESTgetStyleSetById(MainStyleset.matchUserLookId);
                product2 = SubStyleset.Data.look;
                SubProduct = product2;
            }

            if (MainProduct != null)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    sProductInfoControl.imgStylesetImage.Source = new BitmapImage(new Uri(MainStyleset.imageUrl));
                    sProductInfoControl.lblMainProductName.Content = MainProduct.name;
                    sProductInfoControl.imgMainProduct.Source = new BitmapImage(new Uri(MainProduct.imageUrl));
                    sProductInfoControl.lblMainProductPrice.Content = MainProduct.price;
                    if (SubProduct != null)
                    {
                        sProductInfoControl.lblSubProductName.Content = SubProduct.name;
                        sProductInfoControl.imgSubProduct.Source = new BitmapImage(new Uri(SubProduct.imageUrl));
                        sProductInfoControl.lblSubProductPrice.Content = SubProduct.price;
                    }
                }));
            }
            else
            {

            }
            sProductInfoControl.btnLike.Click += new RoutedEventHandler(_sProductInfo_P_LeftHandUp);
            sProductInfoControl.btnClose.Click += new RoutedEventHandler(_sProductInfo_P_RightHandUp);
        }

        private void _sProductInfo_P_RightHandUp(object sender, EventArgs e)
        {
            _Global_C_RemoveControl(sProductInfoControl);
            sProductInfoControl.Dispose();
            sProductInfoControl = null;
            Action<string> RightSwipeGestureHandler = _ViewStyleset_SwipeGestureRight;
            Action<string> LeftSwipeGestureHandler = _ViewStyleset_SwipeGestureLeft;
            KinectController.ChangeGestureEventHandler(RightSwipeGestureHandler, "R");
            KinectController.ChangeGestureEventHandler(LeftSwipeGestureHandler, "L");
        }

        private void _sProductInfo_P_LeftHandUp(object sender, EventArgs e)
        {
            RESTful RestObj = new RESTful();
            RestObj.RESTsetLikeStyleset(CurrentStylesetUid);
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
            {
                _NoticeScreen_Initialize("보고 있는 코디를 '좋아요' 하였습니다.");
                sProductInfoControl.btnLike.Visibility = System.Windows.Visibility.Hidden;
                sProductInfoControl.imgLike.Visibility = System.Windows.Visibility.Hidden;
            }));
        }

        #endregion

        #region StylesetProductExtraInfoFunc

        private void _sProductExtraInfo_Initialize(_REST_StyleSetInfo MainStyleset)
        {
            CurrentStylesetUid = MainStyleset.id;
            RESTful RestObj = new RESTful();
            _REST_ProductInfo product1 = MainStyleset.look;
            _REST_ProductInfo product2 = null;
            IRestResponse<_REST_StyleSetInfo> SubStyleset;
            _REST_ProductInfo MainProduct = product1;
            _REST_ProductInfo SubProduct = null;
            if (MainStyleset.matchUserLookId != 0)
            {
                SubStyleset = RestObj.RESTgetStyleSetById(MainStyleset.matchUserLookId);
                product2 = SubStyleset.Data.look;
                SubProduct = product2;
            }

            if (MainProduct != null)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    sProductExtraInfoControl.imgStylesetImage.Source = new BitmapImage(new Uri(MainStyleset.imageUrl));
                    sProductExtraInfoControl.lblMainProductName.Content = MainProduct.name;
                    sProductExtraInfoControl.imgMainProduct.Source = new BitmapImage(new Uri(MainProduct.imageUrl));
                    sProductExtraInfoControl.lblMainProductPrice.Content = MainProduct.price;
                    if (SubProduct != null)
                    {
                        sProductExtraInfoControl.lblSubProductName.Content = SubProduct.name;
                        sProductExtraInfoControl.imgSubProduct.Source = new BitmapImage(new Uri(SubProduct.imageUrl));
                        sProductExtraInfoControl.lblSubProductPrice.Content = SubProduct.price;
                    }
                }));
            }
            else
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    _NoticeScreen_Initialize("코디 정보를 찾을 수 없습니다.");
                }));
                _sProductInfo_P_RightHandUp(null, null);
            }
            sProductExtraInfoControl.btnLike.Click += new RoutedEventHandler(_sProductExtraInfo_P_LeftHandUp);
            sProductExtraInfoControl.btnClose.Click += new RoutedEventHandler(_sProductExtraInfo_P_RightHandUp);
        }

        private void _sProductExtraInfo_P_RightHandUp(object sender, EventArgs e)
        {
            _Global_C_RemoveControl(sProductExtraInfoControl);
            sProductExtraInfoControl.Dispose();
            sProductExtraInfoControl = null;
            Action<string> RightSwipeGestureHandler = _ViewStyleset_SwipeGestureRight;
            Action<string> LeftSwipeGestureHandler = _ViewStyleset_SwipeGestureLeft;
            KinectController.ChangeGestureEventHandler(RightSwipeGestureHandler, "R");
            KinectController.ChangeGestureEventHandler(LeftSwipeGestureHandler, "L");
        }

        private void _sProductExtraInfo_P_LeftHandUp(object sender, EventArgs e)
        {
            RESTful RestObj = new RESTful();
            RestObj.RESTsetLikeStyleset(CurrentStylesetUid);
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    _NoticeScreen_Initialize("보고 있는 코디를 '좋아요' 하였습니다.");
                    sProductExtraInfoControl.btnLike.Visibility = System.Windows.Visibility.Hidden;
                    sProductExtraInfoControl.imgLike.Visibility = System.Windows.Visibility.Hidden;
                }));
        }

        #endregion

        #region SaleCuponFunc

        private void _SaleCupon_Initialize()
        {
            QRCodeWriter QRWriter = new QRCodeWriter();
            ByteMatrix QRObject = QRWriter.encode("http://www.curfit.com/mobile/" + cuponhash.Data.hash , BarcodeFormat.QR_CODE, 600, 600);
            SaleCuponControl.imgQRCode.Source = getImageSource(QRObject.ToBitmap());

            SaleCuponControl.btnClose.Click += new RoutedEventHandler(_SaleCupon_P_RightHandUp);
            SaleCuponControl.btnReturn.Click += new RoutedEventHandler(_SaleCupon_P_LeftHandUp);

        }

        private void _SaleCupon_P_RightHandUp(object sender, EventArgs e)
        {
            MainPageControl = new MainPageComponent();
            _Main_Initialize();
            _Global_C_AddControl(MainPageControl);
            _Global_C_RemoveControl(SaleCuponControl);
            SaleCuponControl.Dispose();
            SaleCuponControl = null;
        }

        private void _SaleCupon_P_LeftHandUp(object sender, EventArgs e)
        {
            _Global_C_RemoveControl(SaleCuponControl);
            SaleCuponControl.Dispose();
            SaleCuponControl = null;
            Action<string> RightSwipeGestureHandler = _ViewStyleset_SwipeGestureRight;
            Action<string> LeftSwipeGestureHandler = _ViewStyleset_SwipeGestureLeft;
            KinectController.ChangeGestureEventHandler(RightSwipeGestureHandler, "R");
            KinectController.ChangeGestureEventHandler(LeftSwipeGestureHandler, "L");
        }

        #endregion

        #region NoticeDialogueFunc

        private void _NoticeScreen_Initialize(string Message)
        {
            NoticeControl = new NoticeScreen(Message);
            _Global_C_AddControl(NoticeControl);
            NoticeTimer = new DispatcherTimer();
            NoticeTimer.Interval = TimeSpan.FromSeconds(1);
            NoticeTimer.Tick += new EventHandler(_NoticeScreen_T);
            NoticeTimer.Start();
        }

        private void _NoticeScreen_T(object sender, EventArgs e)
        {
            NoticeTimeSec--;
            if (NoticeTimeSec == 0)
            {
                NoticeTimer.Stop();
                NoticeTimeSec = 3;
                _Global_C_RemoveControl(NoticeControl);
                NoticeControl.Dispose();
                NoticeControl = null;
            }
        }
        
        #endregion

        #region GlobalFunc

        private void _Global_C_AddControl(UserControl targetControl)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    PageControl.thisWindow = Window.GetWindow(this);
                    Grid MainGrid = PageControl.getGrid(Window.GetWindow(this));

                    MainGrid.Children.Add(targetControl);
                    this.UpdateLayout();
                }));
        }

        private void _Global_C_RemoveControl(UserControl targetControl)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    Grid MainGrid = PageControl.getGrid(Window.GetWindow(this));

                    MainGrid.Children.Remove(targetControl);
                    this.UpdateLayout();
                }));
        }

        private void _Global_C_ShowWaitingScreen()
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    LoadingAdorner.IsAdornerVisible = !LoadingAdorner.IsAdornerVisible;
                }));
        }

        private void _Global_C_ShowWaitingScreen(string Message)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
            {
                LoadingAdorner.IsAdornerVisible = !LoadingAdorner.IsAdornerVisible;
                
            }));
        }

        #endregion
    }
}

