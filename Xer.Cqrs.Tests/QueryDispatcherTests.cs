﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Dispatchers;
using Xer.Cqrs.Registrations.QueryHandlers;
using Xer.Cqrs.Tests.Mocks;
using Xer.Cqrs.Tests.Mocks.CommandHandlers;
using Xer.Cqrs.Tests.Mocks.QueryHandlers;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests
{
    public class QueryDispatcherTests
    {
        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public DispatchAsyncMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Dispatch_Query_To_Registered_Handler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsync, string>)new TestQueryHandler(_testOutputHelper));

                string data = "Test async message.";

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync(new QuerySomethingAsync(data));

                Assert.Equal(result, data);
            }

            [Fact]
            public async Task Dispatch_Query_Multiple_Times_To_Registered_Handler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)new TestQueryHandler(_testOutputHelper));
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingInteger, int>)new TestQueryHandler(_testOutputHelper));

                string data1 = "Test message 1.";
                string data2 = "Test message 2.";

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync(new QuerySomething(data1));
                var result2 = dispatcher.DispatchAsync(new QuerySomething(data2));
                var result3 = dispatcher.DispatchAsync(new QuerySomethingInteger(1));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal(await result1, data1);
                Assert.Equal(await result2, data2);
                Assert.Equal(await result3, 1);
            }

            [Fact]
            public async Task Dispatch_Query_To_Registered_Handler_With_CancellationToken()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsyncWithDelay, string>)new TestQueryHandler(_testOutputHelper));

                var cts = new CancellationTokenSource();

                var dispatcher = new QueryDispatcher(registration);

                string data = "Test async message with cancellation token.";

                var result = await dispatcher.DispatchAsync(new QuerySomethingAsyncWithDelay(data, 500), cts.Token);

                Assert.Equal(result, data);
            }

            public void Dispatch_Query_And_Cancel()
            {
                Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var registration = new QueryHandlerRegistration();
                    registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsyncWithDelay, string>)new TestQueryHandler(_testOutputHelper));

                    var cts = new CancellationTokenSource();

                    var dispatcher = new QueryDispatcher(registration);
                    Task task = dispatcher.DispatchAsync(new QuerySomethingAsyncWithDelay("This will be cancelled", 2000), cts.Token);

                    cts.Cancel();

                    await task;
                });
            }
        }

        public class DispatchMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public DispatchMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public void Dispatch_To_Registered_Query_Handler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomething, string>)new TestQueryHandler(_testOutputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch(new QuerySomething("Test message."));

                Assert.Equal(result, "Test message.");
            }

            [Fact]
            public async Task Dispatch_Query_With_Non_Reference_Type_Result()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomethingInteger, int>)new TestQueryHandler(_testOutputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync(new QuerySomethingInteger(1973));

                Assert.Equal(result, 1973);
            }
        }
    }
}
