using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace ddrescue_for_Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //初回起動時にCygwin&ddrescueをセットアップ
            if (!File.Exists(@".\CygwinPortable\App\Runtime\Cygwin\bin\ddrescue.exe"))
            {
                FirstSetup firstSetup = new FirstSetup();
                firstSetup.ShowDialog();
            }
        }
        private string imagePath = "";
        private async Task<ObservableCollection<DiskInfo>> getPar()
        {
            string infodisk = "";
            ObservableCollection<DiskInfo> list = new ObservableCollection<DiskInfo>();
            using (StreamWriter sw = new StreamWriter(@".\CygwinPortable\App\Runtime\Cygwin\getDisk.bat", false))
            {
                sw.WriteLine("@echo off\r\nsetlocal enableextensions\r\nset TERM=\r\ncd /d \"%~dp0bin\" && .\\bash --login -i -c \"cat /proc/partitions\"");
            }
            await Task.Run(() =>
            {
                ProcessStartInfo si = new ProcessStartInfo(@".\CygwinPortable\App\Runtime\Cygwin\getDisk.bat");
                // ウィンドウ表示を完全に消したい場合
                si.CreateNoWindow = true;
                si.RedirectStandardError = false;
                si.RedirectStandardOutput = true;
                si.UseShellExecute = false;

                bool ok = false;
                using (var proc = new Process())
                using (var ctoken = new CancellationTokenSource())
                {

                    proc.EnableRaisingEvents = true;
                    proc.StartInfo = si;
                    // コールバックの設定
                    proc.Exited += (s, ev) =>
                    {
                        Console.WriteLine($"exited");
                        this.Dispatcher.Invoke((Action)(() =>
                        {


                        }));
                        // プロセスが終了すると呼ばれる
                        ctoken.Cancel();
                    };
                    // プロセスの開始
                    proc.Start();
                    Task.WaitAll(
                        Task.Run(() =>
                        {
                            while (true)
                            {
                                var l = proc.StandardOutput.ReadLine();
                                //Debug.WriteLine(l);
                                if (l == null)
                                {
                                    Debug.WriteLine(infodisk);
                                    break;
                                }
                                try
                                {
                                    infodisk += l + "\n"; //出力に改行を付加して格納
                                    string[] split = l.Split(" ");
                                    string[] splitResult = new string[10];
                                    int i = 0;
                                    foreach (string s in split)
                                    {
                                        //何か入っているとき実行
                                        if ((s != "" && s.Contains("Copying non-tried blocks... Pass 1 (forwards)")) || ok)
                                        {
                                            ok = true;
                                            splitResult[i] = s;
                                            Debug.WriteLine(splitResult[i]);
                                            i++;
                                        }
                                        if(s.Contains("Finished"))
                                        {
                                            run.Content = "実行";
                                            Title = "";
                                            cancel = true;
                                            image.IsEnabled = true;
                                            DirectAccess.IsEnabled = true;
                                            ReadErrorIgnore.IsEnabled = true;
                                            kuwashiku.IsEnabled = true;
                                            MessageBox.Show("終わり");
                                            break;
                                        }

                                    }
                                    list.Add(new DiskInfo { major = splitResult[0], minor = splitResult[1], blocks = int.Parse(splitResult[2]), name = splitResult[3], winMounts = splitResult[4] }); //listに追加
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }),
                        Task.Run(() =>
                        {
                            ctoken.Token.WaitHandle.WaitOne();
                            proc.WaitForExit();
                        })
                    );
                }
            });


            return list;
        }
        private bool cancel = false; //キャンセルかどうか。
        private async void ddrescueRun(string option)
        {
            try
            {
                await Task.Run(() =>
                {
                    ProcessStartInfo si = new ProcessStartInfo(@".\CygwinPortable\App\Runtime\Cygwin\bin\ddrescue.exe", $"{option}");
                    // ウィンドウ表示を完全に消したい場合
                    si.CreateNoWindow = true;
                    si.RedirectStandardError = false;
                    si.RedirectStandardOutput = true;
                    si.UseShellExecute = false;

                    using (var proc = new Process())
                    using (var ctoken = new CancellationTokenSource())
                    {

                        proc.EnableRaisingEvents = true;
                        proc.StartInfo = si;
                        // コールバックの設定
                        proc.Exited += (s, ev) =>
                        {
                            Console.WriteLine($"exited");
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                this.IsEnabled = true;

                            }));
                            // プロセスが終了すると呼ばれる
                            ctoken.Cancel();
                        };
                        // プロセスの開始
                        proc.Start();
                        bool syokika = false;
                        Task.WaitAll(
                            Task.Run(() =>
                            {
                                int count = 0;
                                ObservableCollection<string> buf = new ObservableCollection<string>();
                                while (true)
                                {
                                    string l = proc.StandardOutput.ReadLine();
                                    buf.Add(l);
                                    if (buf.Count == 8 && !syokika) //余計な部分を消す
                                    {
                                        buf.Clear();
                                        syokika = true;
                                    }
                                    else if (syokika)
                                    {
                                        if (buf.Count == 8)
                                        {
                                            string t = "";
                                            this.Dispatcher.Invoke((Action)(() =>
                                            {
                                                foreach (string s in buf)
                                                {
                                                    t += s + "\n";
                                                }
                                                Prompt.Text = t; //プロンプトを表示
                                                Title = buf[0];
                                            }));
                                            buf.Clear();
                                            t = "";
                                        }
                                        else if (buf.Count >= 10) //10行追加されたらすべて消す。
                                        {
                                            buf.Clear();
                                        }
                                    }


                                    Debug.WriteLine(l);
                                    count++;
                                    if (l.Contains("Finished"))
                                    {
                                        this.Dispatcher.Invoke((Action)(() =>
                                        {
                                            run.Content = "実行";
                                            cancel = true;
                                            image.IsEnabled = true;
                                            DirectAccess.IsEnabled = true;
                                            ReadErrorIgnore.IsEnabled = true;
                                            kuwashiku.IsEnabled = true;
                                            Title = "Finished!!";
                                            MessageBox.Show("終了しました!", "インフォメーション", MessageBoxButton.OK, MessageBoxImage.Information);
                                        }));
                                    }

                                    if (l == null || cancel == true)
                                    {
                                        proc.Kill();
                                        cancel = false;
                                        break;
                                    }
                                }
                            }),
                            Task.Run(() =>
                            {
                                ctoken.Token.WaitHandle.WaitOne();
                                proc.WaitForExit();
                            })
                        );
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private async void getPartition_Click(object sender, RoutedEventArgs e)
        {
            listview.ItemsSource = await getPar();
        }

        private void listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview.SelectedItem != null)
            {
                if (listview.SelectedItems.Count >= 3)
                {
                    listview.SelectedItems.Clear();
                }
                else
                {
                    before.Content = "/dev/" + (listview.SelectedItems[0] as DiskInfo).name;
                    if (listview.SelectedItems.Count == 2)
                    {
                        after.Content = "/dev/" + (listview.SelectedItems[1] as DiskInfo).name;
                    }
                }
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            if (image.IsChecked == true)
            {
                if (imagePath == "")
                {
                    var dlg = new CommonSaveFileDialog();
                    dlg.DefaultFileName = "Image.img";
                    dlg.Filters.Add(new CommonFileDialogFilter("IMG", "*.img"));
                    if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        after.Content = dlg.FileName;
                        imagePath = dlg.FileName;
                        listview.SelectionMode = SelectionMode.Single;
                    }
                    else
                    {
                        image.IsChecked = false;
                        imagePath = "";
                        listview.SelectionMode = SelectionMode.Multiple;
                    }
                }
            }
            else
            {
                listview.SelectionMode = SelectionMode.Multiple;
                imagePath = "";
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listview.ItemsSource = await getPar();
        }
        private int badTryNum = 0;
        private void BadRead_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(BadRead.Text, out badTryNum))
            {
                BadRead.Text = String.Empty;
                MessageBox.Show("数字を入力してください。");
            }


        }
        public string tmpOption = " ";
        public string log = "";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (listview.SelectedItems.Count == 0)
            {
                System.Windows.MessageBox.Show("ドライブを選択してください", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {

                string moto = (string)before.Content;
                string saki = (string)after.Content;
                DateTime dateTime = DateTime.Now;
                if ((string)run.Content == "実行")
                {
                    var first = System.Windows.MessageBox.Show("本当に実行しますか？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (first != MessageBoxResult.Cancel)
                    {
                        var second = System.Windows.MessageBox.Show("本当の本当に実行しますか？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if (first == MessageBoxResult.OK && second == MessageBoxResult.OK)
                        {
                            string dir = Path.GetDirectoryName(imagePath);
                            run.Content = "キャンセル";
                            cancel = false;
                            if (image.IsChecked == true)
                            {
                                saki = imagePath;
                                tmpOption += $"-f -r{BadRead.Text} ";

                                //log = $"{dir}\\{Path.GetFileName(imagePath)}.LOG";
                                tmpOption += $"{moto} {saki}";
                                ddrescueRun(tmpOption);
                                tmpOption = "";
                                image.IsEnabled = false;
                                DirectAccess.IsEnabled = false;
                                ReadErrorIgnore.IsEnabled = false;
                                kuwashiku.IsEnabled = false;
                            }
                            else
                            {
                                dir = Path.GetTempPath();
                                tmpOption += $"-f -r{BadRead.Text} ";

                                log = $"{dir}{Path.GetFileName(imagePath)}.LOG";
                                tmpOption += $"{moto} {saki}";
                                ddrescueRun(tmpOption);
                                tmpOption = "";
                                image.IsEnabled = false;
                                DirectAccess.IsEnabled = false;
                                ReadErrorIgnore.IsEnabled = false;
                                kuwashiku.IsEnabled = false;
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("キャンセルしました", "インフォメーション", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("キャンセルしました", "インフォメーション", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
                else
                {
                    run.Content = "実行";
                    Title = "";
                    cancel = true;
                    image.IsEnabled = true;
                    DirectAccess.IsEnabled = true;
                    ReadErrorIgnore.IsEnabled = true;
                    kuwashiku.IsEnabled = true;
                    System.Windows.MessageBox.Show("キャンセルしました", "インフォメーション", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void forceLock()
        {
            this.IsEnabled = false;
        }
        private void ReadErrorIgnore_Checked(object sender, RoutedEventArgs e)
        {
            tmpOption += "-n ";
        }

        private void DirectAccess_Checked(object sender, RoutedEventArgs e)
        {
            tmpOption += "-d ";
        }

        private void image_Unchecked(object sender, RoutedEventArgs e)
        {
            image.IsChecked = false;
            imagePath = "";
            listview.SelectionMode = SelectionMode.Multiple;
        }

        private void kuwashiku_Checked(object sender, RoutedEventArgs e)
        {
            tmpOption += "-v ";
        }

        private void kuwashiku_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private async void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            cancel = true;
            await Task.Delay(1000);
            Environment.Exit(0);
        }
    }
}