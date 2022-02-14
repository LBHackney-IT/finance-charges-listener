using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinanceChargesListener.Domain
{
    public class Enums
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum TargetType
        {
            Block,
            Concierge,
            Dwelling,
            LettableNonDwelling,
            MediumRiseBlock,
            NA,
            TravellerSite,
            AdministrativeBuilding,
            BoilerHouse,
            BoosterPump,
            CleanersFacilities,
            CombinedHeatAndPowerUnit,
            CommunityHall,
            Estate,
            HighRiseBlock,
            Lift,
            LowRiseBlock,
            NBD,
            OutBuilding,
            TerracedBlock,
            WalkUpBlock
        }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ChargeMaintenanceStatus
        {
            Pending,
            Applied
        }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ChargeType
        {
            Estate,
            Block,
            Property
        }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ChargeGroup
        {
            Tenants,
            Leaseholders
        }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ChargeFrequency
        {
            Monthly,
            Weekly
        }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum ChargeSubGroup
        {
            Estimate,
            Actual
        }
    }
}
