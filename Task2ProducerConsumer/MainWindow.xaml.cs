using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace ProducerConsumer
{
    public partial class MainWindow : Window
    {
        private Queue<int> queue = new Queue<int>();
        private object locker = new object();
        private bool running = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            log.Clear();
            running = true;

            Thread producer = new Thread(Producer);
            Thread consumer = new Thread(Consumer);

            producer.Start();
            consumer.Start();

            new Thread(() =>
            {
                Thread.Sleep(10000);
                running = false;
                lock (locker)
                {
                    Monitor.PulseAll(locker);
                }
                Dispatcher.Invoke(() => log.AppendText("\nРабота завершена ✅"));
            }).Start();
        }

        void Producer()
        {
            int counter = 1;
            while (running)
            {
                lock (locker)
                {
                    queue.Enqueue(counter);
                    Dispatcher.Invoke(() => log.AppendText($"\nПроизводитель добавил {counter}"));
                    counter++;
                    Monitor.Pulse(locker);
                }
                Thread.Sleep(500);
            }
        }

        void Consumer()
        {
            while (running)
            {
                int item = -1;
                lock (locker)
                {
                    while (queue.Count == 0 && running)
                        Monitor.Wait(locker);

                    if (queue.Count > 0)
                        item = queue.Dequeue();
                }

                if (item != -1)
                {
                    Dispatcher.Invoke(() => log.AppendText($"\nПотребитель обработал {item}"));
                    Thread.Sleep(1000);
                }
            }
        }
    }
}