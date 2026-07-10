using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLibrary.CQRS
{
    public interface IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        Task<TResult> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}
