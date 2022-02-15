using FinanceChargesListener.Domain;
using System;

namespace FinanceChargesListener.Infrastructure.Interfaces
{
    public interface IGenerateUrl<T> where T : class
    {
        Uri Execute(Guid id, SearchBy searchBy = SearchBy.ById);
    }
}
