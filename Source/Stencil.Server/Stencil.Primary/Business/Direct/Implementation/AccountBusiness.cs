using Codeable.Foundation.Common;
using Stencil.Common;
using Stencil.Common.Data;
using Stencil.Data.Sql;
using Stencil.Domain;
using Stencil.Primary.Emaling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Direct.Implementation
{
    public partial class AccountBusiness
    {
        public Account CreateInitialAccount(Account insertIfEmpty)
        {
            return base.ExecuteFunction("CreateInitialAccount", delegate ()
            {
                dbAccount found = null;
                using (var db = base.CreateSQLContext())
                {
                    found = (from a in db.dbAccounts
                             select a).FirstOrDefault();
                }
                if (found == null)
                {
                    return Insert(insertIfEmpty);
                }
                return null;
            });
        }

        public Account GetForValidPassword(string email, string password)
        {
            return base.ExecuteFunction("GetForValidPassword", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        email = email.Trim();
                        dbAccount found = (from a in db.dbAccounts
                                           where a.email == email
                                           select a).FirstOrDefault();

                        if (found != null)
                        {
                            string computedPassword = GeneratePasswordHash(found.password_salt, password);
                            if (computedPassword == found.password)
                            {
                                return found.ToDomainModel();
                            }
                        }
                    }
                }
                return null;
            });
        }
        public Account GetByApiKey(string api_key)
        {
            return base.ExecuteFunction("GetByApiKey", delegate ()
            {
                using (var db = this.CreateSQLContext())
                {
                    dbAccount result = (from n in db.dbAccounts
                                        where (n.api_key == api_key)
                                        select n).FirstOrDefault();
                    return result.ToDomainModel();
                }
            });
        }
        public Account GetByEmail(string email)
        {
            return base.ExecuteFunction("GetByEmail", delegate ()
            {
                using (var db = this.CreateSQLContext())
                {
                    dbAccount result = (from n in db.dbAccounts
                                        where (n.email == email)
                                        select n).FirstOrDefault();
                    return result.ToDomainModel();
                }
            });
        }
        public void UpdateLastLogin(Guid account_id, DateTime last_login_utc, string platform)
        {
            base.ExecuteMethod("UpdateLastLogin", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    if (!string.IsNullOrEmpty(platform) && platform.Length > 250)
                    {
                        platform = platform.Substring(0, 250);
                    }
                    dbAccount found = (from n in db.dbAccounts
                                       where n.account_id == account_id
                                       select n).FirstOrDefault();

                    if (found != null)
                    {
                        found.last_login_utc = last_login_utc;
                        found.last_login_platform = platform;
                        db.SaveChanges();

                        this.API.Index.Accounts.UpdateLastLogin(account_id, found.ToDomainModel().last_login_utc, platform);
                    }
                }
            });
        }

        public Account PasswordResetStart(Guid account_id)
        {
            return base.ExecuteFunction("PasswordResetStart", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    dbAccount found = (from a in db.dbAccounts
                                       where a.account_id == account_id
                                       select a).FirstOrDefault();

                    if (found != null && !found.deleted_utc.HasValue)
                    {
                        found.password_reset_token = ShortGuid.NewGuid().ToString();
                        found.password_reset_utc = DateTime.UtcNow;
                        found.InvalidateSync(this.DefaultAgent, "passwordreset");
                        db.SaveChanges();

                        this.API.Integration.Email.SendPasswordResetInitiated(found.email, string.Format("{0} {1}", found.first_name, found.last_name), found.password_reset_token);
                    }
                    return found.ToDomainModel();
                }
            });
        }
        public bool PasswordResetComplete(Guid account_id, string token, string password)
        {
            return base.ExecuteFunction("PasswordResetComplete", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    dbAccount found = (from a in db.dbAccounts
                                       where a.account_id == account_id
                                       && a.password_reset_token == token
                                       && !string.IsNullOrEmpty(token)
                                       select a).FirstOrDefault();

                    if (found != null && !found.deleted_utc.HasValue)
                    {
                        if (found.password_reset_utc.HasValue)
                        {
                            TimeSpan difference = DateTime.UtcNow - found.password_reset_utc.Value;
                            if (difference.TotalHours > 24)
                            {
                                return false;
                            }
                        }
                        found.password_salt = Guid.NewGuid().ToString("N");
                        found.password = GeneratePasswordHash(found.password_salt, password); ;
                        found.password_reset_token = string.Empty;
                        found.password_reset_utc = null;
                        db.SaveChanges();

                        this.API.Integration.Email.SendRequestPasswordCompleted(found.email, string.Format("{0} {1}", found.first_name, found.last_name));
                        return true;

                    }
                    return false;
                }
            });
        }

        public void UpdatePushTokenApple(Guid account_id, string registrationID)
        {
            base.ExecuteMethod("UpdatePushTokenApple", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    dbAccount found = (from a in db.dbAccounts
                                       where a.account_id == account_id
                                       select a).FirstOrDefault();

                    if (found != null && !found.deleted_utc.HasValue)
                    {
                        found.push_ios = registrationID;
                        db.SaveChanges();
                    }
                }
            });
        }
        public void UpdatePushTokenGoogle(Guid account_id, string registrationID)
        {
            base.ExecuteMethod("UpdatePushTokenGoogle", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    dbAccount found = (from a in db.dbAccounts
                                       where a.account_id == account_id
                                       select a).FirstOrDefault();

                    if (found != null && !found.deleted_utc.HasValue)
                    {
                        found.push_google = registrationID;
                        db.SaveChanges();
                    }
                }
            });
        }
        public void UpdatePushTokenMicrosoft(Guid account_id, string registrationID)
        {
            base.ExecuteMethod("UpdatePushTokenMicrosoft", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    dbAccount found = (from a in db.dbAccounts
                                       where a.account_id == account_id
                                       select a).FirstOrDefault();

                    if (found != null && !found.deleted_utc.HasValue)
                    {
                        found.push_microsoft = registrationID;
                        db.SaveChanges();
                    }
                }
            });
        }

        protected virtual string GeneratePasswordHash(string salt, string password)
        {
            return base.ExecuteFunction("GeneratePasswordHash", delegate ()
            {
                return new SHA256Managed().HashAsString(salt + password);
            });
        }

        partial void PreProcess(Account account, bool forInsert)
        {
            base.ExecuteMethod("PreProcess", delegate ()
            {
                if (forInsert)
                {
                    if (string.IsNullOrEmpty(account.password_salt))
                    {
                        account.password_salt = Guid.NewGuid().ToString("N");
                    }

                    account.password = GeneratePasswordHash(account.password_salt, account.password); ;
                }

                // ensure keys, controller should, but this is critical
                if (string.IsNullOrEmpty(account.api_key))
                {
                    account.api_key = Guid.NewGuid().ToString("N").ToLower();
                }
                if (string.IsNullOrEmpty(account.api_secret))
                {
                    account.api_secret = Guid.NewGuid().ToString("N").ToLower();
                }
            });
        }
    }
}
