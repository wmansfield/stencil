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
    public partial class PostIndex : IndexerBase<sdk.Post>, IPostIndex
    {
        public PostIndex(IFoundation foundation)
            : base(foundation, "PostIndex", DocumentNames.Post)
        {

        }
        protected override string GetModelId(sdk.Post model)
        {
            return model.post_id.ToString();
        }
        
        public virtual sdk.Post GetById(Guid id, Guid? for_account_id)
        {
            return base.ExecuteFunction("GetById", delegate ()
            {
                ElasticClient client = ClientFactory.CreateClient();
                IGetResponse<sdk.Post> response = client.Get<sdk.Post>(id.ToString(), ClientFactory.IndexName, this.DocumentType);

                sdk.Post result = response.Source;

                this.PostProcessForUser(new List<sdk.Post>() { result }, for_account_id);

                return result;
            });
        }
        public ListResult<sdk.Post> GetByAccount(Guid account_id, int skip, int take, string order_by = "", bool descending = false, Guid? for_account_id = null)
        {
            return base.ExecuteFunction("GetByAccount", delegate ()
            {
                QueryContainer query = Query<sdk.Post>.Term(w => w.account_id, account_id);

                

                int takePlus = take;
                if(take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }
                
                List<SortFieldDescriptor<sdk.Post>> sortFields = new List<SortFieldDescriptor<sdk.Post>>();
                if(!string.IsNullOrEmpty(order_by))
                {
                    SortFieldDescriptor<sdk.Post> item = new SortFieldDescriptor<sdk.Post>()
                        .Field(order_by)
                        .Order(descending ? SortOrder.Descending : SortOrder.Ascending);
                        
                    sortFields.Add(item);
                }
                SortFieldDescriptor<sdk.Post> defaultSort = new SortFieldDescriptor<sdk.Post>()
                    .Field(r => r.stamp_utc)
                    .Descending();
                
                sortFields.Add(defaultSort);
                
                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse<sdk.Post> searchResponse = client.Search<sdk.Post>(s => s
                    .Query(q => query)
                    .Skip(skip)
                    .Take(takePlus)
                    .Sort(sr => sr.Multi(sortFields))
                    .Type(this.DocumentType));

                ListResult<sdk.Post> result = searchResponse.Documents.ToSteppedListResult(skip, take, searchResponse.GetTotalHit());
                
                this.PostProcessForUser(result.items, for_account_id);
                
                return result;
            });
        }
        
        public ListResult<sdk.Post> Find(Guid? for_account_id, int skip, int take, string keyword = "", string order_by = "", bool descending = false, Guid? account_id = null)
        {
            return base.ExecuteFunction("Find", delegate ()
            {
                int takePlus = take;
                if(take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }
                
                QueryContainer query = Query<sdk.Post>
                    .MultiMatch(m => m
                        .Query(keyword)
                        .Type(TextQueryType.PhrasePrefix)
                        .Fields(mf => mf
                                .Field(f => f.body)
                ));
                                
                if(account_id.HasValue)
                {
                    query &= Query<sdk.Post>.Term(f => f.account_id, account_id.Value);
                }
                
                
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
                ISearchResponse<sdk.Post> searchResponse = client.Search<sdk.Post>(s => s
                    .Query(q => query)
                    .Skip(skip)
                    .Take(takePlus)
                    .Sort(r => r.Field(order_by, sortOrder))
                    .Type(this.DocumentType));
                
                ListResult<sdk.Post> result = searchResponse.Documents.ToSteppedListResult(skip, take, searchResponse.GetTotalHit());
                
                this.PostProcessForUser(result.items, for_account_id);
                
                return result;
            });
        }
        
        partial void PostProcessForUser(List<Post> items, Guid? account_id);
        

    }
}
