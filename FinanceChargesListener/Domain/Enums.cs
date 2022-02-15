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
    public enum TargetType
    {
        Tenure,
        Person,
        Organisation,
        Asset,
        Block,
        Estate,
        Dwelling,
        Concierge,
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
        HighRiseBlock,
        Lift,
        LowRiseBlock,
        NBD,
        OutBuilding,
        TerracedBlock,
        WalkUpBlock
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChargeType
    {
        Estate,
        Block,
        Property,
        NA
    }

    public enum SearchBy
    {
        ById, ByTargetId
    }
}
