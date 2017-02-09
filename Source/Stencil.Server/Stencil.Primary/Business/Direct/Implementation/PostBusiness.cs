using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stencil.Domain;

namespace Stencil.Primary.Business.Direct.Implementation
{
    public partial class PostBusiness
    {
        partial void PreProcess(Post post, bool forInsert)
        {
            if (forInsert)
            {
                post.created_utc = DateTime.UtcNow;
            }
        }
    }
}
