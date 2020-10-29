using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Star.Project.GUI.Data;

namespace Star.Project.GUI.Controls
{
    public class WindowControl : UserControl, IDisposable
    {
        private IntPtr appWin = IntPtr.Zero;

        private bool isDisposed = false;

        public bool IsCreated { get; private set; } = false;

        public WindowControl(IntPtr handle)
        {
            this.Loaded += HostControl_Loaded;
            this.Unloaded += HostControl_Unloaded;
            this.appWin = handle;
        }
        ~WindowControl()
        {
            this.Dispose();
        }

        private void HostControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsCreated) return;

            try
            {
                IsCreated = true;

                var helper = new WindowInteropHelper(Window.GetWindow(this));

                Win32API.SetParent(this.appWin, helper.Handle);
                Win32API.SetWindowLong(this.appWin, Win32API.GWL_STYLE, Win32API.WS_VISIBLE);

                UpdateSize(this.TransformToAncestor(App.Current.MainWindow).Transform(new Point(0, 0)));
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message + "Error");

                // 出错了，把自己隐藏起来
                this.Visibility = Visibility.Collapsed;
            }
        }


        private void HostControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Dispose();
        }

        public void UpdateSize( Point point)
        {
            if (this.appWin != IntPtr.Zero)
            {
                PresentationSource source = PresentationSource.FromVisual(this);

                var scaleX = 1D;
                var scaleY = 1D;
                if (source != null)
                {
                    scaleX = source.CompositionTarget.TransformToDevice.M11;
                    scaleY = source.CompositionTarget.TransformToDevice.M22;
                }

                var width = (int)(this.ActualWidth * scaleX);
                var height = (int)(this.ActualHeight * scaleY);
                //var vector = VisualTreeHelper.GetOffset(this);
                 

                Win32API.MoveWindow(appWin, (int)point.X, (int)point.Y, width, height, true);
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (appWin != IntPtr.Zero)
                    {
                        appWin = IntPtr.Zero;
                    }
                }

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
