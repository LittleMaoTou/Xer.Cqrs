using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Xer.Cqrs.CommandStack;
using Xer.DomainDriven.Repositories;
// using Xer.Cqrs.CommandStack.Attributes;

namespace Domain.Commands
{
    public class ActivateProductCommand
    {
        public Guid ProductId { get; }

        public ActivateProductCommand(Guid productId) 
        {
            ProductId = productId; 
        }
    }

    /// <summary>
    /// This handler can be registered either through Container, Basic or Attribute registration.
    /// In real projects, implementing only one of the interfaces or only using the [CommandHandler] attribute should do.
    /// </summary>
    public class ActivateProductCommandHandler : ICommandAsyncHandler<ActivateProductCommand>
    {
        private readonly IAggregateRootRepository<Product> _productRepository;

        public ActivateProductCommandHandler(IAggregateRootRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }
        
        // [CommandHandler] // To allow this method to be registered through attribute registration.
        public async Task HandleAsync(ActivateProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetByIdAsync(command.ProductId);
            if (product == null)
            {
                throw new ProductNotFoundException("Product not found.");
            }

            product.Activate();

            await _productRepository.SaveAsync(product);
        }
    }
}