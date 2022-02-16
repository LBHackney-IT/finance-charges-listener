using FinanceChargesListener.Domain.EventMessages;
using System;
using System.Collections.Generic;

namespace FinanceChargesListener.Boundary
{
    public class ChargesEventSns
    {
        public Guid Id { get; set; }

        public Guid EntityId { get; set; }

        public Guid EntityTargetId { get; set; }

        public string EventType { get; set; }

        public string SourceDomain { get; set; }

        public string SourceSystem { get; set; }

        public string Version { get; set; }

        public Guid CorrelationId { get; set; }

        public DateTime DateTime { get; set; }

        public User User { get; set; }

        public ChargesEventData EventData { get; set; } = new ChargesEventData();
    }

    public class ChargesEventData
    {
        public List<DetailedChargeChange> NewData { get; set; }
    }
}
