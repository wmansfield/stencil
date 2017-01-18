using Stencil.Data.Sql;
using Stencil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Direct.Implementation
{
    public partial class GlobalSettingBusiness
    {
        public GlobalSetting GetByName(string name)
        {
            return base.ExecuteFunction("GetByName", delegate ()
            {
                using (var db = this.CreateSQLContext())
                {
                    dbGlobalSetting result = (from n in db.dbGlobalSettings
                                              where (n.name == name)
                                              select n).FirstOrDefault();
                    return result.ToDomainModel();
                }
            });
        }
        public List<GlobalSetting> FindWithPrefix(string prefix)
        {
            return base.ExecuteFunction("FindWithPrefix", delegate ()
            {
                using (var db = this.CreateSQLContext())
                {
                    var result = (from n in db.dbGlobalSettings
                                  where (n.name.StartsWith(prefix))
                                  select n);
                    return result.ToDomainModel();
                }
            });
        }
    }
}
