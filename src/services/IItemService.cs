
using System.Collections.Generic;
using System.Threading.Tasks;
using Lambda.Models;

namespace Item.Service
{
    public interface IItemService
    {
        Task<IEnumerable<ItemResponse>> GetItems(string id, string key);

        Task<SaveItemRequest> SaveItem(SaveItemRequest request);

        Task<IEnumerable<SaveItemRequest>> BatchWrite(IEnumerable<SaveItemRequest> request);

        Task DeleteTable();

    }
}