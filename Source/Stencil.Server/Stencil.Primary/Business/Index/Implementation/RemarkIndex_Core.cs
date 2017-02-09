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
    public partial class RemarkIndex : IndexerBase<sdk.Remark>, IRemarkIndex
    {
        public RemarkIndex(IFoundation foundation)
            : base(foundation, "RemarkIndex", DocumentNames.Remark)
        {

        }
        protected override string GetModelId(sdk.Remark model)
        {
            return model.remark_id.ToString();
        }
        public ListResult<sdk.Remark> GetByPost(Guid post_id, int skip, int take, string order_by = "", bool descending = false)
        {
            return base.ExecuteFunction("GetByPost", delegate ()
            {
                QueryContainer query = Query<sdk.Remark>.Term(w => w.post_id, post_id);

                

                int takePlus = take;
                if(take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }
                
                List<SortFieldDescriptor<sdk.Remark>> sortFields = new List<SortFieldDescriptor<sdk.Remark>>();
                if(!string.IsNullOrEmpty(order_by))
                {
                    SortFieldDescriptor<sdk.Remark> item = new SortFieldDescriptor<sdk.Remark>()
                        .Field(order_by)
                        .Order(descending ? SortOrder.Descending : SortOrder.Ascending);
                        
                    sortFields.Add(item);
                }
                SortFieldDescriptor<sdk.Remark> defaultSort = new SortFieldDescriptor<sdk.Remark>()
                    .Field(r => r.stamp_utc)
                    .Descending();
                
                sortFields.Add(defaultSort);
                
                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse<sdk.Remark> searchResponse = client.Search<sdk.Remark>(s => s
                    .Query(q => query)
                    .Skip(skip)
                    .Take(takePlus)
                    .Sort(sr => sr.Multi(sortFields))
                    .Type(this.DocumentType));

                ListResult<sdk.Remark> result = searchResponse.Documents.ToSteppedListResult(skip, take, searchResponse.GetTotalHit());
                
                return result;
            });
        }
        public ListResult<sdk.Remark> GetByAccount(Guid account_id, int skip, int take, string order_by = "", bool descending = false)
        {
            return base.ExecuteFunction("GetByAccount", delegate ()
            {
                QueryContainer query = Query<sdk.Remark>.Term(w => w.account_id, account_id);

                

                int takePlus = take;
                if(take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }
                
                List<SortFieldDescriptor<sdk.Remark>> sortFields = new List<SortFieldDescriptor<sdk.Remark>>();
                if(!string.IsNullOrEmpty(order_by))
                {
                    SortFieldDescriptor<sdk.Remark> item = new SortFieldDescriptor<sdk.Remark>()
                        .Field(order_by)
                        .Order(descending ? SortOrder.Descending : SortOrder.Ascending);
                        
                    sortFields.Add(item);
                }
                SortFieldDescriptor<sdk.Remark> defaultSort = new SortFieldDescriptor<sdk.Remark>()
                    .Field(r => r.stamp_utc)
                    .Descending();
                
                sortFields.Add(defaultSort);
                
                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse<sdk.Remark> searchResponse = client.Search<sdk.Remark>(s => s
                    .Query(q => query)
                    .Skip(skip)
                    .Take(takePlus)
                    .Sort(sr => sr.Multi(sortFields))
                    .Type(this.DocumentType));

                ListResult<sdk.Remark> result = searchResponse.Documents.ToSteppedListResult(skip, take, searchResponse.GetTotalHit());
                
                return result;
            });
        }
        
        public int GetCountRemark(Guid post_id)
        {
            return base.ExecuteFunction("GetCountRemark", delegate ()
            {
                QueryContainer query = Query<Remark>.Term(w => w.post_id, post_id);
                
                query &= Query<Remark>.Exists(f => f.Field(x => x.remark_id));
               
                ElasticClient client = this.ClientFactory.CreateClient();
                ISearchResponse<sdk.Remark> response = client.Search<sdk.Remark>(s => s
                    .Query(q => query)
                    .Skip(0)
                    .Take(0)
                    .Type(this.DocumentType));

                 
                return (int)response.GetTotalHit();
            });
        }
        
        public ListResult<sdk.Remark> Find(int skip, int take, string keyword = "", string order_by = "", bool descending = false, Guid? post_id = null, Guid? account_id = null)
        {
            return base.ExecuteFunction("Find", delegate ()
            {
                int takePlus = take;
                if(take != int.MaxValue)
                {
                    takePlus++; // for stepping
                }
                
                QueryContainer query = Query<sdk.Remark>
                    .MultiMatch(m => m
                        .Query(keyword)
                        .Type(TextQueryType.PhrasePrefix)
                        .Fields(mf => mf
                                .Field(f => f.text)
                ));
                                
                if(post_id.HasValue)
                {
                    query &= Query<sdk.Remark>.Term(f => f.post_id, post_id.Value);
                }
                if(account_id.HasValue)
                {
                    query &= Query<sdk.Remark>.Term(f => f.account_id, account_id.Value);
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
                ISearchResponse<sdk.Remark> searchResponse = client.Search<sdk.Remark>(s => s
                    .Query(q => query)
                    .Skip(skip)
                    .Take(takePlus)
                    .Sort(r => r.Field(order_by, sortOrder))
                    .Type(this.DocumentType));
                
                ListResult<sdk.Remark> result = searchResponse.Documents.ToSteppedListResult(skip, take, searchResponse.GetTotalHit());
                
                return result;
            });
        }
        

    }
}
