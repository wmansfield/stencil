using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Index
{
    public static class _IndexExtensions
    {
        public static IPromise<Fields> Multi<T>(this FieldsDescriptor<T> descriptor, params System.Linq.Expressions.Expression<Func<T, object>>[] fields)
           where T : class
        {
            IPromise<Fields> promise = descriptor as IPromise<Fields>;
            foreach (var expression in fields)
            {
                promise = descriptor.Field(expression) as IPromise<Fields>;
            }
            return promise;
        }
        /// <summary>
        /// NEST Backwards compatibility for dynamic sort fields
        /// </summary>
        public static IPromise<IList<ISort>> Multi<T>(this SortDescriptor<T> search, IEnumerable<SortFieldDescriptor<T>> fields)
            where T : class
        {
            IPromise<IList<ISort>> promise = search as IPromise<IList<ISort>>;
            foreach (SortFieldDescriptor<T> item in fields)
            {
                promise.Value.Add(item);
            }
            return search;
        }

        /// <summary>
        /// NEST Backwards compatibility 
        /// </summary>
        public static MappingsDescriptor AddMapping<T>(this MappingsDescriptor mapping, TypeName typeName, Func<TypeMappingDescriptor<T>, ITypeMapping> map)
            where T : class
        {
            var item = mapping.Map(typeName, map);
            return mapping;
        }

        /// <summary>
        /// NEST Backwards compatibility for Get
        /// </summary>
        public static IGetResponse<T> Get<T>(this ElasticClient client, string id, string indexName, string documentType)
            where T : class
        {
            return client.Get<T>(new GetRequest(indexName, documentType, id));
        }
        public static long GetTotalHit<T>(this ISearchResponse<T> response)
             where T : class
        {
            if (response != null && response.HitsMetaData != null)
            {
                return response.HitsMetaData.Total;
            }
            return 0;
        }
    }
}
