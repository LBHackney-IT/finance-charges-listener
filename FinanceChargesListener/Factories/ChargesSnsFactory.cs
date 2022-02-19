using FinanceChargesListener.Boundary;
using System;

namespace FinanceChargesListener.Factories
{
    public static class ChargesSnsFactory
    {
        public static ChargesSns CreateFileUploadMessage(EntityFileMessageSqs messageSqs)
        {
            return new ChargesSns
            {
                CorrelationId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                EntityId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                EventType = EventTypes.FileUploadEvent,
                Version = Constants.V1VERSION,
                SourceDomain = Constants.SOURCEDOMAIN,
                SourceSystem = Constants.SOURCESYSTEM,
                EventData = new EventData
                {
                    NewData = messageSqs
                },
                User = new User { Name = Constants.NAME, Email = Constants.EMAIL }
            };
        }
    }
}
