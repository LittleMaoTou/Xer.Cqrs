﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Xer.Cqrs.Dispatchers.CommandHandlerRegistration;

namespace Xer.Cqrs.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly CommandHandlerRegistration _registration;

        public CommandDispatcher(CommandHandlerRegistration registration)
        {
            _registration = registration;
        }

        public virtual void Dispatch(ICommand command)
        {
            Type comandType = command.GetType();

            IEnumerable<HandleCommandDelegate> handleCommandAsyncDelegates = _registration.GetRegisteredCommandHandlers(comandType);

            List<Task> tasks = new List<Task>(handleCommandAsyncDelegates.Count());

            foreach (HandleCommandDelegate handleCommandDelegate in handleCommandAsyncDelegates)
            {
                Task task = handleCommandDelegate.Invoke(command);
                task.ConfigureAwait(false);

                tasks.Add(task);
            }

            // Wait synchronously.
            Task.WhenAll(tasks).GetAwaiter().GetResult();
        }
    }
}
