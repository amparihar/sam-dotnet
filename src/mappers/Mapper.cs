using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;

using Lambda.Models;

namespace Lambda.Mappers
{
    public class Mapper
    {
        public static IEnumerable<ItemResponse> ToItemResponse(IEnumerable<Document> items)
        {
            return items.Select(ToItemResponse);
        }

        public static ItemResponse ToItemResponse(Document doc)
        {
            return new ItemResponse
            {
                Id = doc["id"],
                Key = doc["key"],
                Name = doc["name"]
            };
        }

    }
}