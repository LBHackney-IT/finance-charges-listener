using Amazon.DynamoDBv2.DataModel;
using FinanceChargesListener.Domain;
using Hackney.Core.DynamoDb.Converters;
using System;
using System.Collections.Generic;
using static FinanceChargesListener.Domain.Enums;

namespace FinanceChargesListener.Infrastructure.Entity
{
    [DynamoDBTable("Charges", LowerCamelCaseProperties = true)]
    public class ChargeDbEntity
    {
        [DynamoDBHashKey(AttributeName = "target_id")]
        public Guid TargetId { get; set; }

        [DynamoDBRangeKey(AttributeName = "id")]
        public Guid Id { get; set; }

        [DynamoDBProperty(AttributeName = "target_type", Converter = typeof(DynamoDbEnumConverter<TargetType>))]
        public TargetType TargetType { get; set; }

        [DynamoDBProperty(AttributeName = "charge_group", Converter = typeof(DynamoDbEnumConverter<ChargeGroup>))]
        public ChargeGroup ChargeGroup { get; set; }

        [DynamoDBProperty(AttributeName = "charge_sub_group", Converter = typeof(DynamoDbEnumConverter<ChargeSubGroup>))]
        public ChargeSubGroup? ChargeSubGroup { get; set; }

        [DynamoDBProperty(AttributeName = "charge_year")]
        public short ChargeYear { get; set; }

        [DynamoDBProperty(AttributeName = "detailed_charges", Converter = (typeof(DynamoDbObjectListConverter<DetailedCharges>)))]
        public IEnumerable<DetailedCharges> DetailedCharges { get; set; }

        [DynamoDBProperty(AttributeName = "created_by")]
        public string CreatedBy { get; set; }

        [DynamoDBProperty(AttributeName = "last_updated_by")]
        public string LastUpdatedBy { get; set; }

        [DynamoDBProperty(AttributeName = "created_at", Converter = typeof(DynamoDbDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [DynamoDBProperty(AttributeName = "last_updated_at", Converter = typeof(DynamoDbDateTimeConverter))]
        public DateTime LastUpdatedAt { get; set; }
    }
}
