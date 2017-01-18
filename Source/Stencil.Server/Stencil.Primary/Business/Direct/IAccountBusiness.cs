using Stencil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Direct
{
    public partial interface IAccountBusiness
    {
        Account CreateInitialAccount(Account insertIfEmpty);
        Account GetForValidPassword(string email, string password);
        Account GetByApiKey(string api_key);
        void UpdateLastLogin(Guid account_id, DateTime last_login_utc, string platform);
        Account GetByEmail(string email);
        Account PasswordResetStart(Guid account_id);
        bool PasswordResetComplete(Guid account_id, string token, string password);
        void UpdatePushTokenApple(Guid account_id, string registrationID);
        void UpdatePushTokenGoogle(Guid account_id, string registrationID);
        void UpdatePushTokenMicrosoft(Guid account_id, string registrationID);

    }
}
