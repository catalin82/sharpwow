using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SharpWoW.Game
{
    public class ThreadManager
    {
        public Thread LaunchThread(ThreadStart ts)
        {
            Thread t = new Thread(
                () =>
                {
                    try
                    {
                        ts();
                    }
                    catch (ThreadAbortException)
                    {
                    }

                    lock (mThreadList) mThreadList.Remove(Thread.CurrentThread);
                }
            );

            lock (mThreadList) mThreadList.Add(t);
            t.Start();
            return t;
        }

        public void Shutdown()
        {
            lock (mThreadList)
            {
                foreach (var thread in mThreadList)
                    thread.Abort();
            }


        }

        private List<Thread> mThreadList = new List<Thread>();
    }
}
