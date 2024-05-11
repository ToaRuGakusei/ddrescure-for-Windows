using System.Diagnostics;
using System.IO;
using System.Windows;
namespace ddrescue_for_Windows
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
        private bool isEnd = false;
        private void Notouch()
        {
            this.IsEnabled = false; //MainWindowの画面を無効化
            isEnd = false;
            Task.Run(() =>
            {
                while (true)
                {
                    if (isEnd)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            this.IsEnabled = true;
                        }));
                        break;
                    }
                }
            });
        }
        private async System.Threading.Tasks.Task cygwin(FileDownloader fld)
        {
            var m = await fld.GetContent("https://github.com/MachinaCore/CygwinPortable/releases/download/1.4.0.0/CygwinPortable_1.4.0.0.paf.exe");
            try
            {
                using (FileStream fs = new FileStream(@".\Cygwin.exe", FileMode.Create))
                {
                    //ファイルに書き込む
                    m.WriteTo(fs);
                    m.Close();
                }
                isEnd = true;

            }
            catch (Exception ex)
            {

            }
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileDownloader fileDownloader = new FileDownloader();
                DownloadNow dln = new DownloadNow();
                Notouch();
                dln.Show();
                await cygwin(fileDownloader);
                dln.Close();
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
                MessageBox.Show($"Download Cygwin X64をクリックしてください。\nダウンロードが終わると管理者権限を求められるので、許可をしてください。\n自動的にインストールが開始します。\nインストールが終わったらこのダイアログのOKを押してください。\nレジストリが～っていうポップアップが英語で出た場合、Cancelをクリックしておいてください。");
                using (StreamWriter sw = new StreamWriter(@".\CygwinPortable\App\Runtime\Cygwin\Setup.bat", false))
                {
                    //wget https://raw.githubusercontent.com/ToaRuGakusei/apt-cyg_patch/main/apt-cyg & chmod 755 apt-cyg & mv apt-cyg /usr/local/bin/ & 
                    sw.WriteLine("@echo off\r\nsetlocal enableextensions\r\nset TERM=\r\ncd /d \"%~dp0bin\" && .\\bash --login -i -c \"mkdir ~/tmp & wget -nc -P ~/tmp https://raw.githubusercontent.com/ToaRuGakusei/apt-cyg_patch/main/apt-cyg & chmod 755 ~/tmp/apt-cyg & ~/tmp/apt-cyg -m https://ftp.iij.ad.jp/pub/cygwin/x86_64/ update & ~/tmp/apt-cyg install ddrescue\"");
                }
                await Task.Delay(1000);
                ProcessStartInfo pi2 = new ProcessStartInfo()
                {
                    FileName = @".\CygwinPortable\App\Runtime\Cygwin\Setup.bat",
                    UseShellExecute = false,

                };
                while (!File.Exists(@".\CygwinPortable\App\Runtime\Cygwin\bin\ddrescue.exe"))
                {
                    var res2 = Process.Start(pi2);
                    //Debug.WriteLine(res2.StandardOutput);
                    res2.WaitForExit();
                    Debug.WriteLine(res2.ExitCode);

                }
                this.Close();

            }
            catch (Exception)
            {

            }
        }
    }
}
