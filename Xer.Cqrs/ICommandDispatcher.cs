﻿namespace Xer.Cqrs
{
    /// <summary>
    /// Process commands and delegates to handlers.
    /// </summary>
    public interface ICommandDispatcher
    {
        void Dispatch(ICommand command);
        void RegisterHandler<TCommand>(ICommandHandler<TCommand> commandHandler) where TCommand : ICommand;
    }
}
