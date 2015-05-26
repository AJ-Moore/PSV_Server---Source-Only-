using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace PSV_Server
{
    class ConsoleWriter : TextWriter
    {
        RichTextBox _output;
        private ReaderWriterLock rwl = new ReaderWriterLock();

        public ConsoleWriter(RichTextBox output)
        {
            _output = output;
        }
        public override void Write(char value){
            rwl.AcquireWriterLock(Timeout.Infinite);
            try
            {
                //Thread.Sleep(10);
                base.Write(value);
                _output.Text += (value.ToString());
            }
            finally
            {
                rwl.ReleaseWriterLock();
            }
        }
        public override Encoding Encoding{
            get { return System.Text.Encoding.UTF8; }
        }
    }
}


