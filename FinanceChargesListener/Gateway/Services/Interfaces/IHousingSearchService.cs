using FinanceChargesListener.Domain;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Services.Interfaces
{
    public interface HousingSearchService
    {
        Task<AssetListResponse> GetAssets(string type, int pageSize, int pageNumber, string lastHitId = null);
    }
}
