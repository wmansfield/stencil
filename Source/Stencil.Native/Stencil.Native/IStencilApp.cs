using Stencil.Native.App.Config;
using Stencil.Native.Core;
using Stencil.SDK;
using Stencil.SDK.Models;
using Stencil.SDK.Models.Requests;
using Stencil.SDK.Models.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Stencil.Native.Caching;

namespace Stencil.Native
{
    public interface IStencilApp
    {
        AppConfig AppConfig { get; }
        AccountInfo CurrentAccount { get; }
        IViewPlatform ViewPlatform { get; }
        string CurrentCulture { get; }

        int Badge { get; }
        void Initialize();
        Task RefreshAccountAsync();
        void ApplyAppPreferences(Func<AppPreferences, bool> action);
        AppPreferences AppPreferences { get; }

        StencilSDK GetSDK(bool useCurrentUserInfo);


        Task<ItemResult<AccountInfo>> RegisterAsyncSafe(RegisterInput info, Action<bool> onProcessing = null);
        Task<ItemResult<AccountInfo>> LoginAsyncSafe(AuthLoginInput info, Action<bool> onProcessing = null);
        void LogOff(bool notifyViewPlatform, bool redirect);

        void UpdatePreferredCulture(string culture);

        void OnAppActivated();

        void PersistPushNotificationToken(string deviceToken);

        string GetLocalizedText(I18NToken token, string defaultText);


        void PostsInvalidateCache(string suffix = "");
        Task<bool> PostsGetAsync(RequestToken requestToken, bool? allowStale, bool force, int skip, int take, FetchedRequestDelegate<ListResult<Post>> onFetched, Action<bool> onFetching = null);
        Task<ItemResult<Post>> PostCreateAsync(Post post, Action<bool> onFetching = null);

        Task<bool> RemarksGetAsync(RequestToken requestToken, Guid post_id, int skip, int take, Action<RequestToken, ListResult<Remark>> onFetched, Action<bool> onFetching = null);
        Task<ItemResult<Remark>> RemarkCreateAsync(Remark remark, Action<bool> onFetching = null);
    }
}
