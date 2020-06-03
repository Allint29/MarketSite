using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace MyWebSite.Tools
{
    public class ProducerConsQueue : IDisposable
    {
        private EventWaitHandle wh = new AutoResetEvent(false);
        private BackgroundWorker worker;
        object locker = new object();
        Queue<string> tasks = new Queue<string>();
        private DirectoryInfo dirInfo;
        private string task = null;
        private bool _isAlive;

        public ProducerConsQueue()
        {
            dirInfo = new DirectoryInfo($@"{Directory.GetCurrentDirectory()}\TestLogs");
            _isAlive = true;
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += bw_DoWork;
            worker.RunWorkerCompleted += bw_RunWorkerCompleted;
            worker.RunWorkerAsync(null);
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                Console.WriteLine("ProducerConsQueue: " + e.Error);
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (_isAlive)
            {
                lock (locker)
                {
                    task = null;
                    if (tasks.Count > 0)
                    {
                        task = tasks.Dequeue();
                    }
                }

                if (task != null)
                {
                    //Работа
                    //task.ExecuteNonQuery();
                }
                else
                {
                    wh.WaitOne();
                }
            }
        }

        public void EnqueSafeSQLCommandQueue(string task)
        {
            lock (locker)
            {
                tasks.Enqueue(task);
            }

            try
            {
                wh.Set();
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e);
               // throw;
            }
        }

        public void Dispose()
        {
            wh?.Dispose();
            worker?.Dispose();
        }
    }
}
