using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace UE_2Hour
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // WinAPI функции
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private static System.Timers.Timer timer;
        private static TimeSpan activeTime = TimeSpan.Zero;
        private static DateTime lastCheckTime;
        private static DateTime? activeStartTime;
        private static List<string> activeIntervals = new List<string>();
        private static string documentsPath;
        private static string logFilePath;

        public MainWindow()
        {
            InitializeComponent();

            // Путь к документам пользователя
            documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            logFilePath = System.IO.Path.Combine(documentsPath, "UE5_ActiveTimeLog.txt");

            timer = new System.Timers.Timer(1000); // Проверка каждую секунду
            timer.Elapsed += CheckActiveWindow;
            timer.Start();

            lastCheckTime = DateTime.Now;

            // Обработка закрытия окна
            this.Closing += MainWindow_Closing;
        }

        private void CheckActiveWindow(object sender, ElapsedEventArgs e)
        {
            IntPtr handle = GetForegroundWindow();
            StringBuilder windowText = new StringBuilder(256);
            GetWindowText(handle, windowText, 256);

            bool isUE5Active = windowText.ToString().IndexOf("Visual", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isUE5Active)
            {
                if (activeStartTime == null)
                {
                    activeStartTime = DateTime.Now;
                }

                activeTime += DateTime.Now - lastCheckTime;

                Dispatcher.Invoke(() =>
                {
                    ActiveTimeTextBlock.Text = $"Активное время в UE5: {activeTime:hh\\:mm\\:ss}";
                });
            }
            else
            {
                if (activeStartTime != null)
                {
                    string interval = $"({activeStartTime:HH:mm:ss} - {DateTime.Now:HH:mm:ss}): {DateTime.Now - activeStartTime:hh\\:mm\\:ss}";
                    activeIntervals.Add(interval);
                    activeStartTime = null;
                }
            }

            lastCheckTime = DateTime.Now;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (activeStartTime != null)
            {
                string interval = $"({activeStartTime:HH:mm:ss} - {DateTime.Now:HH:mm:ss}): {DateTime.Now - activeStartTime:hh\\:mm\\:ss}";
                activeIntervals.Add(interval);
            }
            WriteLogToFile();
        }

        private void WriteLogToFile()
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now:dd-MM-yyyy} => {activeTime:hh\\:mm\\:ss}:");
                foreach (var interval in activeIntervals)
                {
                    writer.WriteLine(interval);
                }
                writer.WriteLine();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
