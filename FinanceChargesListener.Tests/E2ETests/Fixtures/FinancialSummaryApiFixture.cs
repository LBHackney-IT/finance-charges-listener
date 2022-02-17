using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Tests.E2ETests.Fixtures
{
    public class FinancialSummaryApiFixture : BaseApiFixture<AddAssetSummaryRequest>
    {
        public FinancialSummaryApiFixture()
        {
            Environment.SetEnvironmentVariable("FINANCIAL_SUMMARY_API_URL", FixtureConstants.FinanceSummaryApiRoute);
            Route = FixtureConstants.HousingSearchApiRoute;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && !Disposed)
            {
                ResponseObject = null;
                base.Dispose(disposing);
            }
        }
        public void GivenTheAccountDoesNotExist(Guid id)
        {
            // Nothing to do here
        }
    }
}
