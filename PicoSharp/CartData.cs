using System;
using System.IO;

namespace PicoSharp
{
    public class CartData
    {
        // Inject or set these as appropriate for your project
        private readonly byte[] m_memory;

        // Offsets/constants you already have
        private const int MEMORY_CARTDATA = 0x5e00;
        private const int MEMORY_CARTDATA_SIZE = 0x100;

        private FileStream? cartdata;
        private bool m_needs_flush;

        public CartData(byte[] memory)
        {
            m_memory = memory;
        }

        public bool Open(string id)
        {
            if (cartdata != null)
                return false;

            try
            {
                // Get user directory
                string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string appPath = Path.Combine(userProfilePath, "PicoSharp");
                string cartPath = Path.Combine(appPath, $"{id}.bin");

                // mkdir -p behavior
                Directory.CreateDirectory(appPath);

                // Open or create; read/write; exclusive
                cartdata = new FileStream(cartPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

                // Read up to 0x100 bytes into memory at MEMORY_CARTDATA
                cartdata.Seek(0, SeekOrigin.Begin);
                int toRead = MEMORY_CARTDATA_SIZE;
                int readTotal = 0;

                while (toRead > 0)
                {
                    int read = cartdata.Read(m_memory, MEMORY_CARTDATA + readTotal, toRead);
                    if (read <= 0) break;
                    readTotal += read;
                    toRead -= read;
                }

                // If file shorter than 0x100, zero-fill the rest
                if (readTotal < MEMORY_CARTDATA_SIZE)
                    Array.Clear(m_memory, MEMORY_CARTDATA + readTotal, MEMORY_CARTDATA_SIZE - readTotal);

                return true;
            }
            catch
            {
                // If anything failed, ensure stream is closed and return false
                if (cartdata != null)
                {
                    try { cartdata.Dispose(); } catch { /* ignore */ }
                    cartdata = null;
                }
                return false;
            }
        }

        public void Flush()
        {
            if (cartdata != null && m_needs_flush)
            {
                m_needs_flush = false;
                cartdata.Seek(0, SeekOrigin.Begin);
                cartdata.Write(m_memory, MEMORY_CARTDATA, MEMORY_CARTDATA_SIZE);
                cartdata.Flush(true);
            }
        }

        public void SetDelayedFlush()
        {
            m_needs_flush = true;
        }

        public void Close()
        {
            if (cartdata != null)
            {
                Flush();
                cartdata.Dispose();
                cartdata = null;
            }
        }
    }
}