using KBCommunication.Interfaces;
using KBCommunication.Routed.Interfaces;

namespace KBCommunication.Routed
{
    /// <inheritdoc/>
    public class RoutedCommunication<F> : Communication<F, F>
    {
        protected readonly ICommunicationRouter<F> router;
        protected readonly IFrameValidator<F> validator;

        /// <inheritdoc/>
        public RoutedCommunication(IFrameValidator<F> validator, ICommunicationRouter<F> router)
            : base(validator, router)
        {
            this.validator = validator;
            this.router = router;
            this.router.AddValidator(this.validator);
        }

        /// <inheritdoc/>
        protected override void Connection_Received(object sender, F data) { }

        #region IDisposable

        private bool _disposed = false;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                router.RemoveValidator(validator);
            }

            _disposed = true;

            base.Dispose(disposing);
        }

        #endregion IDisposable
    }
}
