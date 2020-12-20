using KBCommunication.Interfaces;
using KBCommunication.Routed;
using KBCommunication.Routed.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace KBCommunicationTest
{
    public class CommunicationRouterData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunicationRouter<int>>((f, c) => new CommunicationRouterMulticast<int>(f, c)) };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunicationRouter<int>>((f, c) => new CommunicationRouterSinglecast<int>(f, c)) };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
