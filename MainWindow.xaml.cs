using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using System.Windows;
using TodaysAlphaReactor.Properties;

namespace TodaysAlphaReactor
{
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            RestoreWindowBounds();

            var encodedQueryString = Uri.EscapeDataString(Properties.Resources.QueryString);
            var today = DateTime.Today;
            var month = today.Month;
            var day = today.Day;
            var url = $"https://twitter.com/search?q={encodedQueryString}%20{month}%2F{day}&src=typed_query";

            wvMain.Source = new Uri(url);

            _ = InitializeAsync();
            wvMain.NavigationStarting += WvMain_NavigationStarting;
            this.Closed += MainWindow_Closed;
        }

        private async Task InitializeAsync()
        {
            await wvMain.EnsureCoreWebView2Async(null);
            wvMain.CoreWebView2.NewWindowRequested += WvMain_NewWindowRequested;
        }

        private void RestoreWindowBounds()
        {
            var settings = Settings.Default;
            if (settings.WindowLeft >= 0 && (settings.WindowLeft + settings.WindowWidth) < SystemParameters.VirtualScreenWidth)
            {
                this.Left = settings.WindowLeft;
            }
            if (settings.WindowTop >= 0 && (settings.WindowTop + settings.WindowHeight) < SystemParameters.VirtualScreenHeight)
            {
                this.Top = settings.WindowTop;
            }
            if (settings.WindowWidth > 0 && settings.WindowWidth <= SystemParameters.WorkArea.Width)
            {
                this.Width = settings.WindowWidth;
            }
            if (settings.WindowHeight > 0 && settings.WindowHeight <= SystemParameters.WorkArea.Height)
            {
                this.Height = settings.WindowHeight;
            }
        }

        private void SaveWindowBounds()
        {
            var settings = Settings.Default;
            settings.WindowLeft = this.Left;
            settings.WindowTop = this.Top;
            settings.WindowWidth = this.Width;
            settings.WindowHeight = this.Height;
            settings.Save();
        }

        private void WvMain_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;

            wvMain.CoreWebView2.Navigate(e.Uri);
        }

        private void WvMain_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var uri = new Uri(e.Uri);
            if (!uri.Host.Contains("twitter"))
            {
                System.Diagnostics.Process.Start(uri.ToString());
                e.Cancel = true;
            }
        }
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            SaveWindowBounds();
        }

    }
}
