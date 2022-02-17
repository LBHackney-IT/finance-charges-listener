using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceChargesListener.UseCase.Utility
{
    public static class ChargeHelper
    {
        public static DetailedCharges GetChargeDetailModel(decimal chargeAmount,
         string chargeName, string chargeCode, ChargeGroup chargeGroup, ChargeType chargeType)
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
                            : ChargeFrequency.Monthly.ToString(),
                StartDate = Helper.GetFirstMondayForApril(DateTime.UtcNow.Year),
                EndDate = Helper.GetFirstMondayForApril(DateTime.UtcNow.AddYears(1).Year).AddDays(-1)
            };
        }

        public static ChargeType GetChargeType(string chargeName)
        {
            switch (chargeName)
            {
                case "Estate Cleaning":
                case "Estate Repairs":
                case "Estate Electricity":
                case "Roads, footpaths and drainage":
                case "CCTV Maintenance":
                case "Grounds Maintenance":
                    return ChargeType.Estate;
                case "Block Cleaning":
                case "Block Repairs":
                case "Block Electricity":
                case "Communal Door Entry Maintenance":
                case "Lift Maintenance":
                case "Communal TV aerial Maintenance":
                case "Heating/Hotwater Maintenance":
                case "Heating/Hotwater Standing Charge":
                case "Heating/Hotwater Energy":
                case "Block CCTV Maintenance":
                case "Concierge Service":
                    return ChargeType.Block;
                default:
                    return ChargeType.Property;
            }
        }
        public static Charge GetChargeModel(string assetType, ChargeGroup chargeGroup, string chargeSubGroup, string createdBy,
          short chargeYear, EstimateActualCharge estimateOrActualCharge)
        {
            var detailedChargesList = new List<DetailedCharges>();
            if (estimateOrActualCharge.BlockCCTVMaintenanceAndMonitoring >= 0)
            {
                var chargeType = GetChargeType(Constants.BlockCCTVMaintenanceAndMonitoring);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BlockCCTVMaintenanceAndMonitoring,
                    Constants.BlockCCTVMaintenanceAndMonitoring, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.BlockCleaning >= 0)
            {
                var chargeType = GetChargeType(Constants.BlockCleaning);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BlockCleaning,
                    Constants.BlockCleaning, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.BlockElectricity >= 0)
            {
                var chargeType = GetChargeType(Constants.BlockElectricity);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BlockElectricity,
                    Constants.BlockElectricity, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.BlockRepairs >= 0)
            {
                var chargeType = GetChargeType(Constants.BlockRepairs);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BlockRepairs,
                    Constants.BlockRepairs, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.BuildingInsurancePremium >= 0)
            {
                var chargeType = GetChargeType(Constants.BuildingInsurancePremium);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.BuildingInsurancePremium,
                    Constants.BuildingInsurancePremium, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.CommunalTVAerialMaintenance >= 0)
            {
                var chargeType = GetChargeType(Constants.CommunalTVAerialMaintenance);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.CommunalTVAerialMaintenance,
                    Constants.CommunalTVAerialMaintenance, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.ConciergeService >= 0)
            {
                var chargeType = GetChargeType(Constants.ConciergeService);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.ConciergeService,
                    Constants.ConciergeService, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.DoorEntry >= 0)
            {
                var chargeType = GetChargeType(Constants.DoorEntry);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.DoorEntry,
                    Constants.DoorEntry, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.EstateCCTVMaintenanceAndMonitoring >= 0)
            {
                var chargeType = GetChargeType(Constants.EstateCCTVMaintenanceAndMonitoring);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateCCTVMaintenanceAndMonitoring,
                    Constants.EstateCCTVMaintenanceAndMonitoring, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.EstateCleaning >= 0)
            {
                var chargeType = GetChargeType(Constants.EstateCleaning);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateCleaning,
                    Constants.EstateCleaning, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.EstateElectricity >= 0)
            {
                var chargeType = GetChargeType(Constants.EstateElectricity);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateElectricity,
                    Constants.EstateElectricity, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.EstateRepairs >= 0)
            {
                var chargeType = GetChargeType(Constants.EstateRepairs);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateRepairs,
                    Constants.EstateRepairs, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.EstateRoadsFootpathsAndDrainage >= 0)
            {
                var chargeType = GetChargeType(Constants.EstateRoadsFootpathsAndDrainage);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.EstateRoadsFootpathsAndDrainage,
                    Constants.EstateRoadsFootpathsAndDrainage, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.GroundRent >= 0)
            {
                var chargeType = GetChargeType(Constants.GroundRent);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.GroundRent,
                    Constants.GroundRent, Constants.GroundRentChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.GroundsMaintenance >= 0)
            {
                var chargeType = GetChargeType(Constants.GroundsMaintenance);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.GroundsMaintenance,
                    Constants.GroundsMaintenance, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.HeatingOrHotWaterEnergy >= 0)
            {
                var chargeType = GetChargeType(Constants.HeatingOrHotWaterEnergy);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.HeatingOrHotWaterEnergy,
                    Constants.HeatingOrHotWaterEnergy, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.HeatingOrHotWaterMaintenance >= 0)
            {
                var chargeType = GetChargeType(Constants.HeatingOrHotWaterMaintenance);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.HeatingOrHotWaterMaintenance,
                    Constants.HeatingOrHotWaterMaintenance, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.HeatingStandingCharge >= 0)
            {
                var chargeType = GetChargeType(Constants.HeatingStandingCharge);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.HeatingStandingCharge,
                    Constants.HeatingStandingCharge, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.LiftMaintenance >= 0)
            {
                var chargeType = GetChargeType(Constants.LiftMaintenance);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.LiftMaintenance,
                    Constants.LiftMaintenance, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.ManagementCharge >= 0)
            {
                var chargeType = GetChargeType(Constants.ManagementCharge);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.ManagementCharge,
                    Constants.ManagementCharge, Constants.EstimateChargeCode, chargeGroup, chargeType));
            }
            if (estimateOrActualCharge.ReserveFund >= 0)
            {
                var chargeType = GetChargeType(Constants.ReserveFund);
                detailedChargesList.Add(GetChargeDetailModel(estimateOrActualCharge.ReserveFund,
                    Constants.ReserveFund, Constants.ReserveFundChargeCode, chargeGroup, chargeType));
            }

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
