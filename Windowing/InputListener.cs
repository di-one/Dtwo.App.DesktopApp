using Dtwo.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dtwo.App.DesktopApp.Windowing
{
    public class InputListener : IDisposable
    {
        private bool m_isListening;
        private bool disposedValue;
        private Task? m_task;

        #region PInvoke
        public static bool KeyIsDown(int key)
        {
            return (GetKeyState(key) & KEY_PRESSED) != 0;
        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(ushort virtualKeyCode);

        private const int KEY_PRESSED = 0x8000;

        [DllImport("user32.dll")]
        static extern short GetKeyState(int key);
        #endregion

        public InputListener()
        {
        }

        public void Listen(List<InputKey> inputKeys, Action<InputKey> onKeyUp)
        {
            if (m_isListening) return;

            m_isListening = true;

            m_task = Task.Factory.StartNew(() =>
            {
                try
                {
                    while (m_isListening)
                    {
                        for (int i = 0; i < inputKeys.Count; i++)
                        {
                            if (m_isListening == false)
                                break;

                            var inputKey = inputKeys[i];

                            var asyncKeyState = GetAsyncKeyState((ushort)inputKey.KeyId);

                            //Debug.WriteLine($"Key {inputKey.KeyId} is down {asyncKeyState}");

                            if (asyncKeyState == 0)
                            {
                                onKeyUp(inputKey);
                            }
                            else
                            {
                                continue;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    LogManager.LogError(ex.ToString(), 1);
                }

                LogManager.Log("key listening thread exit");
            }, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            m_isListening = false;
            m_task = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                Stop();

                disposedValue = true;
            }
        }

        // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        ~InputListener()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
