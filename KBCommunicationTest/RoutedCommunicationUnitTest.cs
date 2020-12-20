using KBCommunication;
using KBCommunication.Interfaces;
using KBCommunication.Routed;
using KBCommunication.Routed.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace KBCommunicationTest
{
    public class RoutedCommunicationUnitTest
    {
        [Theory]
        [ClassData(typeof(CommunicationRouterData))]
        public void TestRouted1(Func<IFrameValidator<int>, IConnection<int>, ICommunicationRouter<int>> constructor)
        {
            // Arrange
            List<int> received = new List<int>();
            List<int> tmp = new List<int>();
            var connection = new Mock<IConnection<int>>();
            var router = constructor(new FrameValidator<int>(x => true), connection.Object);
            var route = new FrameValidator<int>(x => x % 2 == 0);
            var rotued = new RoutedCommunication<int>(route, router);
            rotued.Received += (s, e) => received.Add(e);
            router.Received += (s, e) => tmp.Add(e);

            // Act
            connection.Raise(x => x.Received += null, connection, 1);
            connection.Raise(x => x.Received += null, connection, 2);
            connection.Raise(x => x.Received += null, connection, 3);
            connection.Raise(x => x.Received += null, connection, 4);

            // Assert
            Assert.Equal(new int[] { 2, 4 }, received);
            Assert.Equal(new int[] { 1, 3 }, tmp);
        }

        [Theory]
        [ClassData(typeof(CommunicationRouterData))]
        public void TestRouted2(Func<IFrameValidator<int>, IConnection<int>, ICommunicationRouter<int>> constructor)
        {
            // Arrange
            List<int> received1 = new List<int>();
            List<int> received2 = new List<int>();
            List<int> tmp = new List<int>();
            var connection = new Mock<IConnection<int>>();
            var router = constructor(new FrameValidator<int>(x => true), connection.Object);
            var route1 = new FrameValidator<int>(x => x % 3 == 0);
            var route2 = new FrameValidator<int>(x => x % 3 == 1);
            var rotued1 = new RoutedCommunication<int>(route1, router);
            var rotued2 = new RoutedCommunication<int>(route2, router);
            rotued1.Received += (s, e) => received1.Add(e);
            rotued2.Received += (s, e) => received2.Add(e);
            router.Received += (s, e) => tmp.Add(e);

            // Act
            connection.Raise(x => x.Received += null, connection, 1);
            connection.Raise(x => x.Received += null, connection, 2);
            connection.Raise(x => x.Received += null, connection, 3);
            connection.Raise(x => x.Received += null, connection, 4);
            connection.Raise(x => x.Received += null, connection, 5);
            connection.Raise(x => x.Received += null, connection, 6);

            // Assert
            Assert.Equal(new int[] { 3, 6 }, received1);
            Assert.Equal(new int[] { 1, 4 }, received2);
            Assert.Equal(new int[] { 2, 5 }, tmp);
        }
    }
}
