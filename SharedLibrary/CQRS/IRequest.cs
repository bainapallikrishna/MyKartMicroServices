using System;

namespace SharedLibrary.CQRS
{
    // Marker for requests that return TResult
    public interface IRequest<TResult>
    {
    }

    public interface ICommand<TResult> : IRequest<TResult>
    {
    }

    public interface IQuery<TResult> : IRequest<TResult>
    {
    }
}
