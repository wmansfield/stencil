using Codeable.Foundation.Common;
using Stencil.SDK;
using sdk = Stencil.SDK.Models;
using Stencil.SDK.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Index.Implementation
{
    public partial class AccountIndex : IndexerBase<sdk.Account>, IAccountIndex
    {
        public AccountIndex(IFoundation foundation)
            : base(foundation, "AccountIndex", DocumentNames.Account)
        {

        }
        protected override string GetModelId(sdk.Account model)
        {
            return model.account_id.ToString();
        }
        
        public ListResult<sdk.Account> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false)
        {
            return base.ExecuteFunction("Find", delegate ()
            {
                int takePlus = take;
                if(take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }
                
                QueryContainer query = Query<sdk.Account>
                    .MultiMatch(m => m
                        .Query(keyword)
                        .Type(TextQueryType.PhrasePrefix)
                        .Fields(mf => mf
                                .Field(f => f.email)
                ));
                                
                
                
                SortOrder sortOrder = SortOrder.Ascending;
                if (descending)
                {
                    sortOrder = SortOrder.Descending;
                }
                if (string.IsNullOrEmpty(order_by))
                {
                    order_by = "";
                }

                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse<sdk.Account> searchResponse = client.Search<sdk.Account>(s => s
                    .Query(q => query)
                    .Skip(skip)
                    .Take(takePlus)
                    .Sort(r => r.Field(order_by, sortOrder))
                    .Type(this.DocumentType));
                
                ListResult<sdk.Account> result = searchResponse.Documents.ToSteppedListResult(skip, take, searchResponse.GetTotalHit());
                
                return result;
            });
        }
        

    }
}
