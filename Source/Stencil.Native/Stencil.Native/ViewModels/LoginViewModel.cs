using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Stencil.Native.Core;
using Stencil.SDK;
using Stencil.SDK.Models.Requests;
using Stencil.SDK.Models.Responses;

namespace Stencil.Native.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(IViewModelView view)
            : base(view, "LoginViewModel")
        {

        }

        public string Text_SignIn
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.Login_SignIn, "Sign In").ToUpper();
            }
        }
        
        public string Text_MissingUserName
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.Login_MissingUser, "You must provide your email");
            }
        }
        public string Text_LoggingIn
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.Login_LoggingIn, "Logging In..");
            }
        }
        public string Text_MissingUserPassword
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.Login_MissingPassword, "You must provide a password");
            }
        }
        public string Validate(string userName, string password)
        {
            return base.ExecuteFunction("Validate", delegate ()
            {
                if (string.IsNullOrEmpty(userName))
                {
                    return this.Text_MissingUserName;
                }
                if (string.IsNullOrEmpty(password))
                {
                    return this.Text_MissingUserName;
                }
                return string.Empty;
            });
        }

        public Task<ActionResult> LoginAsync(string userName, string password)
        {
            return base.ExecuteFunctionAsync<ActionResult>("LoginAsync", async delegate ()
            {
                string validationMessage = this.Validate(userName, password);
                if(!string.IsNullOrEmpty(validationMessage))
                {
                    return new ActionResult()
                    {
                        success = false,
                        message = validationMessage
                    };
                }
                else
                {
                    try
                    {
                        ItemResult<AccountInfo> response = await this.StencilApp.LoginAsyncSafe(new AuthLoginInput()
                        {
                            password = password,
                            user = userName
                        });
                        return response;
                    }
                    catch(Exception ex)
                    {
                        return new ActionResult()
                        {
                            success = false,
                            message = ex.FirstNonAggregateException().Message
                        };
                    }
                }
            });
        }
    }
}
