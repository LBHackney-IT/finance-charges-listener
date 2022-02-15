using FinanceChargesListener.Domain;
using System;
using System.Threading.Tasks;

namespace FinanceChargesListener.Infrastructure.Interfaces
{
    public interface IHousingData<T> where T : class
    {
        Task<T> DownloadAsync(Guid id, SearchBy searchBy = SearchBy.ById);
    }
}
