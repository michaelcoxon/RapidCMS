using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Helpers;
using static RapidCMS.Core.Models.Data.Query;

namespace RapidCMS.Core.Models.ApiBridge.Request
{
    public class QueryModel
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string? SearchTerm { get; set; }
        public int? ActiveTab { get; set; }

        // TODO: protect this variable more
        public string? VariantTypeName { get; set; }

        public IEnumerable<OrderModel>? OrderBys { get; set; }

        public IQuery GetQuery<TEntity>()
        {
            var query = Create(Take, 1 + Skip / Math.Max(1, Take), SearchTerm, ActiveTab);

            if (OrderBys != null)
            {
                // is throwing here the best option?
                query.SetOrderBys(OrderBys
                    .Select(x => new OrderByModel(
                        x.OrderByType, 
                        PropertyMetadataHelper.GetPropertyMetadata(typeof(TEntity), x.PropertyName)
                            ?? throw new InvalidOperationException("Invalid properties given"))));
            }

            return query;
        }
    }
}
