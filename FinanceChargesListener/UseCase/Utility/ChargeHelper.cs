using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceChargesListener.UseCase.Utility
{
    public static class ChargeHelper
    {
        public static DetailedCharges GetChargeDetailModel(decimal chargeAmount,
         string chargeName, string chargeCode, ChargeGroup chargeGroup, ChargeType chargeType, short chargeYear)
        {
            return new DetailedCharges
            {
                Type = Constants.ServiceChargeType,
                SubType = chargeName,
                ChargeCode = chargeCode,
                Amount = chargeAmount,
                ChargeType = chargeType,
                Frequency = chargeGroup == ChargeGroup.Tenants
                            ? ChargeFrequency.Weekly.ToString()
                            : ChargeFrequency.Yearly.ToString(),
                StartDate = Helper.GetFirstMondayForApril(chargeYear),
                EndDate = Helper.GetFirstMondayForApril(chargeYear + 1).AddDays(-1)
            };
        }

        public static ChargeType GetChargeType(string chargeName)
        {
            switch (chargeName)
            {
                case "Estate Cleaning":
                case "Estate Repairs":
                case "Estate Electricity":
                case "Estate Roads Footpaths and Drainage":
                case "Estate CCTV Maintenance and Monitoring":
                case "Grounds Maintenance":
                    return ChargeType.Estate;
                case "Block Cleaning":
                case "Block Repairs":
                case "Block Electricity":
                case "Lift Maintenance":
                case "Communal TV Aerial Maintenance":
                case "Heating/Hotwater Maintenance":
                case "Heating/Hotwater Standing Charge":
                case "Heating/Hotwater Energy":
                case "Block CCTV Maintenance and Monitoring":
                case "Door Entry":
                case "Concierge Service":
                    return ChargeType.Block;
                default:
                    return ChargeType.Property;
            }
        }

        public static Charge GetChargeModel(string assetType, ChargeGroup chargeGroup, string chargeSubGroup,
            string createdBy, short chargeYear, EstimateActualCharge estimateOrActualCharge)
        {
            var detailedChargesList = new List<DetailedCharges>();
            var chargeType = GetChargeType(Constants.BlockCCTVMaintenanceAndMonitoring);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BlockCCTVMaintenanceAndMonitoring,
                Constants.BlockCCTVMaintenanceAndMonitoring, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.BlockCleaning);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BlockCleaning,
                Constants.BlockCleaning, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.BlockElectricity);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BlockElectricity,
                Constants.BlockElectricity, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.BlockRepairs);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BlockRepairs,
                Constants.BlockRepairs, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.BuildingInsurancePremium);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BuildingInsurancePremium,
                Constants.BuildingInsurancePremium, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.CommunalTVAerialMaintenance);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.CommunalTVAerialMaintenance,
                Constants.CommunalTVAerialMaintenance, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.ConciergeService);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.ConciergeService,
                Constants.ConciergeService, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.DoorEntry);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.DoorEntry,
                Constants.DoorEntry, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.EstateCCTVMaintenanceAndMonitoring);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateCCTVMaintenanceAndMonitoring,
                Constants.EstateCCTVMaintenanceAndMonitoring, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.EstateCleaning);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateCleaning,
                Constants.EstateCleaning, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.EstateElectricity);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateElectricity,
                Constants.EstateElectricity, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.EstateRepairs);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateRepairs,
                Constants.EstateRepairs, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.EstateRoadsFootpathsAndDrainage);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateRoadsFootpathsAndDrainage,
                Constants.EstateRoadsFootpathsAndDrainage, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.GroundRent);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.GroundRent,
                Constants.GroundRent, Constants.GroundRentChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.GroundsMaintenance);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.GroundsMaintenance,
                Constants.GroundsMaintenance, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.HeatingOrHotWaterEnergy);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.HeatingOrHotWaterEnergy,
                Constants.HeatingOrHotWaterEnergy, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.HeatingOrHotWaterMaintenance);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.HeatingOrHotWaterMaintenance,
                Constants.HeatingOrHotWaterMaintenance, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.HeatingStandingCharge);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.HeatingStandingCharge,
                Constants.HeatingStandingCharge, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.LiftMaintenance);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.LiftMaintenance,
                Constants.LiftMaintenance, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.ManagementCharge);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.ManagementCharge,
                Constants.ManagementCharge, Constants.EstimateChargeCode, chargeGroup, chargeType, chargeYear));


            chargeType = GetChargeType(Constants.ReserveFund);
            detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.ReserveFund,
                Constants.ReserveFund, Constants.ReserveFundChargeCode, chargeGroup, chargeType, chargeYear));


            var newCharge = new Charge
            {
                Id = Guid.NewGuid(),
                TargetId = estimateOrActualCharge.AssetId,
                ChargeGroup = chargeGroup,
                ChargeSubGroup = Enum.Parse<ChargeSubGroup>(chargeSubGroup),
                ChargeYear = chargeYear,
                TargetType = (TargetType) Enum.Parse(typeof(TargetType), assetType),
                DetailedCharges = detailedChargesList.AsEnumerable(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
            return newCharge;

        }
    }
}
