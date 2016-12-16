using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Only_Skin_Deep
{
    public partial class ListViewEx : ListView
    {
        private const int WM_NULL = 0x0000;
        private const int WM_CREATE = 0x0001;
        private const int WM_DESTROY = 0x0002;
        private const int WM_MOVE = 0x0003;
        private const int WM_SIZE = 0x0005;
        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_HSCROLL = 0x00000114;
        private const int WM_VSCROLL = 0x00000115;
        private const int WM_NOTIFY = 0x0000004E;
        private const int SBM_SETSCROLLINFO = 0x000000E9;
        private const int HDN_FIRST = (0 - 300);
        private const int HDN_BEGINTRACKA = (HDN_FIRST - 6);
        private const int HDN_BEGINTRACKW = (HDN_FIRST - 26);
        private const int WM_LBUTTONDBLCLK = 0x00000203;
        private const int WM_SETCURSOR = 0x00000020;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int WS_VSCROLL = 0x00200000;
        private const int WS_HSCROLL = 0x00100000;
        private const int LVM_FIRST = 0x00001000;
        private const int LVM_GETHEADER = (LVM_FIRST + 31);
        private const int LVM_SETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 54);
        private const int LVM_GETEXTENDEDLISTVIEWSTYLE = (LVM_FIRST + 55);

        private const int SB_HORZ = 0;
        private const int SB_VERT = 1;
        private const int SB_CTL = 2;
        private const int SB_BOTH = 3;

        private const int ESB_ENABLE_BOTH = 0;
        private const int ESB_DISABLE_BOTH = 3;
        private const int ESB_DISABLE_LEFT = 1;
        private const int ESB_DISABLE_RIGHT = 2;
        private const int ESB_DISABLE_UP = 1;
        private const int ESB_DISABLE_DOWN = 2;
        private const int ESB_DISABLE_LTUP = 1;
        private const int ESB_DISABLE_RTDN = 2;

        private const UInt32 SIF_RANGE = 0x00000001;
        private const UInt32 SIF_PAGE = 0x00000002;
        private const UInt32 SIF_POS = 0x00000004;
        private const UInt32 SIF_DISABLENOSCROLL = 0x00000008;
        private const UInt32 SIF_TRACKPOS = 0x00000010;
        private const UInt32 SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS);

        /// <summary>
        /// Scroll info structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SCROLLINFO
        {
            public UInt32 cbSize;
            public UInt32 fMask;
            public int nMin;
            public int nMax;
            public UInt32 nPage;
            public int nPos;
            public int nTrackPos;
        }

        /// <summary>
        /// Notify message header structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR
        {
            public IntPtr hwndFrom;
            public int idFrom;
            public int code;
        }

        /// <summary>
        /// Class used to capture window messages for the header of the list view
        /// control.  
        /// </summary>
        private class HeaderControl : NativeWindow
        {
            private ListViewEx parentListView = null;

            public HeaderControl(ListViewEx listview)
            {
                parentListView = listview;
                IntPtr header = SendMessage(listview.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
                this.AssignHandle(header);
            }

            protected override void WndProc(ref Message message)
            {
                switch (message.Msg)
                {
                    case WM_LBUTTONDBLCLK:
                    case WM_SETCURSOR:
                    if (parentListView.LockColumnSize)
                    {
                        message.Result = (IntPtr)1;
                        return;
                    }
                    break;
                }
                
                base.WndProc(ref message);
            }
        }

        [DllImport("User32", CharSet = CharSet.Auto)]
        private static extern int GetWindowLong(IntPtr hWnd, int Index);

        [DllImport("User32", CharSet = CharSet.Auto)]
        private static extern int SetWindowLong(IntPtr hWnd, int Index, int Value);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool ShowScrollBar(IntPtr hWnd, int fnBar, bool bShow);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool EnableScrollBar(IntPtr hWnd, uint wSBflags, uint wArrows);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GetScrollInfo(IntPtr hWnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int SetScrollInfo(IntPtr hWnd, int fnBar, ref SCROLLINFO lpsi, bool fRedraw);

        private bool locked = true;
        private HeaderControl hdrCtrl = null;

        /// <summary>
        /// Property to turn on and off the ability to size the column headers.
        /// </summary>
        public bool LockColumnSize
        {
            get { return locked; }
            set { locked = value; }
        }

        public ListViewEx()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                case WM_VSCROLL:
                case WM_HSCROLL:
                {
                    // Move focus to the ListView to cause ComboBox to lose focus.
                    this.Focus();
                    break;
                }
                case WM_NOTIFY:
                {
                    NMHDR nmhdr = (NMHDR)msg.GetLParam(typeof(NMHDR));
                    switch (nmhdr.code)
                    {
                        case HDN_BEGINTRACKA:
                        case HDN_BEGINTRACKW:
                            if (locked)
                            {
                                msg.Result = (IntPtr)1;
                                return;
                            }
                            break;
                    }
                    break;
                }
                case WM_NCCALCSIZE:
                {
                    UpdateScrollBars();
                    break;
                }
            }

            base.WndProc(ref msg);
        }


        /// <summary>
        /// When the control is created capture the messages for the header. 
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            hdrCtrl = new HeaderControl(this);
        }

        /// <summary>
        /// Capture CTRL+ to prevent resize of all columns.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyValue == 107 && e.Modifiers == Keys.Control && locked)
            {
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        /// <summary>
        /// Overrides conditions of when to hide or disable the scrollbars
        /// </summary>
        public void UpdateScrollBars()
        {
            ShowScrollBar(this.Handle, SB_VERT, true);

            SCROLLINFO scrollinfo = new SCROLLINFO();
            scrollinfo.cbSize = (UInt32)Marshal.SizeOf(typeof(SCROLLINFO));
            scrollinfo.fMask = SIF_ALL;
            scrollinfo.nMin = 0;
            scrollinfo.nMax = 0;
            scrollinfo.nPage = 0;
            scrollinfo.nPos = 0;
            scrollinfo.nTrackPos = 0;
            bool getResult = GetScrollInfo(this.Handle, SB_VERT, ref scrollinfo);
            int iLastError = Marshal.GetLastWin32Error();

            UInt32 wArrows = ESB_DISABLE_BOTH;
            if (getResult && Items.Count > (int)scrollinfo.nPage)
            {
                wArrows = ESB_ENABLE_BOTH;
            }

            EnableScrollBar(this.Handle, SB_VERT, wArrows);
        }
    }
}
