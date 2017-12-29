using System;

namespace Xer.Cqrs.CommandStack.Resolvers
{
    public class ContainerCommandAsyncHandlerResolver : ICommandHandlerResolver
    {
        private readonly IContainerAdapter _containerAdapter;

        public ContainerCommandAsyncHandlerResolver(IContainerAdapter containerAdapter)
        {
            _containerAdapter = containerAdapter;
        }

        /// <summary>
        /// <para>Resolves an instance of ICommandAsyncHandler<TCommand> from the container</para>
        /// <para>and converts it to a command handler delegate which can be invoked to process the command.</para>
        /// </summary>
        /// <typeparamref name="TCommand">Type of command which is handled by the command handler.</typeparamref>
        /// <returns>Instance of <see cref="CommandHandlerDelegate"/> which executes the command handler processing.</returns>
        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand
        {
            try
            {
                // Try to resolve async handler first.
                ICommandAsyncHandler<TCommand> commandAsyncHandler = _containerAdapter.Resolve<ICommandAsyncHandler<TCommand>>();

                if (commandAsyncHandler == null)
                {
                    // No handlers are resolved. Throw exception.
                    throw NoCommandHandlerResolvedException<TCommand>();
                }

                return CommandHandlerDelegateBuilder.FromCommandHandler(commandAsyncHandler);
            }
            catch(Exception ex)
            {
                // No handlers are resolved. Throw exception.
                throw NoCommandHandlerResolvedException<TCommand>(ex);
            }
        }

        private static NoCommandHandlerResolvedException NoCommandHandlerResolvedException<TCommand>(Exception ex = null) where TCommand : class, ICommand
        {
            Type commandType = typeof(TCommand);

            if(ex != null)
            {
                return new NoCommandHandlerResolvedException($"Error occurred while trying to resolve command handler from the container to handle command of type: { commandType.Name }.", commandType, ex);
            }
            
            return new NoCommandHandlerResolvedException($"Unable to resolve command handler from the container to handle command of type: { commandType.Name }.", commandType, ex);
        }
    }
}