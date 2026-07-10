using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SharedLibrary.CQRS
{
    // A dispatcher that resolves handlers from the IServiceProvider per request scope.
    public class InMemoryDispatcher : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, Type> _handlerTypes = new();

        public InMemoryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        // Register mapping from request type to handler type. Handler must be registered in DI.
        public void RegisterHandler<TRequest, TResult, THandler>()
            where TRequest : IRequest<TResult>
            where THandler : IRequestHandler<TRequest, TResult>
        {
            _handlerTypes[typeof(TRequest)] = typeof(THandler);
        }

        public async Task<TResult> Send<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResult>
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (!_handlerTypes.TryGetValue(typeof(TRequest), out var handlerType))
                throw new InvalidOperationException($"No handler type registered for {typeof(TRequest).FullName}");

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetService(typeof(IRequestHandler<TRequest, TResult>)) as IRequestHandler<TRequest, TResult>;
            if (handler == null)
                throw new InvalidOperationException($"Handler for {typeof(TRequest).FullName} could not be resolved from DI. Ensure it is registered.");

            return await handler.Handle(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
