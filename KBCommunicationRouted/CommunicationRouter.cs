using KBCommunication.Interfaces;
using KBCommunication.Routed.Interfaces;
using System.Collections.Generic;

namespace KBCommunication.Routed
{
    /// <inheritdoc cref="ICommunicationRouter{F}"/>
    public abstract class CommunicationRouter<F> : Communication<F, F>, ICommunicationRouter<F>
    {
        /// <summary>
        /// Routing lock object.
        /// </summary>
        protected readonly object LockObject = new object();

        /// <summary>
        /// List of known routes.
        /// </summary>
        protected List<IFrameValidator<F>> Routing = new List<IFrameValidator<F>>();

        /// <inheritdoc/>
        protected CommunicationRouter(IFrameFactory<F, F> frameFactory, IConnection<F> connection)
            : base(frameFactory, connection)
        { }

        /// <inheritdoc/>
        public void AddValidator(IFrameValidator<F> validator)
        {
            lock (LockObject)
            {
                Routing.Add(validator);
            }
        }

        /// <inheritdoc/>
        public bool CheckValidator(IFrameValidator<F> validator)
        {            
            lock (LockObject)
            {
                return Routing.Contains(validator);
            }
        }

        /// <inheritdoc/>
        public void ClearValidators()
        {
            lock (LockObject)
            {
                Routing.Clear();
            }
        }

        /// <inheritdoc/>
        public void RemoveValidator(IFrameValidator<F> validator)
        {
            lock (LockObject)
            {
                Routing.Remove(validator);
            }
        }
    }
}
