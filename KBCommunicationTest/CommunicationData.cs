using KBCommunication;
using KBCommunication.Interfaces;
using KBCommunication.Routed;
using System;
using System.Collections;
using System.Collections.Generic;

namespace KBCommunicationTest
{
    public class CommunicationSendData : IEnumerable<object[]>
    {
        private const int message = 1;

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new Communication<int, int>(f, c)), message };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterMulticast<int>(f, c)), message };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterSinglecast<int>(f, c)), message };
            yield return new object[] {
                new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>(
                    (f, c) => new RoutedCommunication<int>(f, new CommunicationRouterSinglecast<int>(new FrameValidator<int>(x => true), c))),
                message };
            yield return new object[] {
                new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>(
                    (f, c) => new RoutedCommunication<int>(f, new CommunicationRouterMulticast<int>(new FrameValidator<int>(x => true), c))),
                message };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class UnReceivedCommunicationSendData : IEnumerable<object[]>
    {
        private const int message = 1;

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new Communication<int, int>(f, c)), message };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterMulticast<int>(f, c)), message };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterSinglecast<int>(f, c)), message };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class CommunicationConcurrentSendData : IEnumerable<object[]>
    {
        readonly int[] messages = new int[] { 1, 2, 3 };

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new Communication<int, int>(f, c)), messages };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterMulticast<int>(f, c)), messages };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterSinglecast<int>(f, c)), messages };
            yield return new object[] {
                new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>(
                    (f, c) => new RoutedCommunication<int>(f, new CommunicationRouterSinglecast<int>(new FrameValidator<int>(x => true), c))),
                messages };
            yield return new object[] {
                new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>(
                    (f, c) => new RoutedCommunication<int>(f, new CommunicationRouterMulticast<int>(new FrameValidator<int>(x => true), c))),
                messages };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class CommunicationAskingData : IEnumerable<object[]>
    {
        private const int request = 1, response = 2;
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new Communication<int, int>(f, c)), request, response };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterMulticast<int>(f, c)), request, response };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterSinglecast<int>(f, c)), request, response };
            yield return new object[] {
                new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>(
                    (f, c) => new RoutedCommunication<int>(f, new CommunicationRouterSinglecast<int>(new FrameValidator<int>(x => true), c))),
                request,
                response };
            yield return new object[] {
                new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>(
                    (f, c) => new RoutedCommunication<int>(f, new CommunicationRouterMulticast<int>(new FrameValidator<int>(x => true), c))),
                request,
                response };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class CommunicationConcurrentAskingData : IEnumerable<object[]>
    {
        readonly int[] requests = new int[] { 1, 2, 3 };
        readonly int[] responses = new int[] { 4, 5, 6 };

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new Communication<int, int>(f, c)), new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 } };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterMulticast<int>(f, c)), new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 } };
            yield return new object[] { new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>((f, c) => new CommunicationRouterSinglecast<int>(f, c)), new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 } };
            yield return new object[] {
                new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>(
                    (f, c) => new RoutedCommunication<int>(f, new CommunicationRouterSinglecast<int>(new FrameValidator<int>(x => true), c))),
                requests,
                responses };
            yield return new object[] {
                new Func<IFrameValidator<int>, IConnection<int>, ICommunication<int, int>>(
                    (f, c) => new RoutedCommunication<int>(f, new CommunicationRouterMulticast<int>(new FrameValidator<int>(x => true), c))),
                requests,
                responses };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
