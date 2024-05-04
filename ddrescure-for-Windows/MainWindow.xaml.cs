using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace ddrescure_for_Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            /*FirstSetup firstSetup = new FirstSetup();
            firstSetup.Show();*/
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
                                    infodisk += l + "\n";
                                    string[] split = l.Split(" ");
                                    string[] splitResult = new string[10];
                                    int i = 0;
                                    foreach (string s in split)
                                    {
                                        if (s != "")
                                        {
                                            splitResult[i] = s;
                                            Debug.WriteLine(splitResult[i]);
                                            i++;
                                        }
                                    }
                                    list.Add(new DiskInfo { major = splitResult[0], minor = splitResult[1], blocks = int.Parse(splitResult[2]), name = splitResult[3], winMounts = splitResult[4] });
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
        private async void ddrescueRun(string option)
        {
            await Task.Run(() =>
            {
                ProcessStartInfo si = new ProcessStartInfo(@".\CygwinPortable\App\Runtime\Cygwin\bin\ddrescue.exe", $"{option}");
                // ウィンドウ表示を完全に消したい場合
                si.CreateNoWindow = false;
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


                        }));
                        // プロセスが終了すると呼ばれる
                        ctoken.Cancel();
                    };
                    // プロセスの開始
                    proc.Start();
                    Task.WaitAll(
                        Task.Run(async () =>
                        {
                            int count = 0;
                            string[] buf = new string[6];

                            while (true)
                            {
                                var l = await proc.StandardOutput.ReadLineAsync();
                                Debug.WriteLine(l);
                                count++;
                                this.Dispatcher.Invoke((Action)(async () =>
                                {
                                    Prompt.Content += l + "\n";

                                }));
                                if (l == null)
                                {
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

        private async void getPartition_Click(object sender, RoutedEventArgs e)
        {
            listview.ItemsSource = await getPar();
        }

        private void listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview.SelectedItem != null)
            {
                before.Content = (listview.SelectedItems[0] as DiskInfo).name;
                if (listview.SelectedItems.Count == 2)
                {
                    after.Content = (listview.SelectedItems[1] as DiskInfo).name;
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
                        after.Content = Path.GetFileName(dlg.FileName);
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string moto = "/dev/" + (string)before.Content;
            string saki = "/dev/" + (string)after.Content;
            tmpOption += $"-f -r{BadRead.Text} ";
            if (image.IsChecked == true)
            {
                saki = imagePath;
                tmpOption += $"{moto} {saki}";
                ddrescueRun(tmpOption);
                tmpOption = "";
            }
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
    }
}