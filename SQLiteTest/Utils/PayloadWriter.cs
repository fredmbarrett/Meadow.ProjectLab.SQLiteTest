using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SQLiteTest.Utils
{
    public class PayloadWriter : IDisposable
    {
        MemoryStream stream;
        BinaryWriter writer;

        public PayloadWriter(byte[] targetBuffer)
        {
            stream = new MemoryStream(targetBuffer);
            writer = new BinaryWriter(stream);
        }

        public PayloadWriter WriteSingle(float value)
        {
            writer.Write((Single)value);
            return this;
        }

        public void Dispose()
        {
            if (writer != null)
                writer.Close();

            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }

            writer = null;
            stream = null;
        }
    }
}
