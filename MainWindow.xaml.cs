using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Al_Updater
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public MainWindow()
        {
            InitializeComponent();
            Thread.Sleep(1000);
            if (!CheckConnection())
            {
                MessageBox.Show("Не удалось подключится к севреру!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            };
            Loading_Label.Content = "Проверка обновления";
            if (!CheckVersion())
            {
                Loading_Label.Content = "Загрузка обновления";
                DownloadUpdate();
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "launcher.downloading"))
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "launcher.exe")) File.Delete(AppDomain.CurrentDomain.BaseDirectory + "launcher.exe");
                    File.Move(AppDomain.CurrentDomain.BaseDirectory + "launcher.downloading", AppDomain.CurrentDomain.BaseDirectory + "launcher.exe");
                } else
                {
                    Loading_Label.Content = "Обновление не найдено";
                    Thread.Sleep(2000);
                    Close();
                    return;
                }
                Loading_Label.Content = "Успешно";
                Thread.Sleep(2000);
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "launcher.exe");
                Close();
            } else
            {
                Loading_Label.Content = "Обновление не найдено";
                Thread.Sleep(2000);
                Close();
            }
        }

        private bool CheckConnection()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.apiURL);
                request.Timeout = 3000;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public bool CheckVersion()
        {
            string ver = FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + "launcher.exe").ProductVersion;
            ver = ver.Substring(0, ver.LastIndexOf('.'));
            string result = GET("ver.php?v=" + ver);
            return result == "ok";
        }

        private string GET(string Data)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.apiURL + Data);
                //req.UserAgent = "SCO_Internet_Bot";
                WebResponse resp = req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                string Out = sr.ReadToEnd();
                sr.Close();
                return Out;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        private bool FileDownloader(string ServerFilePath, string LocalFilePath)
        {
            try
            {
                if (LocalFilePath.LastIndexOf('/') > 0)
                {
                    if (!Directory.Exists(LocalFilePath.Substring(0, LocalFilePath.LastIndexOf('/'))))
                    {
                        Directory.CreateDirectory(LocalFilePath.Substring(0, LocalFilePath.LastIndexOf('/')));
                    }
                }
                WebClient webClient = new WebClient();
                //webClient.Headers.Add("user-agent", "SCO_Internet_Bot");
                webClient.DownloadFile(Properties.Settings.Default.apiURL + ServerFilePath, AppDomain.CurrentDomain.BaseDirectory + LocalFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void DownloadUpdate()
        {
            string ver = FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + "launcher.exe").ProductVersion;
            ver = ver.Substring(0, ver.LastIndexOf('.'));
            string result = GET("ver.php?v=" + ver);
            FileDownloader(result, "launcher.downloading");
        }

        private void Content_Grid_MouseLeftButtonDown(object sender, EventArgs e)
        {
            DragMove();
        }
    }
}
