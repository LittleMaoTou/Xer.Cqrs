﻿using SimpleInjector;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.QueryStack.Resolvers;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Queries
{
    public class QueryDispatcherTests
    {
        #region DispatchAsync Method

        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public DispatchAsyncMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsync, string>)new TestQueryHandler(_testOutputHelper));

                const string data = nameof(Should_Invoke_Registered_Query_Handler);

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomethingAsync, string>(new QuerySomethingAsync(data));

                Assert.Equal(result, data);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_When_Dispatched_Multiple_Times()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)new TestQueryHandler(_testOutputHelper));
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingNonReferenceType, int>)new TestQueryHandler(_testOutputHelper));

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.DispatchAsync<QuerySomethingNonReferenceType, int>(new QuerySomethingNonReferenceType(1));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal(data1, await result1);
                Assert.Equal(data2, await result2);
                Assert.Equal(1, await result3);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_With_Cancellation_Token()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsyncWithDelay, string>)new TestQueryHandler(_testOutputHelper));

                var cts = new CancellationTokenSource();

                var dispatcher = new QueryDispatcher(registration);

                const string data = nameof(Should_Invoke_Registered_Query_Handler_With_Cancellation_Token);

                var result = await dispatcher.DispatchAsync<QuerySomethingAsyncWithDelay, string>(new QuerySomethingAsyncWithDelay(data, 500), cts.Token);

                Assert.Equal(data, result);
            }
            
            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_In_Container()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);

                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomethingAsync, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);

                const string data = nameof(Should_Invoke_Registered_Query_Handler_In_Container);

                var dispatcher = new QueryDispatcher(resolver);
                var result = await dispatcher.DispatchAsync<QuerySomethingAsync, string>(new QuerySomethingAsync(data));

                Assert.Equal(data, result);
            }

            [Fact]
            public void Should_Throw_When_Cancelled()
            {
                Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var registration = new QueryHandlerRegistration();
                    registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsyncWithDelay, string>)new TestQueryHandler(_testOutputHelper));

                    var cts = new CancellationTokenSource();

                    var dispatcher = new QueryDispatcher(registration);
                    Task task = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomethingAsyncWithDelay("This will be cancelled", 2000), cts.Token);

                    cts.Cancel();

                    await task;
                });
            }

            [Fact]
            public Task Should_Propagate_Exception_From_Query_Handler()
            {
                return Assert.ThrowsAnyAsync<Exception>(async () =>
                {
                    try
                    {
                        var registration = new QueryHandlerRegistration();
                        registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithException, string>)new TestQueryHandler(_testOutputHelper));

                        var dispatcher = new QueryDispatcher(registration);

                        await dispatcher.DispatchAsync<QuerySomethingWithException, string>(new QuerySomethingWithException("This will cause an exception."));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion DispatchAsync Method

        #region Dispatch Method

        public class DispatchMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public DispatchMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public void Should_Invoke_Registered_Query_Handler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomething, string>)new TestQueryHandler(_testOutputHelper));

                const string data = nameof(Should_Invoke_Registered_Query_Handler);

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                Assert.Equal(data, result);
            }

            [Fact]
            public void Should_Invoke_Registered_Query_Handler_In_Container()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);

                var container = new Container();
                container.Register<IQueryHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);

                const string data = nameof(Should_Invoke_Registered_Query_Handler_In_Container);

                var dispatcher = new QueryDispatcher(resolver);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                Assert.Equal(data, result);
            }

            [Fact]
            public async Task Should_Allow_Non_Reference_Type_Return_Types()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomethingNonReferenceType, int>)new TestQueryHandler(_testOutputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomethingNonReferenceType, int>(new QuerySomethingNonReferenceType(1973));

                Assert.Equal(1973, result);
            }

            [Fact]
            public void Should_Propagate_Exception_From_Query_Handler()
            {
                Assert.ThrowsAny<Exception>(() =>
                {
                    try
                    {
                        var registration = new QueryHandlerRegistration();
                        registration.Register(() => (IQueryHandler<QuerySomethingWithException, string>)new TestQueryHandler(_testOutputHelper));

                        var dispatcher = new QueryDispatcher(registration);

                        dispatcher.Dispatch<QuerySomethingWithException, string>(new QuerySomethingWithException("This will cause an exception."));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion Dispatch Method
    }
}
