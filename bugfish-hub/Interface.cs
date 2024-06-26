using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using bugfish_hub.Library.Sqlite;

namespace bugfish_hub
{
    public partial class Interface : Form
    {
        private int borderWidth = 10; // Set the width of the border
        private Color borderColor = Color.FromArgb(0x24, 0x24, 0x24); // Set the color of the border
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnMaximize;
        private System.Windows.Forms.Button btnClose;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private Sqlite sqlite;
        private Point offset;

        public Interface()
        {
            InitializeComponent();
            sqlite = new Sqlite("data.db");
            interface_init_frame_btn();

            // Subscribe to the Paint event
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(borderWidth);
            this.Padding = new Padding(5);
            this.Paint += new PaintEventHandler(Interface_Paint);
            this.Resize += Interface_Resize;

            // Create and configure ContextMenuStrip
            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Show", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; notifyIcon.Visible = true; });
            contextMenuStrip.Items.Add("Exit", null, (s, e) => { Application.Exit(); });


            // Create and configure NotifyIcon
            notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.favicon,
                Visible = true,
                ContextMenuStrip = contextMenuStrip
            };
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Functionality for Resize and Draw the Window
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        // Interface Paint Functionality
        private void Interface_Paint(object sender, PaintEventArgs e)
        {
            // Draw the custom border
            using (Pen borderPen = new Pen(borderColor, borderWidth))
            {
                e.Graphics.DrawRectangle(borderPen, new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1));
            }
        }

        // Interface Resize Functionality
        private void Interface_Resize(object sender, EventArgs e)
        {
            // Update button locations on resize
            btnMinimize.Location = new Point(this.Width - 95, 5);
            btnMaximize.Location = new Point(this.Width - 65, 5);
            btnClose.Location = new Point(this.Width - 35, 5);
        }

        // Minimize Button Click to Minimize Current Form
        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        // Close Window to Tray or Close Completely
        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
            notifyIcon.Visible = true;
        }

        // Maximize Button Click Functionality
        private void BtnMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                Rectangle workingArea = Screen.GetWorkingArea(this);
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;
                this.Location = new Point(Math.Max(this.Location.X, workingArea.X),
                          Math.Max(this.Location.Y, workingArea.Y));
            }
        }

        // NotifyIcon DoubleClick
        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show(); this.WindowState = FormWindowState.Normal; notifyIcon.Visible = true;
        }

        // Initialize Border and Buttons
        private void interface_init_frame_btn()
        {
            // Minimize Button
            btnMinimize = new System.Windows.Forms.Button
            {
                Text = "_",
                Size = new Size(30, 30),
                Location = new Point(this.Width - 95, 5),
                BackColor = Color.FromArgb(0x24, 0x24, 0x24),
                FlatStyle = FlatStyle.Flat
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += BtnMinimize_Click;
            btnMinimize.ForeColor = Color.FromArgb(0xFF, 0x57, 0x07);
            tooltip_frame.SetToolTip(btnMinimize, "Minimize");

            // Maximize Button
            btnMaximize = new System.Windows.Forms.Button
            {
                Text = "O",
                Size = new Size(30, 30),
                Location = new Point(this.Width - 65, 5),
                BackColor = Color.FromArgb(0x24, 0x24, 0x24),
                FlatStyle = FlatStyle.Flat
            };
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnMaximize.Click += BtnMaximize_Click;
            btnMaximize.ForeColor = Color.FromArgb(0xFF, 0x57, 0x07);
            tooltip_frame.SetToolTip(btnMaximize, "Maximize");

            // Close Button
            btnClose = new System.Windows.Forms.Button
            {
                Text = "X",
                Size = new Size(30, 30),
                Location = new Point(this.Width - 35, 5),
                BackColor = Color.FromArgb(0x24, 0x24, 0x24),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;
            btnClose.ForeColor = Color.FromArgb(0xFF, 0x57, 0x07);
            tooltip_frame.SetToolTip(btnClose, "Close");

            // Add buttons to the form
            this.Controls.Add(btnMinimize);
            this.Controls.Add(btnMaximize);
            this.Controls.Add(btnClose);

            btnClose.BringToFront();
            btnMaximize.BringToFront();
            btnMinimize.BringToFront();

        }

        // Allow for resizing by overriding WndProc
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int WM_GETMINMAXINFO = 0x24;
            const int HTCLIENT = 1;
            const int HTCAPTION = 2;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    base.WndProc(ref m);

                    Point pos = PointToClient(new Point(m.LParam.ToInt32()));
                    if (pos.X < borderWidth && pos.Y < borderWidth)
                    {
                        m.Result = (IntPtr)HTTOPLEFT;
                    }
                    else if (pos.X > Width - borderWidth && pos.Y < borderWidth)
                    {
                        m.Result = (IntPtr)HTTOPRIGHT;
                    }
                    else if (pos.X < borderWidth && pos.Y > Height - borderWidth)
                    {
                        m.Result = (IntPtr)HTBOTTOMLEFT;
                    }
                    else if (pos.X > Width - borderWidth && pos.Y > Height - borderWidth)
                    {
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                    }
                    else if (pos.X < borderWidth)
                    {
                        m.Result = (IntPtr)HTLEFT;
                    }
                    else if (pos.X > Width - borderWidth)
                    {
                        m.Result = (IntPtr)HTRIGHT;
                    }
                    else if (pos.Y < borderWidth)
                    {
                        m.Result = (IntPtr)HTTOP;
                    }
                    else if (pos.Y > Height - borderWidth)
                    {
                        m.Result = (IntPtr)HTBOTTOM;
                    }
                    else
                    {
                        m.Result = (IntPtr)HTCLIENT;
                    }
                    return;

                case WM_GETMINMAXINFO:
                    MINMAXINFO minMaxInfo = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));
                    minMaxInfo.ptMinTrackSize.X = 700; // Minimum width
                    minMaxInfo.ptMinTrackSize.Y = 500; // Minimum height
                    Marshal.StructureToPtr(minMaxInfo, m.LParam, true);
                    break;
            }
            base.WndProc(ref m);
        }

        // Function for Mouse Move on Title Bar Selection
        private void header_frame_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Capture the offset from the mouse cursor to the form's location
                offset = new Point(e.X, e.Y);
            }
        }

        // Additional Function for MouseMove
        private void header_frame_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Move the form with the mouse
                Point newLocation = this.PointToScreen(new Point(e.X, e.Y));
                newLocation.Offset(-offset.X, -offset.Y);

                // Ensure the form stays within the screen bounds
                Screen screen = Screen.FromPoint(newLocation);
                Rectangle screenBounds = screen.Bounds;

                // Adjust newLocation if it goes outside screen bounds
                int newX = Math.Max(screenBounds.Left, Math.Min(screenBounds.Right - this.Width, newLocation.X));
                int newY = Math.Max(screenBounds.Top, Math.Min(screenBounds.Bottom - this.Height, newLocation.Y));

                this.Location = new Point(newX, newY);
            }
        }

        // Extra Function for Minimum Resizing in Width and Height to not make the Window Disappear
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public Point ptReserved;
            public Point ptMaxSize;
            public Point ptMaxPosition;
            public Point ptMinTrackSize;
            public Point ptMaxTrackSize;
        }
    }
}
