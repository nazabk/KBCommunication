using KBCommunication.Interfaces;
using KBCommunication.Routed.Interfaces;
using System.Linq;

namespace KBCommunication.Routed
{
    /// <inheritdoc cref="ICommunicationRouter{F}"/>
    public class CommunicationRouterSinglecast<F> : CommunicationRouter<F>
    {
        /// <inheritdoc/>
        public CommunicationRouterSinglecast(IFrameFactory<F, F> frameFactory, IConnection<F> connection)
            : base(frameFactory, connection)
        { }

        /// <inheritdoc/>
        protected override void FrameFactory_FrameValidated(object sender, F frame)
        {
            IFrameValidator<F> routing = null;
            lock (LockObject)
            {
                if (Routing.FirstOrDefault(x => x.Validate(frame)) is IFrameValidator<F> route)
                {
                    Routing.Remove(route);
                    Routing.Add(route);
                    routing = route;
                }
            }

            if (routing != null)
            {
                routing.RegisterData(frame);
            }
            else
            {
                base.FrameFactory_FrameValidated(sender, frame);
            }
        }
    }
}
