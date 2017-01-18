using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
