using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SQLiteTest.Utils
{
    public class PayloadReader : IDisposable
    {
        MemoryStream stream;
        BinaryReader reader;

        public PayloadReader(byte[] targetBuffer)
        {
            stream = new MemoryStream(targetBuffer);
            reader = new BinaryReader(stream);
        }

        /// <summary>
        /// Reads a 32-bit Single-precision float from the payload byte array and advances the current position
        /// of the array by 4 bytes.
        /// </summary>
        /// <returns>The next Single precision float read from the current stream, or Single.MinValue if an error occurred during conversion.</returns>
        /// <exception cref="System.IO.EndOfStreamException">The end of the payload is reached.</exception>
        /// <exception cref="System.ObjectDisposedException">The underlying MemoryStream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        public Single ReadSingle() { return reader.ReadSingle(); }

        public void Dispose()
        {
            if (reader != null)
                reader.Close();

            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }

            reader = null;
            stream = null;
        }
    }
}
