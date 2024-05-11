#if WINDOWS
using Dtwo;
using Dtwo.API;
using Dtwo.App;
using Dtwo.App.DesktopApp;
using Dtwo.App.DesktopApp.Windowing;
using Microsoft.UI;
using System.Runtime.InteropServices;
using Windows.Graphics;
using AppWindow = Microsoft.UI.Windowing.AppWindow;
#endif


namespace Dtwo.App.DesktopApp.Windowing
{
#if WINDOWS
    public class WindowDrag
    {
        private readonly List<InputKey> m_dragKeys = new List<InputKey> { new InputKey() { KeyId = 0x01 } };

        private static POINT m_initialClickOffset;
        private static POINT m_lastMousePosition;

        private bool m_isDrag = false;
        private InputListener m_inputListener;
        private AppWindow m_appWindow;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public WindowDrag(AppWindow appWindow)
        {
            m_appWindow = appWindow;
        }

        private void DragWindow()
        {
            if (m_isDrag == false)
                return;

            var cursor = GetMousePosition();

            if (cursor.X != m_lastMousePosition.X || cursor.Y != m_lastMousePosition.Y)
            {
                m_lastMousePosition = new POINT() { X = cursor.X, Y = cursor.Y };
                m_appWindow.Move(new PointInt32(cursor.X - m_initialClickOffset.X, cursor.Y - m_initialClickOffset.Y));
            }
        }

        public void StartDragWindow()
        {
            if (m_isDrag)
                return;

            m_isDrag = true;
            m_inputListener = new InputListener();

            m_inputListener.Listen(m_dragKeys, (key) =>
            {
                StopDragWindow();
            });

            var cursor = GetMousePosition();

            m_initialClickOffset = new POINT
            {
                X = cursor.X - m_appWindow.Position.X,
                Y = cursor.Y - m_appWindow.Position.Y
            };

            m_lastMousePosition = m_initialClickOffset;

            Task.Factory.StartNew(() =>
            {
                while (m_isDrag)
                {
                    DragWindow();
                }
            });
        }

        private void StopDragWindow()
        {
            m_isDrag = false;
        }


        private static (int X, int Y) GetMousePosition()
        {
            if (GetCursorPos(out POINT point))
            {
                return (point.X, point.Y);
            }
            return (0, 0);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);
    }
#endif
}
