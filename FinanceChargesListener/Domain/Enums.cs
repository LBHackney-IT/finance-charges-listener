using System.Text.Json.Serialization;

namespace FinanceChargesListener.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChargeGroup
    {
        Tenants,
        Leaseholders
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChargeSubGroup
    {
        Actual,
        Estimate
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChargeFrequency
    {
        Monthly,
        Weekly,
        Yearly
    }

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
        Property,
        NA
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ValuesType
    {
        Actual = 1,
        Estimate = 2
    }
}
