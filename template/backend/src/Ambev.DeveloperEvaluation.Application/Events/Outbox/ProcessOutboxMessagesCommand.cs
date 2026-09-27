using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Events.Outbox;

public sealed record ProcessOutboxMessagesCommand(int BatchSize = 20) : IRequest<int>;
