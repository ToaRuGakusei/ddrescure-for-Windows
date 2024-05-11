using System.Runtime.InteropServices;
using System.Windows;
namespace ddrescue_for_Windows
{
    /// <summary>
    /// DownloadNow.xaml の相互作用ロジック
    /// </summary>
    public partial class DownloadNow : Window
    {
        public DownloadNow()
        {
            InitializeComponent();
            Bar.Maximum = 100;
            Bar.Minimum = 0;
        }
        /// <summary>
        /// メニューのハンドル取得
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="bRevert"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        /// <summary>
        /// メニュー項目の削除
        /// </summary>
        /// <param name="hMenu"></param>
        /// <param name="uPosition"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        private const int SC_CLOSE = 0xf060;

        /// <summary>
        /// uPositionに設定するのは項目のID
        /// </summary>
        private const int MF_BYCOMMAND = 0x0000;
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper((Window)sender).Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, false);
            RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
        }
        /// <summary>
        /// プログレスバー関連
        /// </summary>
        /// <returns></returns>
        private async Task LoopTask() => await Task.Run(() =>
                                            {
                                                bool end = false;
                                                string name = String.Empty;
                                                string DownloadNow = String.Empty;
                                                while (true)
                                                {
                                                    this.Dispatcher.Invoke((Action)(() =>
                                                    {
                                                        DownloadNow = FileDownloader.WhatName + "をダウンロード中";
                                                        Title = DownloadNow;
                                                        downloadRead.Content = DownloadNow;
                                                        downloadBytes.Content = (FileDownloader.TotalBytes) + "b";
                                                    }));
                                                    this.Dispatcher.Invoke((Action)(() =>
                                                    {
                                                        Bar.Value = FileDownloader.now;
                                                        downloadLabel.Content = $"{Bar.Value:F0}%";
                                                    }));
                                                    this.Dispatcher.Invoke((Action)(() =>
                                                    {
                                                        name = FileDownloader.WhatName;
                                                        end = FileDownloader.IsEnd;
                                                    }));
                                                    if (name.Contains("cygwin") && end == true)
                                                    {
                                                        break;
                                                    }

                                                }
                                            });

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoopTask();
        }
    }
}
