using Stencil.Native.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.ViewModels
{
    public class BaseViewModelLanguage : BaseClass
    {
        public BaseViewModelLanguage(string trackPrefix)
            : base(trackPrefix)
        {
        }

        public virtual IStencilApp StencilApp
        {
            get
            {
                return Container.StencilApp;
            }
        }

        #region Language


        public string Text_General_EmailWatermark
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.General_EmailWatermark, "email@domain.com");
            }
        }
        public string Text_General_PasswordWatermark
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.General_PasswordWatermark, "Password");
            }
        }
        public string Text_General_Edit
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.General_Edit, "Edit");
            }
        }

        public string Text_General_ErrorSearching
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.General_ErrorSearching, "Error Searching, please try again.");
            }
        }
        public string Text_General_NoResultsFor
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.General_NoResultsFor, "No results found for: {0}");
            }
        }
       
        public string Text_General_Cancel
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.General_Cancel, "Cancel");
            }
        }
        public string Text_General_Loading
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.General_Loading, "Loading..");
            }
        }
        public string Text_General_Sending
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.General_Sending, "Sending..");
            }
        }
        public string Text_General_Processing
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.General_Processing, "Processing..");
            }
        }
        public string Text_General_UnableToSubmit
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.General_UnableToSubmit, "Unable to process request. Please try again in a few moments.");
            }
        }
        public string Text_General_UnableToLoad
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.General_UnableToLoad, "Unable to load requested item. Please try again in a few moments.");
            }
        }
        public string Text_General_Leave
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.General_Leave, "Leave");
            }
        }
        public string Text_General_NoThanks
        {
            get
            {
                return StencilApp.GetLocalizedText(I18NToken.General_NoThanks, "No Thanks");
            }
        }


        #endregion

    }
}
