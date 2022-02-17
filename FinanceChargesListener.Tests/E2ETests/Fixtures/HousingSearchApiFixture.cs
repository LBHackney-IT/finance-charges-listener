using FinanceChargesListener.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Tests.E2ETests.Fixtures
{
    public class HousingSearchApiFixture : BaseApiFixture<AssetListResponse>
    {
        public HousingSearchApiFixture()
        {
            Environment.SetEnvironmentVariable("HOUSING_SEARCH_API_URL", FixtureConstants.HousingSearchApiRoute);
            Environment.SetEnvironmentVariable("HOUSING_SEARCH_API_TOKEN", FixtureConstants.HousingSearchApiToken);
            Route = FixtureConstants.HousingSearchApiRoute;
            Token = FixtureConstants.HousingSearchApiToken;
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
