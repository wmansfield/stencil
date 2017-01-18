using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Stencil.Common;
using Stencil.Common.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Stencil.Primary.Business.Index
{
    public partial class StencilElasticClientFactory : ChokeableClass, IStencilElasticClientFactory
    {
        public StencilElasticClientFactory(IFoundation foundation)
            : base(foundation)
        {
            this.SettingsResolver = this.IFoundation.Resolve<ISettingsResolver>();
        }

        private ConnectionSettings _connectionSettings;

        protected ISettingsResolver SettingsResolver { get; set; }
        protected virtual ConnectionSettings ConnectionSettings
        {
            get
            {
                if (_connectionSettings == null)
                {
                    bool isLocalHost = this.SettingsResolver.IsLocalHost();

                    _connectionSettings = new ConnectionSettings(new Uri(this.HostUrl))
                        .DefaultIndex(this.IndexName);

                    if (isLocalHost || this.QueryDebugEnabled)
                    {
                        _connectionSettings.DisableDirectStreaming();
                        _connectionSettings.OnRequestCompleted(details =>
                        {
                            Debug.WriteLine(" --- ElasticSearch REQEUST --- ");
                            if (details.RequestBodyInBytes != null)
                            {
                                Debug.WriteLine(Encoding.UTF8.GetString(details.RequestBodyInBytes));
                            }
                            Debug.WriteLine(" --- ElasticSearch RESPONSE --- ");
                            if (details.ResponseBodyInBytes != null)
                            {
                                Debug.WriteLine(Encoding.UTF8.GetString(details.ResponseBodyInBytes));
                            }
                        });
                    }
                }
                return _connectionSettings;
            }
        }
        public virtual string IndexName
        {
            get
            {
                return this.SettingsResolver.GetSetting(CommonAssumptions.APP_KEY_ES_INDEX);
            }
        }
        public virtual string HostUrl
        {
            get
            {
                return this.SettingsResolver.GetSetting(CommonAssumptions.APP_KEY_ES_URL);
            }
        }
        public virtual int ReplicaCount
        {
            get
            {
                return int.Parse(this.SettingsResolver.GetSetting(CommonAssumptions.APP_KEY_ES_REPLICA));
            }
        }
        public virtual int ShardCount
        {
            get
            {
                return int.Parse(this.SettingsResolver.GetSetting(CommonAssumptions.APP_KEY_ES_SHARDS));
            }
        }
        public virtual bool QueryDebugEnabled
        {
            get
            {
                bool result = false;
                bool.TryParse(this.SettingsResolver.GetSetting(CommonAssumptions.APP_KEY_DEBUG_QUERIES), out result);
                return result;
            }
        }

        public virtual bool HasEnsuredModelIndices { get; protected set; }

        [Obsolete("Should only be used in dev", false)]
        public virtual void ClearIndex()
        {
            base.ExecuteMethod("ClearIndex", delegate ()
            {
                ElasticClient client = CreateClient();
                client.DeleteIndex(this.IndexName);
                this.HasEnsuredModelIndices = false;
            });
        }

        public virtual ElasticClient CreateClient()
        {
            return base.ExecuteFunction("CreateClient", delegate ()
            {
                ElasticClient result = new ElasticClient(this.ConnectionSettings);

                this.EnsureModelIndices(result);
                return result;
            });
        }


        private static bool debug_reset = false;
        private static object debug_lock = new object();
        private static object ensure_lock = new object();

        protected void EnsureModelIndices(ElasticClient client)
        {
            base.ExecuteMethod("EnsureModelIndices", delegate ()
            {
                if (debug_reset)
                {
                    bool executeDebug = false;
                    lock (debug_lock)
                    {
                        if (debug_reset)
                        {
                            executeDebug = true;
                        }
                        debug_reset = false;
                    }
                    if (executeDebug)
                    {
                        //client.Map<Objective>(m => m
                        //            .MapFromAttributes()
                        //            .Type(DocumentTypes.OBJECTIVES)
                        //            .Properties(props => props
                        //                .String(s => s
                        //                    .Name(p => p.campaign_id)
                        //                    .Index(FieldIndexOption.NotAnalyzed))
                        //                .String(s => s
                        //                    .Name(p => p.objective_id)
                        //                    .Index(FieldIndexOption.NotAnalyzed)
                        //            ))
                        //         );
                    }
                }
                if (!this.HasEnsuredModelIndices)
                {
                    lock (ensure_lock)
                    {
                        if (!this.HasEnsuredModelIndices)
                        {
                            if (!client.IndexExists(this.IndexName).Exists)
                            {
                                CustomAnalyzer ignoreCaseAnalyzer = new CustomAnalyzer
                                {
                                    Tokenizer = "keyword",
                                    Filter = new[] { "lowercase" }
                                };
                                Analysis analysis = new Analysis();
                                analysis.Analyzers = new Analyzers();
                                analysis.Analyzers.Add("case_insensitive", ignoreCaseAnalyzer);
                                ICreateIndexResponse createResult = client.CreateIndex(this.IndexName, delegate (Nest.CreateIndexDescriptor descriptor)
                                {
                                    descriptor.Settings(ss => ss
                                        .Analysis(a => analysis)
                                        .NumberOfReplicas(this.ReplicaCount)
                                        .NumberOfShards(this.ShardCount)
                                        .Setting("merge.policy.merge_factor", "10")
                                        .Setting("search.slowlog.threshold.fetch.warn", "1s")
                                        .Setting("max_result_window", "2147483647")
                                    );
                                    this.MapIndexModels(descriptor);
                                    return descriptor;
                                });
                                if (!createResult.Acknowledged)
                                {
                                    throw new Exception("Error creating index, mapping is no longer valid");
                                }
                            }
                            HasEnsuredModelIndices = true;
                        }
                    }
                }
            });
        }

        partial void MapIndexModels(CreateIndexDescriptor indexer);
    }
}
