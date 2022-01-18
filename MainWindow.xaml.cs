using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;

namespace TodaysAlphaReactor
{
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Month Day Format
        /// </summary>
        private class MdFormat
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public string Format { get; private set; }
            public MenuItem MenuItem { get; private set; }

            public MdFormat(int id, string name, string format, MenuItem menuItem)
            {
                Id = id;
                Name = name;
                Format = format;
                MenuItem = menuItem;
            }
        }
        private MdFormat[] _MdFormats;

        /// <summary>
        /// constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            _MdFormats = new MdFormat[]
            {
                new MdFormat(0, Properties.Resources.MonthDayFormatMonthSlashDay, "M/d", MenuMonthDayFormatMonthSlashDay),
                new MdFormat(1, Properties.Resources.MonthDayFormatMonthHyphenDay, "M-d", MenuMonthDayFormatMonthHyphenDay),
                new MdFormat(2, Properties.Resources.MonthDayFormatDaySlashMonth, "d/M", MenuMonthDayFormatDaySlashMonth),
                new MdFormat(3, Properties.Resources.MonthDayFormatDayHyphenMonth, "d-M", MenuMonthDayFormatDayHyphenMonth),
            };
            // load settings
            LoadWindowBounds();
            bool aot = Properties.Settings.Default.AlwaysOnTop;
            Topmost = aot;
            MenuViewAot.IsChecked = aot;
            bool hideMenu = Properties.Settings.Default.HideMenu;
            SetMenuVisibility(hideMenu);
            var mdFormat = _MdFormats.FirstOrDefault(i => i.Id == Properties.Settings.Default.MdFormat);
            SetCheckedMonthDayFormatMenuItem(mdFormat);

            // WebView2 settings
            _ = InitializeCoreWebView2Async();
            wvMain.NavigationStarting += WvMain_NavigationStarting;
            wvMain.NavigationCompleted += WvMain_NavigationCompleted;
            SetUrl(mdFormat);
        }

        /// <summary>
        /// Load window bounds from settings.
        /// </summary>
        private void LoadWindowBounds()
        {
            var settings = Properties.Settings.Default;
            if (settings.WindowLeft >= 0)
            {
                this.Left = settings.WindowLeft;
            }
            if (settings.WindowTop >= 0)
            {
                this.Top = settings.WindowTop;
            }
            if (settings.WindowWidth > 0)
            {
                this.Width = settings.WindowWidth;
            }
            if (settings.WindowHeight > 0)
            {
                this.Height = settings.WindowHeight;
            }
        }

        /// <summary>
        /// Initialize CoreWebView2.
        /// It awaits EnsureCoreWebView2Async because the initialization of CoreWebView2 is asynchronous.
        /// </summary>
        /// <returns>Task</returns>
        private async Task InitializeCoreWebView2Async()
        {
            await wvMain.EnsureCoreWebView2Async(null);
            wvMain.CoreWebView2.NewWindowRequested += WvMain_NewWindowRequested;
        }

        /// <summary>
        /// Set URL of search AlphaReactor info on twitter.
        /// </summary>
        /// <param name="mdFormat">MonthDayFormat</param>
        private void SetUrl(MdFormat mdFormat)
        {
            var encodedQueryString = Uri.EscapeDataString(Properties.Resources.QueryString);
            var today = DateTime.Now;
            // AlphaReactors are repop at 4am.
            today = today.AddHours(-4);
            var md = today.ToString(mdFormat.Format);
            var encodedMd = Uri.EscapeDataString(md);
            var url = $"https://twitter.com/search?q={encodedQueryString}%20{encodedMd}&src=typed_query";
            wvMain.Source = new Uri(url);
        }

        private void SaveWindowBounds()
        {
            var settings = Properties.Settings.Default;
            settings.WindowLeft = this.Left;
            settings.WindowTop = this.Top;
            settings.WindowWidth = this.Width;
            settings.WindowHeight = this.Height;
            settings.Save();
        }

        private void WvMain_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // suppresses opening in a new window.
            e.Handled = true;
            wvMain.CoreWebView2.Navigate(e.Uri);
        }

        private void WvMain_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var uri = new Uri(e.Uri);
            // when try to open a site out of twitter,
            if (!uri.Host.Contains("twitter"))
            {
                // launch the default browser.
                System.Diagnostics.Process.Start(uri.ToString());
                e.Cancel = true;
            }
        }

        private async void WvMain_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // hide the twitter's login bar.
            await wvMain.CoreWebView2.ExecuteScriptAsync("document.querySelector('#layers:first-child').style.display = 'none';");
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // control menu visibility when the HideMenu setting is true.
            if (Properties.Settings.Default.HideMenu)
            {
                // press alt key to toggle visibility.
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    if (Menu.Visibility == Visibility.Visible)
                    {
                        Menu.Visibility = Visibility.Hidden;
                        Menu.Height = 0;
                    }
                    else
                    {
                        Menu.Visibility = Visibility.Visible;
                        Menu.Height = double.NaN;
                    }
                }
                else if (Keyboard.IsKeyDown(Key.Escape))
                {
                    // press esc key to hide menu when the menu is visible.
                    if (Menu.Visibility == Visibility.Visible)
                    {
                        Menu.Visibility = Visibility.Hidden;
                        Menu.Height = 0;
                    }
                }
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            SaveWindowBounds();
        }

        private void MenuItemQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemAot_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var aot = menuItem.IsChecked;
            var settings = Properties.Settings.Default;
            settings.AlwaysOnTop = aot;
            settings.Save();
            Topmost = aot;
        }

        private void MenuItemMdFormat_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var mdFormatName = Convert.ToString(menuItem.Header);
            var mdFormat = _MdFormats.FirstOrDefault(i => i.Name == mdFormatName);

            var settings = Properties.Settings.Default;
            settings.MdFormat = mdFormat.Id;
            settings.Save();
            SetUrl(mdFormat);
            SetCheckedMonthDayFormatMenuItem(mdFormat);
        }

        /// <summary>
        /// Do exclusive control of the MonthDayFormat menu.
        /// </summary>
        /// <param name="checkedMdFormat">current checked MonthDayFormat menu item</param>
        private void SetCheckedMonthDayFormatMenuItem(MdFormat checkedMdFormat)
        {
            foreach (var mdFormat in _MdFormats)
            {
                if (mdFormat.Equals(checkedMdFormat))
                {
                    mdFormat.MenuItem.IsChecked = true;
                }
                else
                {
                    mdFormat.MenuItem.IsChecked = false;
                }
            }
        }

        private void MenuHideMenu_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var hideMenu = menuItem.IsChecked;
            var settings = Properties.Settings.Default;
            settings.HideMenu = hideMenu;
            settings.Save();
            SetMenuVisibility(hideMenu);
        }

        /// <summary>
        /// Set menu visibility from HideMenu setting.
        /// </summary>
        /// <param name="hideMenu">HideMenu setting</param>
        private void SetMenuVisibility(bool hideMenu)
        {
            Menu.Visibility = hideMenu ? Visibility.Hidden : Visibility.Visible;
            if (hideMenu)
            {
                Menu.Height = 0;
            }
            else
            {
                // set double.NaN to set initial (auto) height
                Menu.Height = double.NaN;
            }
        }
    }
}
