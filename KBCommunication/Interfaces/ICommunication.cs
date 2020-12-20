using System;
using System.Threading;
using System.Threading.Tasks;

namespace KBCommunication.Interfaces
{
    /// <summary>
    /// Communication handler witch conversion between <typeparamref name="T"/> and <typeparamref name="F"/> types.
    /// </summary>
    /// <typeparam name="F">Type of frames.</typeparam>
    /// <typeparam name="T">Data type for communication purposes.</typeparam>
    public interface ICommunication<F, T> : IConnection<F>
    {
        /// <summary>
        /// Frame factory.
        /// </summary>
        IFrameFactory<F, T> FrameFactory { get; }

        /// <summary>
        /// Current connection.
        /// </summary>
        IConnection<T> Connection { get; }

        /// <summary>
        /// Receives frame.
        /// </summary>
        /// <param name="cancelToken">Cancellation token.</param>
        /// <returns>Awaitable. <typeparamref name="F"/> frame.</returns>
        /// <exception cref="OperationCanceledException">On <paramref name="cancelToken"/>
        /// cancellation requested.</exception>
        Task<F> Receive(CancellationToken cancelToken);

        /// <summary>
        /// Sends question and waits for response.
        /// </summary>
        /// <param name="frame">Question to send.</param>
        /// <param name="cancelToken">Cancellation token.</param>
        /// <returns>Awaitable. <typeparamref name="F"/> response.</returns>
        /// <exception cref="OperationCanceledException">On <paramref name="cancelToken"/>
        /// cancellation requested.</exception>
        Task<F> Ask(F frame, CancellationToken cancelToken);
    }
}
