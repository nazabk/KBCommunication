using System;
using System.Threading;
using System.Threading.Tasks;

namespace KBCommunication.Interfaces
{
    /// <summary>
    /// Connection interface.
    /// </summary>
    /// <typeparam name="T">Data type for communication purposes.</typeparam>
    public interface IConnection<T>
    {
        /// <summary>
        /// Data has been received.
        /// </summary>
        event EventHandler<T> Received;

        /// <summary>
        /// Sends data.
        /// </summary>
        /// <param name="data">Data to send.</param>
        /// <param name="cancelToken">Canncellation token.</param>
        /// <returns>Awaitable. Sending task.</returns>
        /// <exception cref="OperationCanceledException">On <paramref name="cancelToken"/>
        /// cancellation requested.</exception>
        Task Send(T data, CancellationToken cancelToken);
    }
}
