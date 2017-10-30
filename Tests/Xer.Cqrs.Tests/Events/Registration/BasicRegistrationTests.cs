﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xer.Cqrs.Events;
using Xer.Cqrs.Events.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Events.Registration
{
    public class BasicRegistrationTests
    {
        #region Register Method

        public class RegisterMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public RegisterMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public void Should_Store_All_Event_Handlers()
            {
                var asyncHandler1 = new TestEventAsyncHandler1(_testOutputHelper);
                var asyncHandler2 = new TestEventAsyncHandler2(_testOutputHelper);
                var asyncHandler3 = new TestEventAsyncHandler3(_testOutputHelper);
                var handler1 = new TestEventHandler1(_testOutputHelper);
                var handler2 = new TestEventHandler2(_testOutputHelper);
                var handler3 = new TestEventHandler3(_testOutputHelper);

                var registration = new EventHandlerRegistration();
                registration.Register<TestEvent>(() => asyncHandler1);
                registration.Register<TestEvent>(() => asyncHandler2);
                registration.Register<TestEvent>(() => asyncHandler3);
                registration.Register<TestEvent>(() => handler1);
                registration.Register<TestEvent>(() => handler2);
                registration.Register<TestEvent>(() => handler3);

                IEnumerable<EventHandlerDelegate> eventHandlerDelegates = registration.ResolveEventHandlers<TestEvent>();

                Assert.Equal(6, eventHandlerDelegates.Count());
            }
        }

        #endregion Register Method
    }
}
