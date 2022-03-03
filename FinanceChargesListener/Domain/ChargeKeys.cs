using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Domain
{
    public class ChargeKeys
    {
        public Guid Id { get; }
        public Guid TargetId { get; }

        public ChargeKeys(Guid id, Guid targetId)
        {
            Id = id;
            TargetId = targetId;
        }
    }
}
