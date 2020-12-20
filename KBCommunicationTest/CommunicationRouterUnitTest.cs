using KBCommunication;
using KBCommunication.Interfaces;
using KBCommunication.Routed.Interfaces;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace KBCommunicationTest
{
    public class CommunicationRouterUnitTest
    {
        [Theory]
        [ClassData(typeof(CommunicationRouterData))]
        public void TestAddValidator(Func<IFrameValidator<int>, IConnection<int>, ICommunicationRouter<int>> constructor)
        {
            // Arrange
            var connection = new Mock<IConnection<int>>();
            var router = constructor(new FrameValidator<int>(x => true), connection.Object);
            var route = new FrameValidator<int>(x => true);

            // Act
            router.AddValidator(route);

            // Assert
            Assert.True(router.CheckValidator(route));
        }

        [Theory]
        [ClassData(typeof(CommunicationRouterData))]
        public void TestRemoveValidator(Func<IFrameValidator<int>, IConnection<int>, ICommunicationRouter<int>> constructor)
        {
            // Arrange
            var connection = new Mock<IConnection<int>>();
            var router = constructor(new FrameValidator<int>(x => true), connection.Object);
            var route = new FrameValidator<int>(x => true);

            // Act
            router.AddValidator(route);
            router.RemoveValidator(route);

            // Assert
            Assert.False(router.CheckValidator(route));
        }

        [Theory]
        [ClassData(typeof(CommunicationRouterData))]
        public void TestClearRoute(Func<IFrameValidator<int>, IConnection<int>, ICommunicationRouter<int>> constructor)
        {
            // Arrange
            var connection = new Mock<IConnection<int>>();
            var router = constructor(new FrameValidator<int>(x => true), connection.Object);
            var route = new FrameValidator<int>(x => true);

            // Act
            router.AddValidator(route);
            router.ClearValidators();

            // Assert
            Assert.False(router.CheckValidator(route));
        }
    }
}
