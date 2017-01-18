using System;
using System.Collections.Generic;
using System.Text;
using Stencil.Domain;

namespace Stencil.Primary.Business.Direct
{
    // WARNING: THIS FILE IS GENERATED
    public partial interface IAssetBusiness
    {
        Asset GetById(Guid asset_id);
        Asset Insert(Asset insertAsset);
        Asset Update(Asset updateAsset);
        
        void Delete(Guid asset_id);
        
        
    }
}

