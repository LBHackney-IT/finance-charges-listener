using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Interfaces
{
    public interface EsGateway
    {
        public Task<IndexResponse> IndexAsset(QueryableAsset esAsset);
        public Task<UpdateResponse<QueryableAsset>> UpdateAsset(QueryableAsset esAsset);
    }
}
