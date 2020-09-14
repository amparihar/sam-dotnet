using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;

using Lambda.Models;

namespace Lambda.Mappers
{
    public class Mapper
    {
        public IEnumerable<ItemResponse> ToItemResponse(IEnumerable<Document> items)
        {
            return items.Select(ToItemResponse);
        }

        public ItemResponse ToItemResponse(Document doc)
        {
            return new ItemResponse
            {
                Id = doc["id"],
                Key = doc["key"],
                Name = doc["name"]
            };
        }

        public Document ToSaveItemDocumentModel(SaveItemRequest request)
        {
            return new Document
            {
                ["id"] = request.Id,
                ["key"] = request.Key,
                ["name"] = request.Name
            };
        }

    }
}