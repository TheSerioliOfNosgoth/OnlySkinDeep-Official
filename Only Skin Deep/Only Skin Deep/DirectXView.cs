using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace ModelEx3
{
    public class DirectXView : UserControl
    {
        protected Color m_xBackGroundColour = Color.Black;
        protected Device m_xDevice;
        protected PresentParameters m_xPresentParams;
        private int m_iLastDeviceReset;
        private int m_iLastTick;
        private int m_iLastFrameTime;
        private int m_iFrameRate;
        private int m_iLastFrameRate;
        private bool m_bPauseRendering;
        private bool m_bDeviceLost;
        private Texture m_xTexture;
        private Rectangle m_xRect;

        public bool DeviceLost
        {
            get { return m_bDeviceLost; }
        }

        public int LastDeviceReset
        {
            get { return m_iLastDeviceReset; }
        }

        public bool PauseRendering
        {
            get { return m_bPauseRendering; }
            set
            {
                m_bPauseRendering = value;
                if (TopLevelControl != null &&
                    TopLevelControl.Focused == false)
                {
                    TopLevelControl.Focus();
                }
            }
        }

        public int FrameRate
        {
            get { return m_iLastFrameRate; }
        }

        public int LastFrameTime
        {
            get { return m_iLastFrameTime; }
        }

        public DirectXView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            Paint += new PaintEventHandler(Render);
            m_xTexture = null;
            m_xRect = new Rectangle(0, 0, 256, 256);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        //protected override bool IsInputKey(Keys keyData)
        //{
        //    return true;
        //    // return base.IsInputKey(keyData);
        //}

        public virtual bool Initialize()
        {
            return SetupDevice();
        }

        protected virtual bool SetupDevice()
        {
            try
            {
                m_xPresentParams = new PresentParameters();
                m_xPresentParams.Windowed = true;
                m_xPresentParams.SwapEffect = SwapEffect.Discard;
                m_xPresentParams.EnableAutoDepthStencil = true;
                m_xPresentParams.AutoDepthStencilFormat = DepthFormat.D16;
                m_xPresentParams.DeviceWindow = this;

                m_xDevice = new Device(
                    0,
                    DeviceType.Hardware,
                    this,
                    CreateFlags.SoftwareVertexProcessing |
                    CreateFlags.MultiThreaded,
                    m_xPresentParams
                );

                m_xDevice.DeviceResizing += new CancelEventHandler(m_xDevice_DeviceResizing);
                m_xDevice.DeviceReset += new EventHandler(OnResetDevice);
                m_xDevice.DeviceLost += new EventHandler(OnDeviceLost);
            }
            catch (DirectXException)
            {
                return false;
            }

            return true;
        }

        void m_xDevice_DeviceResizing(object sender, CancelEventArgs e)
        {
            // e.Cancel = true;
        }

        protected virtual void OnDeviceLost(object sender, EventArgs e)
        {
            m_bDeviceLost = true;

            // Dispose of all resources
        }

        protected virtual void OnResetDevice(object sender, EventArgs e)
        {            
            // Recreate all resources

            m_bDeviceLost = false;
            m_iLastDeviceReset = Environment.TickCount;

            m_xDevice.SetTexture(0, m_xTexture);

            Update();
        }

        protected void UpdateFrameRate()
        {
            // Track all frames in the last second...
            if (Environment.TickCount - m_iLastTick >= 1000)
            {
                m_iLastFrameRate = m_iFrameRate;
                m_iFrameRate = 0;
                m_iLastTick = Environment.TickCount;
                OnTick();
            }
            m_iLastFrameTime = Environment.TickCount;
            m_iFrameRate++;
        }

        protected virtual void OnTick()
        {
        }

        private void Render(object sender, PaintEventArgs e)
        {
            if (m_xDevice == null || TopLevelControl == null)
            {
                return;
            }

            if (((Form)TopLevelControl).WindowState == FormWindowState.Minimized ||
                ((Form)TopLevelControl).ContainsFocus == false)
            {
                return;
            }

            if (m_bDeviceLost)
            {
                int iCanReset = 0;
                bool bCanReset = !m_xDevice.CheckCooperativeLevel(out iCanReset);

                if (!bCanReset || (ResultCode)iCanReset != ResultCode.Success)
                {
                    return;
                }

                m_xDevice.Reset(m_xPresentParams);
            }

            // If the time since the last frame is less than a 30th of a second,
            //// then don't render the new frame...
            //if (((uint)Environment.TickCount - (uint)m_iLastFrameTime) < ((uint)3))
            //{
            //    return;
            //}

            UpdateFrameRate();

            // This should cause the other controls to be drawn...
            // No idea why it's needed though...
            TopLevelControl.Update();

            m_xDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, m_xBackGroundColour, 1.0f, 0);
            RenderScene();
            m_xDevice.Present();

            // Queue the next frame...
            if (PauseRendering == false)
            {
                Invalidate();
            }
        }

        public Device GetDevice()
        {
            return m_xDevice;
        }

        public void SetTexture(Texture texture, Rectangle rect)
        {
            m_xTexture = texture;
            m_xRect = rect;

            m_xDevice.SetTexture(0, texture);

            Invalidate();
        }

        protected virtual void RenderScene()
        {
            m_xDevice.BeginScene();

            CustomVertex.TransformedColoredTextured[] vertices = new CustomVertex.TransformedColoredTextured[4];

            vertices[0].Position = new Vector4((float)m_xRect.Left, (float)m_xRect.Top, 0, 1.0f);
            vertices[1].Position = new Vector4((float)m_xRect.Right, (float)m_xRect.Top, 0, 1.0f);
            vertices[2].Position = new Vector4((float)m_xRect.Left, (float)m_xRect.Bottom, 0, 1.0f);
            vertices[3].Position = new Vector4((float)m_xRect.Right, (float)m_xRect.Bottom, 0, 1.0f);

            vertices[0].Color = unchecked((int)0xFFFFFFFF);
            vertices[1].Color = unchecked((int)0xFFFFFFFF);
            vertices[2].Color = unchecked((int)0xFFFFFFFF);
            vertices[3].Color = unchecked((int)0xFFFFFFFF);

            vertices[0].Tu = 0.0f;
            vertices[0].Tv = 0.0f;
            vertices[1].Tu = 1.0f;
            vertices[1].Tv = 0.0f;
            vertices[2].Tu = 0.0f;
            vertices[2].Tv = 1.0f;
            vertices[3].Tu = 1.0f;
            vertices[3].Tv = 1.0f;

            m_xDevice.VertexFormat = CustomVertex.TransformedColoredTextured.Format;

            m_xDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertices);

            //vertices[0].Color = unchecked((int)0xFFFFFFFF);
            //vertices[1].Color = unchecked((int)0xFFFFFFFF);
            //vertices[2].Color = unchecked((int)0xFFFFFFFF);
            //vertices[3].Color = unchecked((int)0xFFFFFFFF);

            //UInt16[] indices = { 0, 1, 3, 2, 0 };
            //m_xDevice.DrawIndexedUserPrimitives(PrimitiveType.LineStrip, 0, 4, 4, indices, true, vertices);

            m_xDevice.EndScene();
        }
    }
}
