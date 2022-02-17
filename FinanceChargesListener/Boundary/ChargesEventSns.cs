using System;

namespace FinanceChargesListener.Boundary
{
    public class ChargesEventSns : EntityEventSns
    {
        public Guid EntityTargetId { get; set; }
    }
}
