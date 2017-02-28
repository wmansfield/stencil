using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native
{
    public enum I18NToken
    {
        ConnectionTimeOut, //"Connection timed out."
        Notification, //"Notification"
        VersionNotSupported, //"This version of the app is no longer supported."
        AppExpired, //"App Expired"
        LatestVersionAndroid, //"Please download the latest version from the Play Store."
        LatestVersionIOS, //"Please download the latest version from the App Store.

        Login_LoggingIn, //"Logging In.."
        Login_MissingUser, //"You must provide your email"
        Login_MissingPassword, //"You must provide a password"
        Login_SignIn, //"Sign In"

        Posts_Title, //"Posts"
        PostCreate_Title, //"New Post"
        PostCreate_Create, //"Create"

        Remarks_Title, //"Posts"
        RemarksCreate_Title, //"New Remark"
        RemarksCreate_Create, //"Create"

        General_NoResultsFor, //"No results found for: {0}"
        General_ErrorSearching, //"Error Searching, please try again."
        General_Edit, //"Edit"
        General_OK, //"OK"
        General_Updating, //"Updating.."
        General_Remove, //"Remove"
        General_Select, //"Select"
        General_NoThanks, //"No Thanks"
        General_Leave, //"Leave"
        General_UnableToLoad, //"Unable to load requested item. Please try again in a few moments."
        General_Loading, //"Loading.."
        General_Sending, //"Sending.."
        General_Cancel, //"Cancel"
        General_Update, //"Update"
        General_Deleting, //"Deleting.."
        General_Upload, //"Upload"
        General_Post, //"Post"
        General_EmailWatermark, //"email@domain.com"
        General_PasswordWatermark, //"Password"
        General_WriteCaption, //"Write a caption.."
        General_WritePost, //"Write a post.."
        General_Send, //"Send"
        General_MustProvideText, //"You must provide text"
        General_LoadingVideo, //"Loading Video.."
        General_LoadingPhoto, //"Loading Photo.."
        General_Processing, //"Processing.."
        General_UnableToSubmit, //"Unable to process request. Please try again in a few moments."
        General_Delete, //"Delete"


        ALERT_SAMPLE, //"{0} said: {1}"
    }
}
