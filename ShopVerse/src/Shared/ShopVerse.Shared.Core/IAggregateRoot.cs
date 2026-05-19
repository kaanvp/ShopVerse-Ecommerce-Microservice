using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Core
{
    /// <summary>
    /// Domain Driven Design (DDD) yaklaşımında Aggregate Root temel sınıfıdır.
    /// Aggregate içindeki domain event'leri yönetir ve kök entity olarak davranır.
    /// </summary>
    public interface IAggregateRoot { }
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
    public abstract class AggregateRoot : BaseEntity, IAggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
