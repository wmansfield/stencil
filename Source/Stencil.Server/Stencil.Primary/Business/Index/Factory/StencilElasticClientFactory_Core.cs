using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.SDK.Models;
using sdk = Stencil.SDK.Models;

namespace Stencil.Primary.Business.Index
{
    public partial class StencilElasticClientFactory
    {
        partial void MapIndexModels(CreateIndexDescriptor indexer)
        {
            indexer.Mappings(mp => mp.Map<sdk.Account>(DocumentNames.Account, p => p
                .AutoMap()
                .Properties(props => props
                    .String(s => s
                        .Name(n => n.account_id)
                        .Index(FieldIndexOption.NotAnalyzed)
                    ).String(m => m
                        .Name(t => t.email)
                        .Fields(f => f
                                .String(s => s.Name(n => n.email)
                                .Index(FieldIndexOption.Analyzed))
                                .String(s => s
                                    .Name(n => n.email.Suffix("sort"))
                                    .Analyzer("case_insensitive"))
                                
                        )
                    ).String(m => m
                        .Name(t => t.first_name)
                        .Fields(f => f
                                .String(s => s.Name(n => n.first_name)
                                .Index(FieldIndexOption.Analyzed))
                                .String(s => s
                                    .Name(n => n.first_name.Suffix("sort"))
                                    .Analyzer("case_insensitive"))
                                
                        )
                    ).String(m => m
                        .Name(t => t.last_name)
                        .Fields(f => f
                                .String(s => s.Name(n => n.last_name)
                                .Index(FieldIndexOption.Analyzed))
                                .String(s => s
                                    .Name(n => n.last_name.Suffix("sort"))
                                    .Analyzer("case_insensitive"))
                                
                        )
                    ).String(m => m
                        .Name(t => t.last_login_utc)
                        .Fields(f => f
                                .String(s => s.Name(n => n.last_login_utc)
                                .Index(FieldIndexOption.Analyzed))
                                .String(s => s
                                    .Name(n => n.last_login_utc.Suffix("sort"))
                                    .Analyzer("case_insensitive"))
                                
                        )
                    ).String(m => m
                        .Name(t => t.last_login_platform)
                        .Fields(f => f
                                .String(s => s.Name(n => n.last_login_platform)
                                .Index(FieldIndexOption.Analyzed))
                                .String(s => s
                                    .Name(n => n.last_login_platform.Suffix("sort"))
                                    .Analyzer("case_insensitive"))
                                
                        )
                    )
                )
            ));
            
        }
    }
}
