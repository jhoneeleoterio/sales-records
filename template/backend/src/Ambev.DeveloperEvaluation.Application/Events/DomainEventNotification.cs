using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Events;

/// <summary>
/// Adapts a domain event to MediatR without coupling the domain layer to MediatR.
/// </summary>
public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
