﻿using System;
using Xer.Cqrs.EventSourcing.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Repositories
{
    public abstract class EventSourcedAggregateRepository<TAggregate> : IEventSourcedAggregateRepository<TAggregate> where TAggregate : EventSourcedAggregate
    {
        protected abstract IDomainEventStore<TAggregate> DomainEventStore { get; }

        public abstract TAggregate GetById(Guid aggregateId);
        public abstract TAggregate GetById(Guid aggregateId, int version);

        public virtual void Save(TAggregate aggregate)
        {
            DomainEventStore.Save(aggregate);
        }
    }
}
