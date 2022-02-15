using Hackney.Shared.Asset.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Tests.E2ETests.Fixtures
{
    public class AssetInformationApiFixture : BaseApiFixture<Asset>
    {
        public AssetInformationApiFixture()
        {
            Environment.SetEnvironmentVariable("ASSET_INFORMATION_API_URL", FixtureConstants.AssetInformationApiRoute);
            Environment.SetEnvironmentVariable("ASSET_INFORMATION_API_TOKEN", FixtureConstants.AssetInformationApiToken);
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
