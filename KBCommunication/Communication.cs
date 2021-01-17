using KBCommunication.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KBCommunication
{
    /// <inheritdoc cref="ICommunication{F, T}"/>
    public class Communication<F, T> : ICommunication<F, T>, IDisposable
    {
        /// <summary>
        /// Receiving semaphore.
        /// </summary>
        protected readonly SemaphoreSlim ReceivingSemaphore = new SemaphoreSlim(1, 1);

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

        /// <summary>
        /// Creates communication handler.
        /// </summary>
        /// <param name="frameFactory">Frame factory.</param>
        /// <param name="connection">Current connection.</param>
        public Communication(IFrameFactory<F, T> frameFactory, IConnection<T> connection)
        {
            FrameFactory = frameFactory ?? throw new ArgumentNullException(nameof(frameFactory));
            FrameFactory.FrameValidated += FrameFactory_FrameValidated;

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Connection.Received += Connection_Received;
        }

        /// <inheritdoc/>
        public virtual event EventHandler<F> Received;

        /// <inheritdoc/>
        public IFrameFactory<F, T> FrameFactory { get; protected set; }

        /// <inheritdoc/>
        public IConnection<T> Connection { get; protected set; }

        /// <inheritdoc/>
        public async Task Send(F frame, CancellationToken cancelToken)
            => await Connection.Send(FrameFactory.FromFrame(frame), cancelToken);

        /// <inheritdoc/>
        public async Task<F> Receive(CancellationToken cancelToken)
        {
            bool waitingForResponse = true;
            F result = default;

            void OnReceived(object sender, F frame)
            {
                if (waitingForResponse)
                {
                    waitingForResponse = false;
                    result = frame;
                    semaphore.Release();
                }
            }

            await ReceivingSemaphore.WaitAsync(cancelToken);
            try
            {
                Received += OnReceived;
                await semaphore.WaitAsync(cancelToken);
            }
            finally
            {                
                Received -= OnReceived;
                while (semaphore.CurrentCount > 0)
                {
                    semaphore.Wait(); 
                }
                ReceivingSemaphore.Release();
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<F> Ask(F data, CancellationToken cancelToken)
        {
            bool receiving = false;
            bool waitingForResponse = true;
            F result = default;

            void OnReceived(object sender, F frame)
            {
                if (waitingForResponse)
                {
                    waitingForResponse = false;
                    result = frame;
                    semaphore.Release();
                }
            }

            await ReceivingSemaphore.WaitAsync(cancelToken);
            try
            {
                await Send(data, cancelToken);
                Received += OnReceived;
                receiving = true;
                await semaphore.WaitAsync(cancelToken);
            }
            finally
            {
                if (receiving) Received -= OnReceived;
                while (semaphore.CurrentCount > 0)
                {
                    semaphore.Wait();
                }
                ReceivingSemaphore.Release();
            }

            return result;
        }

        /// <summary>
        /// Handles frame validation.
        /// </summary>
        /// <param name="sender">Source of frame.</param>
        /// <param name="frame">Frame.</param>
        protected virtual void FrameFactory_FrameValidated(object sender, F frame)
        {
            Received?.Invoke(this, frame);
        }

        /// <summary>
        /// Handles new data received.
        /// </summary>
        /// <param name="sender">Source of data.</param>
        /// <param name="data">New data.</param>
        protected virtual void Connection_Received(object sender, T data)
        {
            FrameFactory.RegisterData(data);
        }

        #region IDispose

        // To detect redundant calls
        private bool disposed = false;

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
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Connection.Received -= Connection_Received;
                FrameFactory.FrameValidated -= FrameFactory_FrameValidated;
                ReceivingSemaphore.Dispose();
                semaphore.Dispose();
            }

            disposed = true;
        }

        #endregion IDispose
    }
}
