using System;
using System.Collections.Generic;
using System.Text;
using FinanceChargesListener.Domain;

namespace FinanceChargesListener.Boundary.Response
{
    public class ChargeResponse
    {
        /// <summary>
        /// Id of charge model
        /// </summary>
        /// <example>
        /// 793dd4ca-d7c4-4110-a8ff-c58eac4b90a7
        /// </example>
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the appropriate tenure
        /// </summary>
        /// <example>
        /// 793dd4ca-d7c4-4110-a8ff-c58eac4b90f8
        /// </example>
        public Guid TargetId { get; set; }

        /// <summary>
        /// Values: [Asset, Tenure]
        /// </summary>
        /// <example>
        /// Asset
        /// </example>
        public TargetType TargetType { get; set; }

        /// <summary>
        /// Charge Group - Tenants/Leaseholders
        /// </summary>
        public ChargeGroup ChargeGroup { get; set; }

        /// <summary>
        /// Required only for ChargeGroup = Leaseholders
        /// Charge Sub Group - Estimate/Actual
        /// </summary>
        /// <example>Actual</example>
        public ChargeSubGroup? ChargeSubGroup { get; set; }

        /// <summary>
        /// Charge Year - 2022 - Charge Start Date Year
        /// </summary>
        public short ChargeYear { get; set; }

        /// <summary>
        /// Information about charges
        /// </summary>
        /// <example>
        ///     [
        ///         {
        ///             "Type":"A454",
        ///             "SubType":"a-5456",
        ///             "Frequency":"Weekly",
        ///             "Amount":1235.21,
        ///             "StartDate":2021-05-11,
        ///             "EndDate":2022-05-11
        ///         },
        ///         {
        ///             "Type":"A454",
        ///             "SubType":"a-5456",
        ///             "Frequency":"Weekly",
        ///             "Amount":1235.21,
        ///             "StartDate":2021-05-11,
        ///             "EndDate":2022-05-11
        ///         }
        ///     ]
        /// </example>
        public IEnumerable<DetailedCharges> DetailedCharges { get; set; }
    }
}
