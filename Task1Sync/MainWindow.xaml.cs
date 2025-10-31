using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace SyncDemo
{
    public partial class MainWindow : Window
    {
        private List<int> numbers = new List<int>();
        private object locker = new object();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Race_Click(object sender, RoutedEventArgs e)
        {
            output.Clear();
            numbers.Clear();

            Thread t1 = new Thread(AddNumbersUnsafe);
            Thread t2 = new Thread(AddNumbersUnsafe);
            t1.Start();
            t2.Start();

            new Thread(() =>
            {
                t1.Join();
                t2.Join();
                Dispatcher.Invoke(() => output.Text += $"Всего элементов: {numbers.Count}\n");
            }).Start();
        }

        void AddNumbersUnsafe()
        {
            for (int i = 0; i < 1000; i++)
                numbers.Add(i);
        }

        private void SafeAdd_Click(object sender, RoutedEventArgs e)
        {
            output.Clear();
            numbers.Clear();

            Thread t1 = new Thread(AddNumbersSafe);
            Thread t2 = new Thread(AddNumbersSafe);
            t1.Start();
            t2.Start();

            new Thread(() =>
            {
                t1.Join();
                t2.Join();
                Dispatcher.Invoke(() => output.Text += $"Всего элементов: {numbers.Count}\n");
            }).Start();
        }

        void AddNumbersSafe()
        {
            for (int i = 0; i < 1000; i++)
            {
                lock (locker)
                {
                    numbers.Add(i);
                }
            }
        }

        private void MonitorTest_Click(object sender, RoutedEventArgs e)
        {
            output.Clear();

            Thread t = new Thread(() =>
            {
                bool gotLock = false;
                try
                {
                    gotLock = Monitor.TryEnter(locker, 1000);
                    if (gotLock)
                    {
                        Dispatcher.Invoke(() => output.Text = "Монитор: доступ получен ✅");
                    }
                    else
                    {
                        Dispatcher.Invoke(() => output.Text = "Монитор: таймаут ⏳");
                    }
                }
                finally
                {
                    if (gotLock) Monitor.Exit(locker);
                }
            });

            t.Start();
        }
    }
}