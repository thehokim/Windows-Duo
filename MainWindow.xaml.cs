using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WindowsDuo
{
    public partial class MainWindow : Window
    {
        // P/Invoke
        [DllImport("User32", SetLastError = true)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);
        [DllImport("User32", SetLastError = true)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

        private Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid("BA3E0F4D-B817-4094-A2D1-D56379E6A0F3");
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        private const int WM_POWERBROADCAST = 0x0218;
        private const int PBT_POWERSETTINGCHANGE = 0x8013;

        private IntPtr _powerNotificationHandle;

        public MainWindow()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden; // Hide initially
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Calculate screen aspect ratio to adjust the 3D plane width perfectly
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double ratio = screenWidth / screenHeight;
            
            // Adjust the 3D mesh width based on ratio
            var positions = MyMesh.Positions;
            positions[0] = new System.Windows.Media.Media3D.Point3D(-ratio, 1, 0);
            positions[1] = new System.Windows.Media.Media3D.Point3D(-ratio, -1, 0);
            positions[2] = new System.Windows.Media.Media3D.Point3D(ratio, -1, 0);
            positions[3] = new System.Windows.Media.Media3D.Point3D(ratio, 1, 0);
            MyMesh.Positions = positions;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(WndProc);
                _powerNotificationHandle = RegisterPowerSettingNotification(hwndSource.Handle, ref GUID_LIDSWITCH_STATE_CHANGE, DEVICE_NOTIFY_WINDOW_HANDLE);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_powerNotificationHandle != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(_powerNotificationHandle);
            }
            base.OnClosed(e);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_POWERBROADCAST && wParam.ToInt32() == PBT_POWERSETTINGCHANGE)
            {
                POWERBROADCAST_SETTING setting = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
                if (setting.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                {
                    bool isLidOpen = setting.Data[0] != 0;
                    if (!isLidOpen)
                    {
                        TriggerLidCloseAnimation();
                    }
                    else
                    {
                        TriggerLidOpenAnimation();
                    }
                }
            }
            return IntPtr.Zero;
        }

        private void TriggerLidCloseAnimation()
        {
            // 1. Capture the screen before the window is visible
            CaptureScreenToBrush();
            
            // 2. Make window visible
            this.Visibility = Visibility.Visible;
            
            // 3. Play animation
            var sb = new Storyboard();
            
            var rotAnim = new DoubleAnimation(0, 50, TimeSpan.FromSeconds(1.5));
            rotAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTarget(rotAnim, MyRotation);
            Storyboard.SetTargetProperty(rotAnim, new PropertyPath("Angle"));
            
            var blurAnim = new DoubleAnimation(0, 30, TimeSpan.FromSeconds(1.5));
            Storyboard.SetTarget(blurAnim, MyBlurEffect);
            Storyboard.SetTargetProperty(blurAnim, new PropertyPath("Radius"));
            
            var darkAnim = new DoubleAnimation(0, 0.7, TimeSpan.FromSeconds(1.5));
            Storyboard.SetTarget(darkAnim, DarkOverlay);
            Storyboard.SetTargetProperty(darkAnim, new PropertyPath("Opacity"));

            sb.Children.Add(rotAnim);
            sb.Children.Add(blurAnim);
            sb.Children.Add(darkAnim);
            sb.Begin();
        }

        private void TriggerLidOpenAnimation()
        {
            var sb = new Storyboard();
            
            var rotAnim = new DoubleAnimation(50, 0, TimeSpan.FromSeconds(1.5));
            rotAnim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTarget(rotAnim, MyRotation);
            Storyboard.SetTargetProperty(rotAnim, new PropertyPath("Angle"));
            
            var blurAnim = new DoubleAnimation(30, 0, TimeSpan.FromSeconds(1.5));
            Storyboard.SetTarget(blurAnim, MyBlurEffect);
            Storyboard.SetTargetProperty(blurAnim, new PropertyPath("Radius"));
            
            var darkAnim = new DoubleAnimation(0.7, 0, TimeSpan.FromSeconds(1.5));
            Storyboard.SetTarget(darkAnim, DarkOverlay);
            Storyboard.SetTargetProperty(darkAnim, new PropertyPath("Opacity"));

            sb.Children.Add(rotAnim);
            sb.Children.Add(blurAnim);
            sb.Children.Add(darkAnim);
            
            sb.Completed += (s, e) => { this.Visibility = Visibility.Hidden; };
            sb.Begin();
        }

        private void CaptureScreenToBrush()
        {
            int width = (int)SystemParameters.PrimaryScreenWidth;
            int height = (int)SystemParameters.PrimaryScreenHeight;
            
            using (var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }
                
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = ms;
                    bi.EndInit();
                    ScreenBrush.ImageSource = bi;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] Data;
        }
    }
}
