﻿using System;
using System.Collections.Generic;
using Xer.DomainDriven.EventSourcing.DomainEvents;
using Xer.DomainDriven.EventSourcing.Exceptions;

namespace Xer.DomainDriven.EventSourcing
{
    public abstract class EventSourcedAggregate : Aggregate
    {
        private readonly Queue<IDomainEvent> _uncommittedDomainEvents = new Queue<IDomainEvent>();
        private readonly DomainEventApplierRegistration _domainEventApplierRegistration = new DomainEventApplierRegistration();

        /// <summary>
        /// Current version of this aggregate.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="aggregateId">Id of this entity.</param>
        public EventSourcedAggregate(Guid aggregateId) 
            : base(aggregateId)
        {
            RegisterDomainEventAppliers(_domainEventApplierRegistration);
        }

        /// <summary>
        /// Constructor to build this entity from the domain event stream.
        /// </summary>
        /// <param name="history">Domain event stream.</param>
        public EventSourcedAggregate(DomainEventStream history)
            : this(Guid.Empty)
        {
            if(history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            // History events are events that are already saved to event store.
            // So, just invoke the applier without tracking events.
            foreach (IDomainEvent domainEvent in history)
            {
                InvokeDomainEventApplier(domainEvent, false);
            }
        }

        // <summary>
        // Clear all internally tracked domain events.
        // </summary>
        internal void ClearUncommitedDomainEvents()
        {
            _uncommittedDomainEvents.Clear();
        }

        /// <summary>
        /// Get an event stream of all the uncommitted domain events applied to this entity.
        /// This will also clear out all internally tracked domain events.
        /// </summary>
        /// <returns>Stream of uncommitted domain events.</returns>
        internal DomainEventStream GetUncommitedDomainEvents()
        {
            return new DomainEventStream(Id, _uncommittedDomainEvents);
        }

        /// <summary>
        /// Register actions to apply certain domain events.
        /// </summary>
        /// <param name="applierRegistration">Domain event applier registration.</param>
        protected abstract void RegisterDomainEventAppliers(DomainEventApplierRegistration applierRegistration);

        /// <summary>
        /// Apply domain event to this entity and mark domain event for commit.
        /// </summary>
        /// <typeparam name="TDomainEvent">Type of domain event to apply.</typeparam>
        /// <param name="domainEvent">Instance of domain event to apply.</param>
        protected void ApplyChange<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            // Invoke and track the event to save to event store.
            InvokeDomainEventApplier(domainEvent);
        }

        /// <summary>
        /// Invoke the registered action to handle the domain event.
        /// </summary>
        /// <typeparam name="TDomainEvent">Type of the domain event to handle.</typeparam>
        /// <param name="domainEvent">Domain event instance to handle.</param>
        /// <param name="markDomainEventForCommit">True, if domain event should be marked/tracked for commit. Otherwise, false.</param>
        protected void InvokeDomainEventApplier<TDomainEvent>(TDomainEvent domainEvent, bool markDomainEventForCommit = true) where TDomainEvent : IDomainEvent
        {
            Action<IDomainEvent> domainEventApplier = _domainEventApplierRegistration.GetDomainEventApplier(domainEvent);
            if (domainEventApplier == null)
            {
                throw new DomainEventNotAppliedException(domainEvent, 
                    $"{GetType().Name} is not configured to apply domain event of type {domainEvent.GetType().Name}");
            }

            try
            {
                domainEventApplier.Invoke(domainEvent);

                // Bump up version.
                Version++;

                // Updated.
                Updated = DateTime.Now;

                if (markDomainEventForCommit)
                {
                    MarkAppliedDomainEventForCommit(domainEvent);
                }
            }
            catch(Exception ex)
            {
                throw new DomainEventNotAppliedException(domainEvent,
                    $"Exception occured while trying to apply domain event of type {domainEvent.GetType().Name}.",
                    ex);
            }
        }

        /// <summary>
        /// Add domain event to list of tracked domain events.
        /// </summary>
        /// <param name="domainEvent">Domain event instance to track.</param>
        private void MarkAppliedDomainEventForCommit(IDomainEvent domainEvent)
        {
            _uncommittedDomainEvents.Enqueue(domainEvent);
        }

        #region Domain Event Handler Registrations

        /// <summary>
        /// Holds the actions to be executed in handling specific types of domain event.
        /// </summary>
        protected class DomainEventApplierRegistration
        {
            private readonly IDictionary<Type, Action<IDomainEvent>> _appliersByDomainEventType = new Dictionary<Type, Action<IDomainEvent>>();

            public void RegisterDomainEventApplier<TDomainEvent>(Action<TDomainEvent> applier) where TDomainEvent : IDomainEvent
            {
                Type domainEventType = typeof(TDomainEvent);

                Action<IDomainEvent> domainEventApplier = (d) => applier.Invoke((TDomainEvent)d);

                _appliersByDomainEventType.Add(domainEventType, domainEventApplier);
            }

            public Action<IDomainEvent> GetDomainEventApplier(IDomainEvent domainEvent)
            {
                Action<IDomainEvent> actionHandler;

                _appliersByDomainEventType.TryGetValue(domainEvent.GetType(), out actionHandler);

                return actionHandler;
            }
        }

        #endregion Domain Event Handler Registrations
    }
}
