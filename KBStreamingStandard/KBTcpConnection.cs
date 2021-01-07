using KBCommunication.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KBStreaming
{
    public class KBTcpConnection : IConnection<byte[]>, IDisposable
    {
        private readonly IPAddress address;
        private readonly int port;
        private readonly byte[] buffer;
        private readonly SemaphoreSlim sendSemaphore = new SemaphoreSlim(1, 1);

        private TcpClient tcpClient;
        private bool active;

        public KBTcpConnection(IPAddress address, int port)
        {
            this.address = address;
            this.port = port;
            buffer = new byte[BufferSize];
            Active = true;
        }

        public event EventHandler<byte[]> Received;

        public static int BufferSize { get; set; } = 102400;

        public static int ReconnectionDelay { get; set; } = 100;

        public bool IsConnected
        {
            get
            {
                try
                {
                    if (tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected)
                    {
                        /* pear to the documentation on Poll:
                         * When passing SelectMode.SelectRead as a parameter to the Poll method it will return
                         * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                         * -or- true if data is available for reading;
                         * -or- true if the connection has been closed, reset, or terminated;
                         * otherwise, returns false
                         */

                        // Detect if client disconnected
                        if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex) when (
                ex is NotSupportedException ||
                ex is SocketException ||
                ex is ObjectDisposedException)
                {
                    return false;
                }
            }
        }

        public bool Active
        {
            get => active;
            set
            {
                if (active == value) return;
                active = value;
                if (active)
                {
                    Connect();
                }
                else
                {
                    tcpClient?.Close();
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
                    await tcpClient.GetStream().WriteAsync(data, 0, data.Length, cancelToken);
                }
            }
            finally
            {
                sendSemaphore.Release();
            }
        }

        #region communication

        private void Connect()
        {
            while (active)
            {
                try
                {
                    tcpClient = new TcpClient();
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    tcpClient.BeginConnect(address, port, OnConnected, tcpClient);
                    return;
                }
                catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException)
                {
                    Reconnect(tcpClient);
                }
            }
        }

        private void OnConnected(IAsyncResult ar)
        {
            var client = ar.AsyncState as TcpClient;
            try
            {
                client.EndConnect(ar);
                ReadMessage(client);
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException)
            {
                Reconnect(client);
            }
        }

        private void ReadMessage(TcpClient client)
        {
            if (!active) return;
            try
            {
                client.GetStream().BeginRead(buffer, 0, buffer.Length, OnReadMessge, client);
            }
            catch (Exception ex) when (
            ex is ObjectDisposedException ||
            ex is IOException ||
            ex is InvalidOperationException)
            {
                Reconnect(client);
            }
        }

        private void OnReadMessge(IAsyncResult ar)
        {
            var client = ar.AsyncState as TcpClient;
            try
            {
                var readed = client.GetStream().EndRead(ar);
                if (readed == 0) throw new IOException();
                var data = buffer.Take(readed).ToArray();
                Task.Run(delegate { Received?.Invoke(this, data); });
                ReadMessage(client);
            }
            catch (Exception ex) when (
            ex is ObjectDisposedException ||
            ex is IOException ||
            ex is InvalidOperationException)
            {
                Reconnect(client);
            }
        }

        private async void Reconnect(TcpClient client)
        {
            client?.Close();
            await Task.Delay(ReconnectionDelay);
            Connect();
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
