using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ddrescure_for_Windows
{
    /// <summary>
    /// FirstSetup.xaml の相互作用ロジック
    /// </summary>
    public partial class FirstSetup : Window
    {
        public FirstSetup()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FileDownloader fileDownloader = new FileDownloader();
            var m = await fileDownloader.GetContent("https://github.com/MachinaCore/CygwinPortable/releases/download/1.4.0.0/CygwinPortable_1.4.0.0.paf.exe");
            try
            {
                using (FileStream fs = new FileStream(@".\Cygwin.exe", FileMode.Create))
                {
                    //ファイルに書き込む
                    m.WriteTo(fs);
                    m.Close();
                }
                MessageBox.Show("インストーラーが起動します。\n指示に従ってそのままインストールしてください。");

                ProcessStartInfo pi = new ProcessStartInfo()
                {
                    FileName = @".\Cygwin.exe",
                    //Arguments = "App",
                    UseShellExecute = true,
                };
                var res = Process.Start(pi);
                res.WaitForExit();
                Debug.WriteLine(res.ExitCode);
                ProcessStartInfo pi1 = new ProcessStartInfo()
                {
                    FileName = @".\CygwinPortable\CygwinPortable.exe",
                    //Arguments = "App",
                    UseShellExecute = true,
                };
                var res1 = Process.Start(pi1);
                res1.WaitForExit();
                Debug.WriteLine(res1.ExitCode);
                MessageBox.Show($"Download Cygwin X64をクリックしてください。\nダウンロードが終わると管理者権限を求められるので、許可をしてください。\n自動的にインストールが開始します。\nインストールが終わったらこのダイアログのOKを押してください。");
                using (StreamWriter sw = new StreamWriter(@".\CygwinPortable\App\Runtime\Cygwin\Setup.bat", false))
                {
                    //wget https://raw.githubusercontent.com/ToaRuGakusei/apt-cyg_patch/main/apt-cyg & chmod 755 apt-cyg & mv apt-cyg /usr/local/bin/ & 
                    sw.WriteLine("@echo off\r\nsetlocal enableextensions\r\nset TERM=\r\ncd /d \"%~dp0bin\" && .\\bash --login -i -c \"apt-cyg -m https://ftp.iij.ad.jp/pub/cygwin/x86_64/ update & apt-cyg install ddrescue & apt-cyg -m https://ftp.iij.ad.jp/pub/cygwin/x86_64/ update & apt-cyg install ddrescue\"");
                }
                await Task.Delay(1000);
                ProcessStartInfo pi2 = new ProcessStartInfo()
                {
                    FileName = @".\CygwinPortable\App\Runtime\Cygwin\Setup.bat",
                    Arguments = "Hello",
                    UseShellExecute = true,
                    
                };
                var res2 = Process.Start(pi2);
                //Debug.WriteLine(res2.StandardOutput);
                res2.WaitForExit();
                Debug.WriteLine(res2.ExitCode);
                var res3 = Process.Start(pi2);
                //Debug.WriteLine(res2.StandardOutput);
                res3.WaitForExit();
                Debug.WriteLine(res3.ExitCode);
                MessageBox.Show("インストール完了");
                this.Close();

            }
            catch (Exception)
            {

            }
        }
    }
}
