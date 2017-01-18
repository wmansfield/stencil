using Codeable.Foundation.Common;
using Codeable.Foundation.Common.System;
using Codeable.Foundation.Core;
using Stencil.Primary.Health;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Index.Implementation
{
    public abstract class IndexerBase<TModel> : IndexerBaseHealth, IIndexer<TModel>
         where TModel : class
    {
        public IndexerBase(IFoundation iFoundation, string trackPrefix, string documentType)
            : base(iFoundation, iFoundation.Resolve<IHandleExceptionProvider>(Assumptions.SWALLOWED_EXCEPTION_HANDLER), trackPrefix)
        {
            DocumentType = documentType;
            this.API = new StencilAPI(iFoundation);
        }


        #region Protected Properties

        public virtual StencilAPI API { get; set; }

        public virtual string DocumentType { get; protected set; }

        public virtual IStencilElasticClientFactory ClientFactory
        {
            get
            {
                return this.IFoundation.Resolve<IStencilElasticClientFactory>();
            }
        }

        #endregion

        #region Abstract Methods
        protected abstract string GetModelId(TModel model);

        #endregion

        #region Public Methods

        public virtual IndexResult CreateDocument(TModel model)
        {
            return this.UpdateDocument(model);
        }
        public virtual IndexResult DeleteDocument(TModel model)
        {
            return base.ExecuteFunctionWrite("DeleteDocument", delegate ()
            {
                ElasticClient client = ClientFactory.CreateClient();
                IDeleteResponse response = client.Delete(new DeleteRequest(this.ClientFactory.IndexName, this.DocumentType, GetModelId(model)));
                if (!response.IsValid)
                {
                    return new IndexResult()
                    {
                        success = false,
                        error = "invalid"
                    };
                }
                else
                {
                    return new IndexResult()
                    {
                        success = true,
                        version = response.Version
                    };
                }
            });
        }
        public virtual IndexResult UpdateDocumentPartial(string id, object partialAnonymousObject)
        {
            return base.ExecuteFunction("UpdateDocumentPartial", delegate ()
            {
                try
                {
                    UpdateRequest<TModel, object> request = new UpdateRequest<TModel, object>(this.ClientFactory.IndexName, this.DocumentType, id);
                    request.Doc = partialAnonymousObject;
                    request.RetryOnConflict = 2;

                    ElasticClient client = this.ClientFactory.CreateClient();
                    IUpdateResponse<TModel> result = client.Update(request);

                    if (!result.IsValid)
                    {
                        HealthReporter.Current.UpdateMetric(HealthTrackType.Each, string.Format(HealthReporter.INDEXER_INSTANT_FAIL_SOFT_FORMAT, typeof(TModel).FriendlyName()), 0, 1);
                        return new IndexResult()
                        {
                            success = false,
                            error = "invalid"
                        };
                    }
                    else
                    {
                        return new IndexResult()
                        {
                            success = true,
                            version = result.Version.ToString(),
                            created = false,
                            attempts = 1
                        };
                    }
                }
                catch (Exception ex)
                {
                    this.IFoundation.LogError(ex, "UpdateDocumentPartial");
                    HealthReporter.Current.UpdateMetric(HealthTrackType.Each, string.Format(HealthReporter.INDEXER_INSTANT_FAIL_TIMEOUT_FORMAT, typeof(TModel).FriendlyName()), 0, 1);
                    return new IndexResult()
                    {
                        success = false,
                        error = ex.Message
                    };
                }

            });
        }


        public virtual IndexResult UpdateDocument(TModel model)
        {
            return base.ExecuteFunctionWrite("UpdateDocument", delegate ()
            {
                try
                {
                    Exception exception = null;
                    string error = "invalid";
                    // try twice
                    for (int i = 0; i < 2; i++)
                    {
                        try
                        {
                            exception = null;
                            ElasticClient client = this.ClientFactory.CreateClient();
                            IndexRequest<TModel> request = new IndexRequest<TModel>(this.ClientFactory.IndexName, this.DocumentType, this.GetModelId(model));
                            request.Document = model;
                            IIndexResponse result = client.Index(request);
                            if (!result.IsValid)
                            {
                                HealthReporter.Current.UpdateMetric(HealthTrackType.Each, string.Format(HealthReporter.INDEXER_INSTANT_FAIL_SOFT_FORMAT, typeof(TModel).FriendlyName()), 0, 1);
                                if (result.ServerError != null && result.ServerError.Error != null && !string.IsNullOrEmpty(result.ServerError.Error.Reason))
                                {
                                    error = result.ServerError.Error.Reason;
                                }
                                // allow it to try again
                            }
                            else
                            {
                                return new IndexResult()
                                {
                                    success = true,
                                    version = result.Version.ToString(),
                                    created = result.Created,
                                    attempts = i + 1
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                    return new IndexResult()
                    {
                        success = false,
                        error = error
                    };
                }
                catch (Exception ex)
                {
                    this.IFoundation.LogError(ex, "UpdateDocument");
                    HealthReporter.Current.UpdateMetric(HealthTrackType.Each, string.Format(HealthReporter.INDEXER_INSTANT_FAIL_TIMEOUT_FORMAT, typeof(TModel).FriendlyName()), 0, 1);
                    return new IndexResult()
                    {
                        success = false,
                        error = ex.Message
                    };
                }
            });
        }

        public virtual TModel GetById(Guid id)
        {
            return base.ExecuteFunction("GetById", delegate ()
            {
                ElasticClient client = ClientFactory.CreateClient();
                IGetResponse<TModel> result = client.Get<TModel>(id.ToString(), ClientFactory.IndexName, this.DocumentType);

                return result.Source;
            });
        }

        public virtual TCustomModel GetById<TCustomModel>(Guid id)
            where TCustomModel : class
        {
            return base.ExecuteFunction("GetById", delegate ()
            {
                ElasticClient client = ClientFactory.CreateClient();
                var result = client.Get<TCustomModel>(id.ToString(), ClientFactory.IndexName, this.DocumentType);

                return result.Source;
            });
        }

        #endregion

    }
}
