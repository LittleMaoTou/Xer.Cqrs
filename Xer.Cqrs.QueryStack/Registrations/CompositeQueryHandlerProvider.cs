﻿using System;
using System.Collections.Generic;

namespace Xer.Cqrs.QueryStack.Registrations
{
    public class CompositeQueryHandlerProvider : IQueryHandlerProvider
    {
        private readonly IEnumerable<IQueryHandlerProvider> _providers;

        public CompositeQueryHandlerProvider(IEnumerable<IQueryHandlerProvider> providers)
        {
            _providers = providers;
        }

        /// <summary>
        /// Get the registered query handler delegate to handle the query of the specified type.
        /// </summary>
        /// <param name="queryType">Type of query to be handled.</param>
        /// <returns>Instance of invokeable QueryAsyncHandlerDelegate.</returns>
        public QueryHandlerDelegate<TResult> GetQueryHandler<TResult>(Type queryType)
        {
            foreach (IQueryHandlerProvider provider in _providers)
            {
                QueryHandlerDelegate<TResult> handlerDelegate = provider.GetQueryHandler<TResult>(queryType);
                if (handlerDelegate != null)
                {
                    return handlerDelegate;
                }
            }

            throw new QueryNotHandledException($"No query handler is registered to handle query of type: { queryType.Name }");
        }
    }
}
