﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Dispatchers
{
    public class BackgroundCommandDispatcher : ICommandDispatcher, ICommandAsyncDispatcher
    {
        private readonly ICommandHandlerResolver _resolver;

        public BackgroundCommandDispatcher(ICommandHandlerResolver provider) 
        {
            _resolver = provider;
        }

        /// <summary>
        /// Dispatch the command to the registered command handlers in the background.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        public void Dispatch<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            DispatchAsync(command).ContinueWith(t => t.Await());
        }

        /// <summary>
        /// Dispatch the command to the registered command handlers in the background.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to dispatch.</typeparam>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        public Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            return Task.Run(() =>
            {
                CommandHandlerDelegate commandHandlerDelegate = _resolver.ResolveCommandHandler<TCommand>();

                if (commandHandlerDelegate == null)
                {
                    throw new CommandNotHandledException($"No command handler is registered to handle command of type: {typeof(TCommand).Name}.");
                }

                return commandHandlerDelegate.Invoke(command, cancellationToken);
            });
        }
    }
}
