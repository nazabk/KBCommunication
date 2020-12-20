using KBCommunication.Interfaces;
using KBCommunication.Routed.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace KBCommunication.Routed
{
    /// <inheritdoc/>
    public class CommunicationRouterMulticast<F> : CommunicationRouter<F>
    {
        /// <inheritdoc/>
        public CommunicationRouterMulticast(IFrameFactory<F, F> frameFactory, IConnection<F> connection)
            : base(frameFactory, connection)
        { }

        /// <inheritdoc/>
        protected override void FrameFactory_FrameValidated(object sender, F frame)
        {
            List<IFrameValidator<F>> routing = new List<IFrameValidator<F>>();
            lock (LockObject)
            {
                routing.AddRange(Routing.Where(x => x.Validate(frame)));
            }
            if (routing.Any())
            {
                foreach (var route in routing)
                {
                    route.RegisterData(frame);
                }
            }
            else
            {
                base.FrameFactory_FrameValidated(sender, frame);
            }
        }
    }
}
