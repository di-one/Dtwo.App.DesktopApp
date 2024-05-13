using Dtwo.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtwo.App.DesktopApp
{
    public static class LogFile 
    {
        private static bool m_sessionIsCreated = false;
        private static FileStream m_fileStream;

        public static string DirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static Thread m_thread;
        private static ConcurrentQueue<LogMessage> m_logQueue = new ConcurrentQueue<LogMessage>();
        private static int m_updatedEntries;

        private static void CreateSession()
        {
            m_sessionIsCreated = true;
            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log";

            m_fileStream = new FileStream(Path.Combine(DirectoryPath, fileName), FileMode.Create);

            m_thread = new Thread(ThreadLoop);
            m_thread.Start();
        }

        private static void ThreadLoop()
        {
            while (true)
            {
                if (m_updatedEntries != 0)
                {
                    while (m_logQueue.TryDequeue(out LogMessage log))
                    {
                        string logString = log.ToString();
                        byte[] logBytes = Encoding.UTF8.GetBytes(logString);
                        m_fileStream.Write(logBytes, 0, logBytes.Length);

                        m_updatedEntries--;
                    }
                }
            }
        }

        public static void WriteLog(LogMessage log)
        {
            if (m_sessionIsCreated == false)
            {
                CreateSession();
            }

            m_logQueue.Enqueue(log);
            m_updatedEntries++;
        }
    }
}
