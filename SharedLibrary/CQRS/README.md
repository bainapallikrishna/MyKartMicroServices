CQRS helpers provided by SharedLibrary

Files:
- IRequest.cs: marker interfaces for ICommand/IQuery
- IRequestHandler.cs: handler interface
- InMemoryDispatcher.cs: very small runtime dispatcher to register handlers and send requests

Usage:
- Register handlers in startup and keep a single InMemoryDispatcher instance (e.g., in DI).
- Use dispatcher.Send(request) to execute commands/queries.
