// Trigger CI/CD pipeline for all services
using MediatR;

namespace BuildingBlocks.CQRS;
public interface IQuery<out TResponse> : IRequest<TResponse>  
    where TResponse : notnull
{
}
