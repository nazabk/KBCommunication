using KBCommunication;
using KBCommunication.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KBCommunicationTest
{
    public class CommunicationUnitTest
    {
        [Theory]
        [ClassData(typeof(CommunicationSendData))]
        public void TestSend(Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor, int sendMessage)
        {
            // Arrange
            int data = default;
            CancellationToken ct = default;
            CancellationToken testCt = new CancellationToken();
            var connection = new Mock<IConnection<int>>();
            connection.Setup(x => x.Send(sendMessage, testCt))
                .Callback<int, CancellationToken>((x, y) =>
                {
                    data = x;
                    ct = y;
                })
                .Returns(Task.CompletedTask);

            var validator = new FrameValidator<int>(x => true);
            var communication = constructor(validator, connection.Object);

            // Act
            communication.Send(sendMessage, testCt).Wait();

            // Assert
            Assert.Equal(sendMessage, data);
            Assert.Equal(testCt, ct);
        }

        [Theory]
        [ClassData(typeof(CommunicationSendData))]
        public void TestSendCancelled(Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor, int sendMessage)
        {
            // Arrange
            CancellationToken testCt = new CancellationToken(true);
            var connection = new Mock<IConnection<int>>();
            connection.Setup(x => x.Send(sendMessage, testCt)).ThrowsAsync(new OperationCanceledException());

            var validator = new FrameValidator<int>(x => true);
            var communication = constructor(validator, connection.Object);

            // Act
            // Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await communication.Send(sendMessage, testCt));
        }

        [Theory]
        [ClassData(typeof(CommunicationSendData))]
        public void TestReceived(Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor, int sendMessage)
        {
            // Arrange
            int result = 0;
            object sender = null;
            var connection = new Mock<IConnection<int>>();

            var validator = new FrameValidator<int>(x => true);
            var communication = constructor(validator, connection.Object);
            communication.Received += (s, i) =>
            {
                sender = s;
                result = i;
            };

            // Act
            connection.Raise(x => x.Received += null, connection, sendMessage);

            // Assert
            Assert.Equal(sendMessage, result);
            Assert.Equal(communication, sender);
        }

        [Theory]
        [ClassData(typeof(UnReceivedCommunicationSendData))]
        public void TestReceivedFailed(Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor, int sendMessage)
        {
            // Arrange
            int result = 0;
            object sender = null;
            bool receivedRaised = false;
            var connection = new Mock<IConnection<int>>();

            var validator = new FrameValidator<int>(x => false);
            var com = constructor(validator, connection.Object);

            validator.ValidationFailed += (s, e) =>
            {
                sender = s;
                result = e;
            };
            com.Received += (s, e) => receivedRaised = true;

            // Act
            connection.Raise(x => x.Received += null, connection, sendMessage);

            // Assert
            Assert.Equal(sendMessage, result);
            Assert.Equal(validator, sender);
            Assert.False(receivedRaised);
        }

        [Theory]
        [ClassData(typeof(CommunicationSendData))]
        public void TestReceive(Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor, int sendMessage)
        {
            // Arrange
            CancellationToken testCt = new CancellationToken();
            var connection = new Mock<IConnection<int>>();

            var validator = new FrameValidator<int>(x => true);
            var com = constructor(validator, connection.Object);

            // Act
            var run = com.Receive(testCt);
            Task.Yield();
            connection.Raise(x => x.Received += null, connection, sendMessage);

            // Assert
            Assert.Equal(sendMessage, run.Result);
        }

        [Theory]
        [ClassData(typeof(CommunicationSendData))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public void TestReceiveCancelled(Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor, int sendMessage)
        {
            // Arrange
            CancellationToken testCt = new CancellationToken(true);
            var connection = new Mock<IConnection<int>>();
            var validator = new FrameValidator<int>(x => true);
            var com = constructor(validator, connection.Object);

            // Act
            // Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await com.Receive(testCt));
        }

        [Theory]
        [ClassData(typeof(CommunicationConcurrentSendData))]
        public void TestConcurrentReceive(
            Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor,
            int[] messages)
        {
            // Arrange
            CancellationToken testCt = new CancellationToken();
            var connection = new Mock<IConnection<int>>();

            var validator = new FrameValidator<int>(x => true);
            var com = constructor(validator, connection.Object);

            var runs = new List<Task<int>>();

            // Act
            var sp = new SpinWait();

            foreach (var request in messages)
            {
                var run = Task.Run(delegate { return com.Receive(testCt).Result; });                
                while (run.Status != TaskStatus.Running) sp.SpinOnce();
                runs.Add(run);
            }

            for (int i = 0; i < runs.Count; i++)
            {
                connection.Raise(x => x.Received += null, connection, messages[i]);                
                while (runs.Skip(i).All(x => x.Status == TaskStatus.Running)) sp.SpinOnce();
            }

            // Assert
            for (int i = 0; i < runs.Count; i++)
            {
                Assert.Equal(messages[i], runs[i].Result);
            }
        }

        [Theory]
        [ClassData(typeof(CommunicationAskingData))]
        public void TestAsk(Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor, int request, int response)
        {
            // Arrange
            CancellationToken testCt = new CancellationToken();
            var connection = new Mock<IConnection<int>>();
            connection.Setup(x => x.Send(request, testCt))
                .Returns(Task.CompletedTask)
                .Callback(delegate
                {
                    Task.Run(delegate
                    {
                        Task.Yield();
                        connection.Raise(x => x.Received += null, connection, response);
                    });
                });

            var validator = new FrameValidator<int>(x => true);
            var com = constructor(validator, connection.Object);

            // Act
            var run = com.Ask(request, testCt);

            // Assert
            Assert.Equal(response, run.Result);
        }

        [Theory]
        [ClassData(typeof(CommunicationAskingData))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public void TestAskCancelledOnSending(Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor, int request, int response)
        {
            // Arrange
            CancellationToken testCt = new CancellationToken(true);
            var connection = new Mock<IConnection<int>>();
            connection.Setup(x => x.Send(request, testCt))
                .ThrowsAsync(new IOException());

            var validator = new FrameValidator<int>(x => true);
            var com = constructor(validator, connection.Object);

            // Act
            var run = com.Ask(request, testCt);

            // Assert
            Assert.ThrowsAsync<IOException>(async () => await com.Ask(request, testCt));
        }

        [Theory]
        [ClassData(typeof(CommunicationAskingData))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public void TestAskCancelled(Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor, int request, int response)
        {
            // Arrange
            CancellationToken testCt = new CancellationToken(true);
            var connection = new Mock<IConnection<int>>();
            connection.Setup(x => x.Send(request, testCt))
                .Returns(Task.CompletedTask);

            var validator = new FrameValidator<int>(x => true);
            var com = constructor(validator, connection.Object);

            // Act
            var run = com.Ask(request, testCt);

            // Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await com.Ask(request, testCt));
        }

        [Theory]
        [ClassData(typeof(CommunicationConcurrentAskingData))]
        public void TestConcurrentAsk(
            Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>> constructor,
            int[] requests,
            int[] responses)
        {
            // Arrange
            CancellationToken testCt = new CancellationToken();
            var connection = new Mock<IConnection<int>>();

            var validator = new FrameValidator<int>(x => true);
            var com = constructor(validator, connection.Object);

            var runs = new List<Task<int>>();

            // Act
            var sp = new SpinWait();

            foreach (var request in requests)
            {
                var run = Task.Run(delegate { return com.Ask(request, testCt).Result; });
                while (run.Status != TaskStatus.Running) sp.SpinOnce();
                runs.Add(run);
            }

            for (int i = 0; i < runs.Count; i++)
            {
                connection.Raise(x => x.Received += null, connection, responses[i]);
                while (runs.Skip(i).All(x => x.Status == TaskStatus.Running)) sp.SpinOnce();
            }

            // Assert
            for (int i = 0; i < runs.Count; i++)
            {
                Assert.Equal(responses[i], runs[i].Result);
            }
        }
    }
}
