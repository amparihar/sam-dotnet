using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Cloud.AWS.DynamoDb;

using Lambda.Mappers;
using Lambda.Models;

namespace Item.Service
{
    public class ItemService : IItemService
    {
        private readonly string _tableName = Environment.GetEnvironmentVariable("ITEM_TABLE_NAME");
        private readonly Table _table;
        private bool _loadTableSuccess { get; set; }
        private readonly DynamoDBService _dbService;
        private readonly Mapper _map;
        public ItemService()
        {
            _dbService = new DynamoDBService();
            _map = new Mapper();
            _loadTableSuccess = Table.TryLoadTable(
                _dbService.DbClient,
                _tableName,
                DynamoDBEntryConversion.V2, out _table);
        }

        public async Task<IEnumerable<ItemResponse>> GetItems(string id, string key)
        {
            if (_loadTableSuccess)
            {
                var filter = new QueryFilter("id", QueryOperator.Equal, id);
                filter.AddCondition("key", QueryOperator.BeginsWith, key);

                var documentResponse = await _table.Query(filter).GetRemainingAsync();
                // TO DO: Use Extention Method 
                // Modify mapper functions as extension methods
                var queryResponse = _map.ToItemResponse(documentResponse);

                return queryResponse;
            }
            return null;
        }

        public async Task<SaveItemRequest> SaveItem(SaveItemRequest request)
        {
            if (_loadTableSuccess)
            {
                var newItem = _map.ToSaveItemDocumentModel(request);
                await _table.PutItemAsync(newItem);
                return request;
            }
            return null;
        }
    }
}