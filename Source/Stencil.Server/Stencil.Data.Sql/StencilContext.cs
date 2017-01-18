using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Data.Sql
{
    public partial class StencilContext
    {
        public StencilContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }


        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                string message = string.Empty;
                try
                {
                    // Retrieve the error messages as a list of strings.
                    var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    var fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    message = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                }
                catch
                {
                    //we made things worse, use the old one
                    throw ex;
                }
                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(message, ex.EntityValidationErrors);
            }
        }

    }
}
