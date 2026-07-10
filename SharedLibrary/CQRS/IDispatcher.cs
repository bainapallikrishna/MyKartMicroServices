using System.Threading;
using System.Threading.Tasks;

namespace SharedLibrary.CQRS
{
    public interface IDispatcher
    {
        Task<TResult> Send<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResult>;
    }
}
