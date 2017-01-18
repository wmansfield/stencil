using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Index
{
    public interface IIndexer<TModel>
        where TModel : class
    {
        IndexResult CreateDocument(TModel model);
        IndexResult DeleteDocument(TModel model);
        IndexResult UpdateDocument(TModel model);
    }
}
