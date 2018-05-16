using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Commands;
using Xer.Cqrs.CommandStack;
using Xer.Delegator;

namespace ConsoleApp.UseCases
{
    public class DeactivateProductUseCase : UseCaseBase
    {
        private readonly CommandDelegator _commandDelegator;

        public override string Name => "DeactivateProduct";

        public DeactivateProductUseCase(CommandDelegator commandDispatcher)
        {
            _commandDelegator = commandDispatcher;    
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string productId = RequestInput("Enter product ID:", input =>
            {
                if (Guid.TryParse(input, out Guid i))
                {
                    return InputValidationResult.Success;
                }

                return InputValidationResult.WithErrors("Invalid product ID.");
            });
            
            await _commandDelegator.SendAsync(new DeactivateProductCommand(Guid.Parse(productId)));

            System.Console.WriteLine("Product deactivated.");
        }
    }
}