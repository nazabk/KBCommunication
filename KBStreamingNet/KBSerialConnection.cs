using KBCommunication.Interfaces;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace KBStreamingNet
{
    public class KBSerialConnection : IConnection<byte[]>
    {
        private readonly SemaphoreSlim sendSemaphore = new SemaphoreSlim(1, 1);
        private readonly SerialPort serialPort;

        private bool active;

        public KBSerialConnection(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.ErrorReceived += SerialPort_ErrorReceived;
        }

        public event EventHandler<byte[]> Received;

        public static int ReconnectionDelay { get; set; } = 500;

        public bool Active
        {
            get => serialPort.IsOpen;
            set
            {
                if (active == value) return;
                active = value;
                if (value)
                {
                    serialPort.Open();
                }
                else
                {
                    serialPort.Close();
                }
            }
        }

        public async Task Send(byte[] data, CancellationToken cancelToken)
        {
            await sendSemaphore.WaitAsync(cancelToken);
            try
            {
                if (data.Length > 0)
                {
                    await serialPort.BaseStream.WriteAsync(data, 0, data.Length, cancelToken);
                }
            }
            finally
            {
                sendSemaphore.Release();
            }
        }

        #region communication

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (sender is SerialPort port)
            {
                port.Close();
                Thread.Sleep(ReconnectionDelay);
                if (active)
                {
                    port.Open();
                }
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var port = sender as SerialPort;
                var bytesReceived = port.BytesToRead;
                if (bytesReceived > 0)
                {
                    var buffer = new byte[bytesReceived];
                    port.Read(buffer, 0, buffer.Length);
                    Task.Run(delegate { Received?.Invoke(this, buffer); });
                }
            }
            catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is TimeoutException)
            { }
        }

        #endregion communication

        #region IDispose

        // To detect redundant calls
        private bool _disposed = false;

        /// <summary>
        /// Disponses class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes class.
        /// </summary>
        /// <param name="disposing">Flag if managed resources should by disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Active = false;
            }

            _disposed = true;
        }

        #endregion IDispose
    }
}
