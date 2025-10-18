using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Windows;

using SharpDX.XAudio2;
using SharpDX.Multimedia;

using RectangleF = SharpDX.RectangleF;
using Color = System.Drawing.Color;

namespace PicoSharp
{
    struct Vertex
    {
        public Vector4 Position;
        public int Color;
        public Vector2 TextureCoord;
    }

    static class Program
	{
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        private const int AUDIO_BUFFER_COUNT = 3;

        private static Emulator m_emulator = null;

		private static XAudio2 m_xAudio2 = null;
		private static MasteringVoice m_masteringVoice = null;
		private static SourceVoice m_sourceVoice = null;
        private static readonly Queue<DataStream> m_pendingStreams = new Queue<DataStream>();

        private static VertexBuffer m_vertexBuffer = null;
		private static VertexDeclaration m_vertexDeclaration = null;

		private static RenderForm m_form = null;
		private static Device m_device = null;
		private static Texture m_texture = null;
        private static Size m_screenSize = new Size(128, 128);
        private static int m_scale = 4;

		private static Timer m_timer = null;

		private static IntPtr m_pixelsIntPtr = IntPtr.Zero;

		[STAThread]
		static void Main()
		{
            string cartPath = Path.Combine(Application.StartupPath, "carts");

            //string fileName = Path.Combine(cartPath, "Hello World.p8.png");
            //string fileName = Path.Combine(cartPath, "Celeste.p8.png");
            //string fileName = Path.Combine(cartPath, "Desert Drift.p8.png");
            //string fileName = Path.Combine(cartPath, "Evening Train Ride.p8.png");
            //string fileName = Path.Combine(cartPath, "Maze.p8.png");
            //string fileName = Path.Combine(cartPath, "Pentagram.p8.png");
            //string fileName = Path.Combine(cartPath, "Picad.p8.png");
            //string fileName = Path.Combine(cartPath, "Super Oval.p8.png");
            //string fileName = Path.Combine(cartPath, "Hunting Grounds.p8.png");

            //string fileName = Path.Combine(cartPath, "Hello World.p8");
            string fileName = Path.Combine(cartPath, "Celeste.p8");
            //string fileName = Path.Combine(cartPath, "Desert Drift.p8");
            //string fileName = Path.Combine(cartPath, "Evening Train Ride.p8");
            //string fileName = Path.Combine(cartPath, "Maze.p8");
            //string fileName = Path.Combine(cartPath, "Pentagram.p8");
            //string fileName = Path.Combine(cartPath, "Picad.p8");
            //string fileName = Path.Combine(cartPath, "Super Oval.p8");
            //string fileName = Path.Combine(cartPath, "Hunting Grounds.p8");

            //string fileName = Path.Combine(cartPath, "apitest1.p8");
            //string fileName = Path.Combine(cartPath, "btntest.p8");
            //string fileName = Path.Combine(cartPath, "coroutine.p8");
            //string fileName = Path.Combine(cartPath, "exfont.p8");
            //string fileName = Path.Combine(cartPath, "expal.p8");
            //string fileName = Path.Combine(cartPath, "font.p8");
            //string fileName = Path.Combine(cartPath, "fstest.p8");
            //string fileName = Path.Combine(cartPath, "geomtest.p8");
            //string fileName = Path.Combine(cartPath, "mousetest.p8");
            //string fileName = Path.Combine(cartPath, "oldfont.p8");
            //string fileName = Path.Combine(cartPath, "paltest.p8");
            //string fileName = Path.Combine(cartPath, "sfxmusictest.p8");
            //string fileName = Path.Combine(cartPath, "sfxtest.p8");
            //string fileName = Path.Combine(cartPath, "soundlooptest.p8");
            //string fileName = Path.Combine(cartPath, "sspr.p8");
            //string fileName = Path.Combine(cartPath, "stattest.p8");
            //string fileName = Path.Combine(cartPath, "time.p8");
            //string fileName = Path.Combine(cartPath, "touchtest.p8");
            //string fileName = Path.Combine(cartPath, "zoomtest.p8");

            m_emulator = new Emulator(fileName);
            m_emulator.Audio += OnAudio;

            m_form = new RenderForm("PicoSharp");
			m_form.TopMost = false;
			m_form.FormBorderStyle = FormBorderStyle.Sizable;
			m_form.WindowState = FormWindowState.Normal;
            m_form.ClientSize = new Size(m_screenSize.Width * m_scale, m_screenSize.Height * m_scale);
            m_form.Show();

            Direct3D direct3D = new Direct3D();

            var adapter = 0;
            var dispMode = direct3D.Adapters[adapter].CurrentDisplayMode;

            m_device = new Device(direct3D, 0, DeviceType.Hardware, m_form.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded, new PresentParameters()
            {
                BackBufferWidth = m_form.ClientSize.Width,
                BackBufferHeight = m_form.ClientSize.Height,
                BackBufferFormat = dispMode.Format,
                BackBufferCount = 1,
                SwapEffect = SwapEffect.Discard,
                PresentationInterval = PresentInterval.Immediate,
                DeviceWindowHandle = m_form.Handle,
                Windowed = true,
                EnableAutoDepthStencil = false
            });

			InitVideoEngine(m_screenSize);
            InitAudioEngine();

            m_emulator.RenderSounds();

            m_timer = new Timer();
			m_timer.Start();

            //m_form.KeyDown += new KeyEventHandler(OnKeyDown);

            double m_accumulator = 0.0;

			RenderLoop.Run(m_form, () =>
			{
				m_timer.Update();

                m_emulator.Update(m_timer.ElapsedTime);

                m_accumulator += m_timer.ElapsedTime;

                //Debug.WriteLine(m_timer.ElapsedTime);

				if (m_accumulator >= 1.0 / 60.0)
				{
                    m_accumulator = 0.0;

                    Marshal.Copy(m_emulator.PixelArray, 0, m_pixelsIntPtr, m_emulator.PixelArray.Length);

                    DirectX9.LoadTextureDataFromIntPtr(m_device, m_texture, m_pixelsIntPtr, 128, 128, 128 * 4);

                    Render();
				}

				if ((GetKeyState((int)Keys.Escape) & 0x8000) != 0)
					m_form.Close();
			});

			DestroyTexture();

            ShutdownAudioEngine();

            Marshal.FreeHGlobal(m_pixelsIntPtr);
		}

        public static void InitVideoEngine(Size canvasSize)
        {
            CreateTexture(m_device, m_screenSize.Width, m_screenSize.Height);

            m_pixelsIntPtr = Marshal.AllocHGlobal(m_screenSize.Width * m_screenSize.Height * 4);

            SizeF quadSize = ResizeKeepAspect(canvasSize, new Size(m_screenSize.Width * m_scale, m_screenSize.Height * m_scale));
            RectangleF quadRect = new RectangleF(quadSize.Width / 2.0f - quadSize.Width / 2.0f, m_form.ClientSize.Height / 2.0f - quadSize.Height / 2.0f, quadSize.Width, quadSize.Height);
            Vertex[] vertexArray = new Vertex[3 * 2];

            vertexArray[0] = new Vertex() { Color = Color.White.ToArgb(), Position = new Vector4(quadRect.Left, quadRect.Top, 0.0f, 1.0f), TextureCoord = new Vector2(0.0f, 0.0f) };
            vertexArray[1] = new Vertex() { Color = Color.White.ToArgb(), Position = new Vector4(quadRect.Right, quadRect.Bottom, 0.0f, 1.0f), TextureCoord = new Vector2(1.0f, 1.0f) };
            vertexArray[2] = new Vertex() { Color = Color.White.ToArgb(), Position = new Vector4(quadRect.Left, quadRect.Bottom, 0.5f, 1.0f), TextureCoord = new Vector2(0.0f, 1.0f) };

            vertexArray[3] = new Vertex() { Color = Color.White.ToArgb(), Position = new Vector4(quadRect.Left, quadRect.Top, 0.0f, 1.0f), TextureCoord = new Vector2(0.0f, 0.0f) };
            vertexArray[4] = new Vertex() { Color = Color.White.ToArgb(), Position = new Vector4(quadRect.Right, quadRect.Top, 0.0f, 1.0f), TextureCoord = new Vector2(1.0f, 0.0f) };
            vertexArray[5] = new Vertex() { Color = Color.White.ToArgb(), Position = new Vector4(quadRect.Right, quadRect.Bottom, 0.0f, 1.0f), TextureCoord = new Vector2(1.0f, 1.0f) };

            m_vertexBuffer = new VertexBuffer(m_device, vertexArray.Length * Marshal.SizeOf(typeof(Vertex)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);

            // SharpDX lock returns DataStream (same call pattern)
            var ds = m_vertexBuffer.Lock(0, 0, LockFlags.None);
            ds.WriteRange(vertexArray);
            m_vertexBuffer.Unlock();

            VertexElement[] vertexElems = new VertexElement[] {
                new VertexElement(0, 0,  DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
                new VertexElement(0, 16, DeclarationType.Color,  DeclarationMethod.Default, DeclarationUsage.Color,              0),
                new VertexElement(0, 20, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate,  0),
                VertexElement.VertexDeclarationEnd
            };

            m_vertexDeclaration = new VertexDeclaration(m_device, vertexElems);
        }

        public static void InitAudioEngine()
        {
            m_xAudio2 = new XAudio2();
            m_masteringVoice = new MasteringVoice(m_xAudio2);

            var waveFormat = new WaveFormat(44100, 16, 1);

            m_sourceVoice = new SourceVoice(m_xAudio2, waveFormat);
            m_sourceVoice.BufferEnd += OnBufferEnd;
            m_sourceVoice.StreamEnd += OnStreamEnd;

            m_sourceVoice.Start();
        }

        public static void ShutdownAudioEngine()
        {
            if (m_sourceVoice != null) { m_sourceVoice.DestroyVoice(); m_sourceVoice.Dispose(); m_sourceVoice = null; }
            if (m_masteringVoice != null) { m_masteringVoice.Dispose(); m_masteringVoice = null; }
            if (m_xAudio2 != null) { m_xAudio2.Dispose(); m_xAudio2 = null; }

            while (m_pendingStreams.Count > 0) m_pendingStreams.Dequeue().Dispose();
        }

        private static void Render()
        {
            m_device.Clear(ClearFlags.Target, SharpDX.Color.Black, 1.0f, 0);
            m_device.BeginScene();

            m_device.SetRenderState(RenderState.AlphaBlendEnable, true);
            m_device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            m_device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

            m_device.SetTexture(0, m_texture);
            m_device.SetStreamSource(0, m_vertexBuffer, 0, Marshal.SizeOf(typeof(Vertex)));
            m_device.VertexFormat = VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Texture1;
            m_device.VertexDeclaration = m_vertexDeclaration;
            m_device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

            m_device.EndScene();
            m_device.Present();
        }

        private static void OnBufferStart(IntPtr context) { }

        private static void OnBufferEnd(nint context)
        {
            // free the oldest submitted DataStream
            if (m_pendingStreams.Count > 0)
            {
                var ds = m_pendingStreams.Dequeue();
                ds.Dispose();
            }
        }

        private static void OnStreamEnd()
        {
            m_emulator.RenderSounds();
        }

        private static void OnAudio(object sender, AudioEventArgs e)
        {
            if (e.Samples == null || e.Samples.Length == 0)
                return;

            // convert short[] -> unmanaged DataStream
            int byteCount = e.Samples.Length * sizeof(short);
            var ds = new DataStream(byteCount, true, true);
            ds.WriteRange(e.Samples);
            ds.Position = 0;

            var buffer = new AudioBuffer
            {
                AudioDataPointer = ds.DataPointer,
                AudioBytes = byteCount,
                Flags = BufferFlags.EndOfStream
            };

            // avoid runaway queue growth
            if (m_sourceVoice.State.BuffersQueued > AUDIO_BUFFER_COUNT)
                m_sourceVoice.FlushSourceBuffers();

            m_sourceVoice.SubmitSourceBuffer(buffer, null);

            // keep stream alive until XAudio2 finishes with it
            m_pendingStreams.Enqueue(ds);
        }

        private static void Screen_Reinitialize(object sender, EventArgs e) { }
        private static void Screen_VideoFrame(object sender, EventArgs e)
        {
            Marshal.Copy(m_emulator.PixelArray, 0, m_pixelsIntPtr, m_emulator.PixelArray.Length);
            DirectX9.LoadTextureDataFromIntPtr(m_device, m_texture, m_pixelsIntPtr, 128, 128, 128 * 4);
        }

        private static void DestroyTexture()
        {
            if (m_texture != null)
            {
                m_texture.Dispose();
                m_texture = null;
            }
        }

        private static bool CreateTexture(Device device, int width, int height)
        {
            DestroyTexture();

            m_texture = new Texture(device, width, height, 0, Usage.Dynamic, Format.X8R8G8B8, Pool.Default);

            DirectX9.ClearTexture(m_texture);

            int pitch = DirectX9.GetTexturePitch(m_texture);

            return true;
        }

        public static Size ResizeKeepAspect(Size sourceSize, Size destSize)
        {
            return ResizeKeepAspect(new SizeF(sourceSize.Width, sourceSize.Height), new SizeF(destSize.Width, destSize.Height)).ToSize();
        }

        public static SizeF ResizeKeepAspect(SizeF sourceSize, SizeF destSize)
        {
            SizeF retSize = new SizeF();

            float sourceAspect = sourceSize.Width / sourceSize.Height;
            float destAspect = destSize.Width / destSize.Height;

            if (sourceAspect > destAspect)
            {
                retSize.Width = destSize.Width;
                retSize.Height = destSize.Width / sourceAspect;
            }
            else
            {
                retSize.Height = destSize.Height;
                retSize.Width = destSize.Height * sourceAspect;
            }

            return retSize;
        }
    }
}