using Stencil.Native.App.Config;
using Stencil.Native.Caching;
using Stencil.Native.Core;
using Stencil.SDK;
using Stencil.SDK.Models;
using Stencil.SDK.Models.Responses;
using Stencil.SDK.Models.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Stencil.SDK.Exceptions;
using System.Net;
using Newtonsoft.Json;

namespace Stencil.Native.App
{
    public class StencilApp : BaseClass, IStencilApp
    {
        #region Constructor

        public StencilApp(IViewPlatform platform, ICacheHost cacheHost, IDataCache dataCache, string apiBaseUrl)
            : base("StencilApp")
        {
            this.ApiBaseUrl = apiBaseUrl;
            this.CacheHost = cacheHost;
            this.DataCache = dataCache;
            this.ViewPlatform = platform;
            this.AppConfig = new AppConfig();
        }

        #endregion

        #region Constants

        private const string CACHE_FILENAME_APP_CONFIG = "app_config.cache";
        private const string CACHE_FILENAME_APP_PREFS = "app_prefs.cache";
        private const string CACHE_FILENAME_INSTALL_ID = "install.guid";
        private const string CACHE_FILENAME_USER_CACHE = "user_mem.cache";

        #endregion

        #region Properties

        protected System.Text.RegularExpressions.Regex _regexHashTags = new System.Text.RegularExpressions.Regex(@"(?:(?<=\s)|^)#(\w*)");
        protected System.Text.RegularExpressions.Regex _regexUserNames = new System.Text.RegularExpressions.Regex(@"(?:(?<=\s)|^)@(\w*)");
        protected static string _notificationInputCacheKey = "notification-input.cache";
        protected static string _notificationResultCacheKey = "notification-result.cache";
        private static object _inputNotificationSyncRoot = new object();


        private string _pendingPushToken;
        private string _culture;

        public string CurrentCulture
        {
            get
            {
                return _culture;
            }
        }
        public string ApiBaseUrl { get; protected set; }
        public ICacheHost CacheHost { get; protected set; }
        public IDataCache DataCache { get; protected set; }
        public IViewPlatform ViewPlatform { get; protected set; }
        public AppConfig AppConfig { get; protected set; }
        public AccountInfo CurrentAccount { get; protected set; }
        public AppPreferences AppPreferences { get; protected set; }
        public int Badge { get; protected set; }

        protected string DeviceToken
        {
            get
            {
                string result = this.CacheHost.PersistentDataGet<string>(false, "deviceToken");
                if (string.IsNullOrEmpty(result))
                {
                    result = Guid.NewGuid().ToString("N");
                    this.CacheHost.PersistentDataSet<string>(false, "deviceToken", result);
                    string test = this.CacheHost.PersistentDataGet<string>(false, "deviceToken");
                }
                return result;
            }
        }

        protected virtual StencilSDK StencilClientAnonymous { get; set; }
        protected virtual StencilSDK StencilClientAuthenticated { get; set; }

        protected DateTime? _lastAppConfigCheck;
        protected DateTime? _lastAppVersionCheck;
        protected DateTime? _lastAccountVerifyCheck;

        #endregion

        #region Public App Methods

        public void Initialize()
        {
            base.ExecuteMethod("Initialize", delegate ()
            {
                this.AppPreferences = this.CacheHost.PersistentDataGet<AppPreferences>(false, CACHE_FILENAME_APP_PREFS);
                if (this.AppPreferences == null)
                {
                    this.AppPreferences = new AppPreferences();
                }
                this.AppConfig = this.CacheHost.PersistentDataGet<AppConfig>(false, CACHE_FILENAME_APP_CONFIG);
                if (this.AppConfig == null)
                {
                    this.AppConfig = new AppConfig();
                }
                this.ApplyAppConfig();

                this.CurrentAccount = this.CacheHost.CachedUserGet<AccountInfo>();

                if ((this.CurrentAccount != null) && (this.CurrentAccount.account_id != Guid.Empty))
                {
                    this.ViewPlatform.OnLoggedOn();

                    this.VerifyCachedAccountAsync();
                }

                this.EnsureAppConfigDelayed();
            });
        }

        /// <summary>
        /// Does Not Throw Exceptions.
        /// </summary>
        public virtual Task<ItemResult<AccountInfo>> RegisterAsyncSafe(RegisterInput info, Action<bool> onProcessing = null)
        {
            return base.ExecuteFunctionAsync("RegisterAsyncSafe", async delegate ()
            {
                ItemResult<AccountInfo> result = new ItemResult<AccountInfo>() { success = false };
                bool isOutdated = false;
                try
                {
                    ItemResult<AccountInfo> registerResult = null;
                    this.LogOff(false, false);

                    registerResult = await this.PostItemUnSafeAsync(onProcessing, async delegate ()
                    {
                        StencilSDK client = this.GetSDK(false);
                        return await client.Auth.RegisterAsync(info);
                    });

                    if (registerResult.IsSuccess())
                    {
                        result = registerResult;
                    }
                    else
                    {
                        result.success = false;
                        result.meta = registerResult.meta;
                        result.message = registerResult.GetMessage();
                    }
                }
                catch (Exception ex)
                {
                    ex = ex.FirstNonAggregateException();

                    base.LogError(ex, "RegisterAsyncSafe");

                    result.success = false;
                    isOutdated = this.IsOutdated(ex);
                    if (isOutdated)
                    {
                        this.Outdated(string.Empty);
                    }
                    result.message = ex.Message;
                    EndpointException endpointException = ex as EndpointException;
                    if (endpointException != null)
                    {
                        result.meta = ((int)endpointException.StatusCode).ToString();
                    }
                }

                if (result.IsSuccess())
                {
                    this.CacheHost.CachedUserSet(result.item);
                    this.CurrentAccount = result.item;
                    this.UserCacheClear();
                    AppPreferences prefs = new AppPreferences();
                    this.CacheHost.PersistentDataSet(false, CACHE_FILENAME_APP_PREFS, prefs);
                    this.AppPreferences = prefs;

                    this.ViewPlatform.OnLoggedOn();
                }
                else
                {
                    this.LogOff(!isOutdated, false);
                    if (string.IsNullOrEmpty(result.message))
                    {
                        result.message = "Could not create account.";
                    }
                }

                return result;
            });
        }

        /// <summary>
        /// Does Not Throw Exceptions.
        /// </summary>
        public virtual Task<ItemResult<AccountInfo>> LoginAsyncSafe(AuthLoginInput info, Action<bool> onProcessing = null)
        {
            return base.ExecuteFunctionAsync("LoginAsyncSafe", async delegate ()
            {
                ItemResult<AccountInfo> result = new ItemResult<AccountInfo>() { success = false };
                bool isOutdated = false;
                try
                {
                    ItemResult<AccountInfo> loginResult = null;
                    this.LogOff(false, false);

                    loginResult = await this.PostItemUnSafeAsync(onProcessing, async delegate ()
                    {
                        StencilSDK client = this.GetSDK(false);
                        return await client.Auth.LoginAsync(info);
                    });

                    if (loginResult.IsSuccess())
                    {
                        result = loginResult;
                    }
                    else
                    {
                        result.success = false;
                        result.meta = loginResult.meta;
                        result.message = loginResult.GetMessage();
                    }
                }
                catch (Exception ex)
                {
                    ex = ex.FirstNonAggregateException();

                    base.LogError(ex, "LoginAsyncSafe");

                    result.success = false;
                    isOutdated = this.IsOutdated(ex);
                    if (isOutdated)
                    {
                        this.Outdated(string.Empty);
                    }
                    result.message = ex.Message;
                    EndpointException endpointException = ex as EndpointException;
                    if (endpointException != null)
                    {
                        result.meta = ((int)endpointException.StatusCode).ToString();
                    }
                }

                if (result.IsSuccess())
                {
                    this.CacheHost.CachedUserSet(result.item);
                    this.CurrentAccount = result.item;
                    this.UserCacheClear();
                    AppPreferences prefs = new AppPreferences();
                    this.CacheHost.PersistentDataSet(false, CACHE_FILENAME_APP_PREFS, prefs);
                    this.AppPreferences = prefs;

                    this.ViewPlatform.OnLoggedOn();
                }
                else
                {
                    this.LogOff(!isOutdated, false);
                    if (string.IsNullOrEmpty(result.message))
                    {
                        result.message = "Could not log in to your account.";
                    }
                }

                return result;
            });
        }

        public virtual void LogOff(bool notifyViewPlatform, bool redirect)
        {
            base.ExecuteMethod("LogOff", delegate ()
            {
                this.StencilClientAnonymous = null;
                this.StencilClientAuthenticated = null;
                this.ViewPlatform.UnRegisterForPushNotifications();
                this.CacheHost.CachedUserClear();
                this.CacheHost.CachedDataClear();
                this.CacheHost.PersistentDataSet(false, CACHE_FILENAME_APP_PREFS, new AppPreferences());
                this.AppPreferences = new AppPreferences();
                this.UserCacheClear();
                this.DataCache.Clear();
                this.DataCache.InvalidateTimedCache();
                this.CurrentAccount = null;
                if (notifyViewPlatform)
                {
                    try
                    {
                        this.ViewPlatform.OnLoggedOff();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex, "OnLoggedOff");
                    }
                }
                if (redirect)
                {
                    this.ViewPlatform.NavigateToFirstScreen();
                }
            });
        }

        public void OnAppActivated()
        {
            base.ExecuteMethod("OnAppActivated", delegate ()
            {
                this.EnsureAppConfigDelayed();
                this.EnsureAppVersionDelayed();
                this.EnsureAccountStillValidDelayed();
            });
        }

        public void PersistPushNotificationToken(string deviceToken)
        {
            this.ExecuteMethodOrSkipAsync("PersistPushNotificationToken", async delegate ()
            {
                this.LogTrace("--------- PersistPushNotificationToken -----------------");
                if (string.IsNullOrEmpty(deviceToken))
                {
                    deviceToken = _pendingPushToken;
                }
                _pendingPushToken = deviceToken;// in case we log off and back on
                if (!string.IsNullOrEmpty(deviceToken))
                {
                    if (this.CurrentAccount != null)
                    {
                        StencilSDK client = this.GetSDK(true);
                        try
                        {
                            ActionResult result = await client.Accounts.RegisterPushTokenAsync(new PushTokenInput()
                            {
                                platform = this.ViewPlatform.ShortName,
                                token = deviceToken
                            });
                            if (result.IsSuccess())
                            {
                                this.LogTrace("saved push notification");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.LogError(ex.FirstNonAggregateException(), "PersistPushNotificationToken");
                        }
                    }
                    else
                    {
                        _pendingPushToken = deviceToken;
                    }
                }
            });
        }

        public virtual void Outdated(string reason)
        {
            base.ExecuteMethod("Outdated", delegate ()
            {
                this.ViewPlatform.OnOutDated(reason);
                this.LogOff(true, true);
            });
        }

        public void ApplyAppPreferences()
        {
            this.ApplyAppPreferences(this.AppPreferences);
        }
        public void ApplyAppPreferences(AppPreferences preferences)
        {
            base.ExecuteMethod("ApplyAppPreferences", delegate ()
            {
                this.AppPreferences = preferences;
                this.CacheHost.PersistentDataSet(false, CACHE_FILENAME_APP_PREFS, preferences);
            });
        }
        public void ApplyAppPreferences(Func<AppPreferences, bool> action)
        {
            base.ExecuteMethod("ApplyAppPreferences", delegate ()
            {
                bool changed = action(this.AppPreferences);
                if (changed)
                {
                    this.ApplyAppPreferences(this.AppPreferences);
                }
            });
        }
        public void InvalidateCacheForPrefix(string prefix)
        {
            base.ExecuteMethod("InvalidateCacheForPrefix", delegate ()
            {
                this.DataCache.InvalidateTimedPrefix(prefix);
            });
        }

        public virtual Task RefreshAccountAsync()
        {
            return base.ExecuteMethodAsync("RefreshAccountAsync", async delegate ()
            {
                // do not wrap this in the normal fetch or get data methods, need direct response
                if ((this.CurrentAccount != null) && (this.CurrentAccount.account_id != Guid.Empty))
                {
                    var sdk = this.GetSDK(true);
                    try
                    {
                        ItemResult<AccountInfo> result = await sdk.Accounts.GetSelfAsync();
                        if (result.IsSuccess() && result.item.account_id == this.CurrentAccount.account_id)
                        {
                            this.CurrentAccount = result.item;
                            this.CacheHost.CachedUserSet(result.item);
                            this.ViewPlatform.OnAccountRefreshed();
                        }
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, "RefreshAccountAsync");
                    }
                }
            });
        }

        public T UserCacheGet<T>(string token)
            where T : class
        {
            string result = null;
            Dictionary<string, string> userCache = this.CacheHost.PersistentDataGet<Dictionary<string, string>>(false, CACHE_FILENAME_USER_CACHE);
            if (userCache == null)
            {
                userCache = new Dictionary<string, string>();
            }

            if (userCache.TryGetValue(token, out result) && !string.IsNullOrEmpty(result))
            {
                return JsonConvert.DeserializeObject<T>(result);
            }
            else
            {
                return default(T);
            }
        }
        public void UserCacheSet<T>(string token, T value)
            where T : class
        {
            Dictionary<string, string> userCache = this.CacheHost.PersistentDataGet<Dictionary<string, string>>(false, CACHE_FILENAME_USER_CACHE);
            if (userCache == null)
            {
                userCache = new Dictionary<string, string>();
            }
            userCache[token] = JsonConvert.SerializeObject(value);
            this.CacheHost.PersistentDataSet(false, CACHE_FILENAME_USER_CACHE, userCache);
        }
        public void UserCacheClear()
        {
            this.CacheHost.PersistentDataSet(false, CACHE_FILENAME_USER_CACHE, string.Empty);

            this.CacheHost.PersistentDataSet(false, _notificationResultCacheKey, string.Empty);
            this.CacheHost.PersistentDataSet(false, _notificationInputCacheKey, string.Empty);
        }

        public virtual void UpdatePreferredCulture(string culture)
        {
            base.ExecuteMethod("UpdatePreferredCulture", delegate ()
            {
                _culture = culture;
                this.AppPreferences.PreferredCulture = culture;
                this.ApplyAppPreferences();
                if (!string.IsNullOrEmpty(_culture))
                {
                    if (this.StencilClientAnonymous != null)
                    {
                        var found = this.StencilClientAnonymous.CustomHeaders.FindIndex(x => x.Key == "accept-language");
                        if (found >= 0)
                        {
                            this.StencilClientAnonymous.CustomHeaders.RemoveAt(found);
                        }
                        this.StencilClientAnonymous.CustomHeaders.Add(new KeyValuePair<string, string>("accept-language", _culture));
                    }
                    if (this.StencilClientAuthenticated != null)
                    {
                        var found = this.StencilClientAuthenticated.CustomHeaders.FindIndex(x => x.Key == "accept-language");
                        if (found >= 0)
                        {
                            this.StencilClientAuthenticated.CustomHeaders.RemoveAt(found);
                        }
                        this.StencilClientAuthenticated.CustomHeaders.Add(new KeyValuePair<string, string>("accept-language", _culture));
                    }
                }
            });
        }
        public virtual string GetLocalizedText(I18NToken token, string defaultText)
        {
            return GetLocalizedText(token.ToString(), defaultText);
        }
        public virtual string GetLocalizedText(string token, string defaultText)
        {
            return base.ExecuteFunction("GetLocalizedText", delegate ()
            {
                if (string.IsNullOrEmpty(_culture))
                {
                    _culture = this.AppPreferences.PreferredCulture;
                    if (string.IsNullOrEmpty(_culture))
                    {
                        _culture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                        if (string.IsNullOrEmpty(_culture))
                        {
                            _culture = "en";
                        }
                    }
                }

                //TODO:SHOULD:Localization: Get localized text from token based on _culture

                return defaultText;
            });
        }
        #endregion

        #region Data Access Methods

        /// <summary>
        /// Throws Errors.
        /// Executes a direct response method WITHOUT support for cached data or timeouts
        /// </summary>
        protected async virtual Task<T> PostItemUnSafeAsync<T>(Action<bool> onProcessing, Func<Task<T>> postMethod)
            where T : ActionResult, new()
        {
            try
            {
                if (onProcessing != null)
                {
                    onProcessing(true);
                }
                return await postMethod();
            }
            catch (Exception ex)
            {
                this.ProcessExecuteException(ex, "PostItemUnSafeAsync");
                ex = ex.FirstNonAggregateException();
                if (ex is EndpointException)
                {
                    return new T()
                    {
                        success = false,
                        message = (ex as EndpointException).Message
                    };
                }
                throw ex; // we dont catch here
            }
            finally
            {
                if (onProcessing != null)
                {
                    onProcessing(false);
                }
            }
        }

        /// <summary>
        /// Fetches scalar data WITHOUT support for cached data or timeouts
        /// </summary>
        protected async virtual Task<bool> GetItemAsync<T>(Action<ItemResult<T>> onFetched, Action<bool> onFetching, Func<Task<ItemResult<T>>> fetchMethod)
        {
            try
            {

                if (onFetching != null)
                {
                    onFetching(true);
                }
                ItemResult<T> result = await fetchMethod();
                if (onFetched != null)
                {
                    onFetched(result);
                }
                return true;
            }
            catch (Exception ex)
            {
                this.ProcessExecuteException(ex, "GetItemAsync");
                return false;
            }
            finally
            {
                if (onFetching != null)
                {
                    onFetching(false);
                }
            }
        }
        /// <summary>
        /// Fetches list data WITHOUT support for cached data or timeouts
        /// </summary>
        protected async virtual Task<bool> GetListAsync<T>(Action<ListResult<T>> onFetched, Action<bool> onFetching, Func<Task<ListResult<T>>> fetchMethod)
        {
            try
            {

                if (onFetching != null)
                {
                    onFetching(true);
                }
                ListResult<T> result = await fetchMethod();
                if (onFetched != null)
                {
                    onFetched(result);
                }
                return true;
            }
            catch (Exception ex)
            {
                this.ProcessExecuteException(ex, "GetListAsync");
                return false;
            }
            finally
            {
                if (onFetching != null)
                {
                    onFetching(false);
                }
            }
        }
        /// <summary>
        /// Fetches list data WITHOUT support for cached data or timeouts
        /// </summary>
        protected async virtual Task<bool> GetListAsync<T>(RequestToken requestToken, Action<RequestToken, ListResult<T>> onFetched, Action<bool> onFetching, Func<Task<ListResult<T>>> fetchMethod)
        {
            try
            {

                if (onFetching != null)
                {
                    onFetching(true);
                }
                ListResult<T> result = await fetchMethod();
                if (onFetched != null)
                {
                    onFetched(requestToken, result);
                }
                return true;
            }
            catch (Exception ex)
            {
                this.ProcessExecuteException(ex, "GetListAsync");
                return false;
            }
            finally
            {
                if (onFetching != null)
                {
                    onFetching(false);
                }
            }
        }

        /// <summary>
        /// Fetches scalar data with support for cached data and timeouts
        /// </summary>
        protected virtual Task<bool> FetchItemAsync<T>(bool allowStale, string cachePrefixKey, string cacheSpecificKey, int staleLimit, FetchedDelegate<ItemResult<T>> onFetched, Action<bool> onFetching, Func<Task<ItemResult<T>>> fetchMethod)
        {
            try
            {
                return this.DataCache.WithTimedRefreshForPrefixAsync<ItemResult<T>>(allowStale, cachePrefixKey, cacheSpecificKey, staleLimit, onFetched, onFetching, async delegate ()
                {
                    return await fetchMethod();
                });
            }
            catch (Exception ex)
            {
                EndpointException endpointException = ex.FirstExceptionOfType<EndpointException>();
                if (endpointException != null)
                {
                    switch (endpointException.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.Forbidden:
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.RequestTimeout:
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.NotImplemented:
                        case HttpStatusCode.ServiceUnavailable:
                            this.ViewPlatform.ShowToast("Error connecting to server. Please check connection.");
                            break;
                        default:
                            break;
                    }
                }
                this.ProcessExecuteException(ex, "FetchItemAsync");
                throw;
            }
        }

        /// <summary>
        /// Fetches list data with support for cached data and timeouts
        /// </summary>
        protected virtual Task<bool> FetchListAsync<T>(RequestToken requestToken, bool allowStale, string cachePrefixKey, string cacheSpecificKey, int staleLimit, FetchedRequestDelegate<ListResult<T>> onFetched, Action<bool> onFetching, Func<Task<ListResult<T>>> fetchMethod)
        {
            try
            {
                return this.DataCache.WithTimedRefreshForPrefixAsync<ListResult<T>>(requestToken, allowStale, cachePrefixKey, cacheSpecificKey, staleLimit, onFetched, onFetching, async delegate ()
                {
                    return await fetchMethod();
                });
            }
            catch (Exception ex)
            {
                EndpointException endpointException = ex.FirstExceptionOfType<EndpointException>();
                if (endpointException != null)
                {
                    switch (endpointException.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.Forbidden:
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.RequestTimeout:
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.NotImplemented:
                        case HttpStatusCode.ServiceUnavailable:
                            this.ViewPlatform.ShowToast("Error connecting to server. Please check connection.");
                            break;
                        default:
                            break;
                    }
                }
                this.ProcessExecuteException(ex, "FetchListAsync");
                throw;
            }
        }
        /// <summary>
        /// Fetches list data with support for cached data and timeouts
        /// </summary>
        protected virtual Task<bool> FetchListAsync<TData, TMeta>(RequestToken requestToken, bool allowStale, string cachePrefixKey, string cacheSpecificKey, int staleLimit, FetchedRequestDelegate<ListResult<TData, TMeta>> onFetched, Action<bool> onFetching, Func<Task<ListResult<TData, TMeta>>> fetchMethod)
            where TMeta : new()
        {
            try
            {
                return this.DataCache.WithTimedRefreshForPrefixAsync<ListResult<TData, TMeta>>(requestToken, allowStale, cachePrefixKey, cacheSpecificKey, staleLimit, onFetched, onFetching, async delegate ()
                {
                    return await fetchMethod();
                });
            }
            catch (Exception ex)
            {
                EndpointException endpointException = ex.FirstExceptionOfType<EndpointException>();
                if (endpointException != null)
                {
                    switch (endpointException.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.Forbidden:
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.RequestTimeout:
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.NotImplemented:
                        case HttpStatusCode.ServiceUnavailable:
                            this.ViewPlatform.ShowToast("Error connecting to server. Please check connection.");
                            break;
                        default:
                            break;
                    }
                }
                this.ProcessExecuteException(ex, "FetchListAsync");
                throw;
            }
        }


        protected virtual void ProcessExecuteException(Exception ex, string methodName)
        {
            base.ExecuteMethod("ProcessEndpointException", delegate ()
            {
                this.LogError(ex, methodName); // standard logging

                if (IsUnauthorized(ex) && methodName != "VerifyCachedAccountAsync") //name should never match.. but...
                {
                    this.VerifyCachedAccountAsync(); // double check our credentials are still valid
                }
            });
        }

        #endregion

        #region Protected App Methods

        protected virtual void ApplyAppConfig()
        {
            base.ExecuteMethod("ApplyAppConfig", delegate ()
            {
                // process any forced changes
            });
        }
        protected virtual Task VerifyCachedAccountAsync()
        {
            return base.ExecuteMethodOrSkipAsync("VerifyCachedAccountAsync", async delegate ()
            {
                // do not wrap this in the normal fetch or get data methods, need direct response
                if ((this.CurrentAccount != null) && (this.CurrentAccount.account_id != Guid.Empty))
                {
                    var sdk = this.GetSDK(true);
                    try
                    {
                        ItemResult<AccountInfo> result = await sdk.Accounts.GetSelfAsync();
                        if (result.IsSuccess() && result.item.account_id == this.CurrentAccount.account_id)
                        {
                            this.CurrentAccount = result.item;
                            this.CacheHost.CachedUserSet(result.item);
                            this.ViewPlatform.OnAccountRefreshed();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsForbidden(ex) || IsUnauthorized(ex))
                        {
                            this.LogOff(true, true);
                            this.ViewPlatform.DisplayNotification("Session Expired", "Your security session is no longer valid. You have been logged out and must re-authenticate to continue.");
                        }
                    }
                }
            });
        }

        public virtual StencilSDK GetSDK(bool useCurrentUserInfo)
        {
            StencilSDK result = null;

            if (useCurrentUserInfo)
            {
                result = this.StencilClientAuthenticated;
            }
            else
            {
                result = this.StencilClientAnonymous;
            }


            if (result == null)
            {
                result = new StencilSDK(this.ApiBaseUrl);

                if (!string.IsNullOrEmpty(_culture))
                {
                    result.CustomHeaders.Add(new KeyValuePair<string, string>("accept-language", _culture));
                }

                result.CustomHeaders.Add(new KeyValuePair<string, string>("X-DevicePlatform", this.ViewPlatform.ShortName));
                result.CustomHeaders.Add(new KeyValuePair<string, string>("X-DeviceVersion", this.ViewPlatform.VersionNumber));
                result.CustomHeaders.Add(new KeyValuePair<string, string>("X-DeviceToken", this.DeviceToken));

                if (useCurrentUserInfo)
                {
                    if (this.CurrentAccount != null)
                    {
                        result.ApplicationKey = this.CurrentAccount.api_key;
                        if (result.ApplicationSecret != this.CurrentAccount.api_secret)
                        {
                            result.ApplicationSecret = this.CurrentAccount.api_secret;
                        }
                        this.StencilClientAuthenticated = result;
                    }
                    else
                    {
                        result.ApplicationKey = string.Empty;
                        result.ApplicationSecret = string.Empty;
                        this.StencilClientAuthenticated = null;
                    }
                }
                else
                {
                    result.ApplicationKey = string.Empty;
                    result.ApplicationSecret = string.Empty;
                    this.StencilClientAnonymous = result;
                }
            }


            return result;
        }

        protected virtual void EnsureAppConfigDelayed()
        {
            Task.Run(async delegate
            {
                try
                {
                    await this.ExecuteMethodOrSkipAsync("EnsureAppConfigDelayed", async delegate ()
                    {
                        if (this.AppConfig.AppConfigIntervalHours > 0)
                        {
                            if (!_lastAppConfigCheck.HasValue || (DateTime.UtcNow - _lastAppConfigCheck.Value).TotalHours > AppConfig.AppConfigIntervalHours)
                            {
                                if (!_lastAppConfigCheck.HasValue)
                                {
                                    _lastAppConfigCheck = DateTime.UtcNow;
                                    await Task.Delay(TimeSpan.FromSeconds(30)); // wait 30 seconds, first load, don't suck up init load bandwidth
                                }
                                else
                                {
                                    _lastAppConfigCheck = DateTime.UtcNow;
                                }


                                var sdk = GetSDK(true);
                                ItemResult<AppConfig> appConfig = await sdk.Server.GetAppConfigAsync(this.ViewPlatform.ShortName);
                                if (appConfig.IsSuccess())
                                {
                                    this.AppConfig = appConfig.item;
                                    this.CacheHost.PersistentDataSet(false, CACHE_FILENAME_APP_CONFIG, this.AppConfig);
                                    this.ApplyAppConfig();
                                }
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    base.LogError(ex, "EnsureAppConfigDelayed");
                }
            });
        }
        protected virtual void EnsureAppVersionDelayed()
        {
            Task.Run(async delegate
            {
                try
                {
                    await this.ExecuteMethodOrSkipAsync("EnsureAppVersionDelayed", async delegate ()
                    {
                        if (this.AppConfig.AppVersionCheckIntervalHours > 0)
                        {
                            if (!_lastAppVersionCheck.HasValue || (DateTime.UtcNow - _lastAppVersionCheck.Value).TotalHours > AppConfig.AppVersionCheckIntervalHours)
                            {

                                if (!_lastAppVersionCheck.HasValue)
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(35)); // wait 30 seconds, first load, don't suck up init load bandwidth
                                }

                                var sdk = GetSDK(false);
                                ItemResult<UpdateRequiredInfo> updateRequired = await sdk.Server.GetIsUpdateRequiredAsync(this.ViewPlatform.ShortName, this.ViewPlatform.VersionNumber);
                                if (updateRequired.IsSuccess())
                                {
                                    if (updateRequired.item.required)
                                    {
                                        this.Outdated(updateRequired.item.message); // don't save, will check at every activate!
                                    }
                                    else
                                    {
                                        _lastAppVersionCheck = DateTime.UtcNow;
                                    }
                                }

                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    base.LogError(ex, "EnsureAppVersionDelayed");
                }
            });
        }
        protected virtual void EnsureAccountStillValidDelayed()
        {
            Task.Run(async delegate
            {
                try
                {
                    await this.ExecuteMethodOrSkipAsync("EnsureAccountStillValidDelayed", async delegate ()
                    {
                        if (this.AppConfig.AccountVerifyIntervalHours > 0)
                        {
                            if (!_lastAccountVerifyCheck.HasValue || (DateTime.UtcNow - _lastAccountVerifyCheck.Value).TotalHours > AppConfig.AccountVerifyIntervalHours)
                            {
                                await this.VerifyCachedAccountAsync();
                                _lastAccountVerifyCheck = DateTime.UtcNow;
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    base.LogError(ex, "EnsureAccountStillValidDelayed");
                }
            });
        }


        protected virtual bool IsOutdated(Exception ex)
        {
            AggregateException aggregate = ex as AggregateException;
            if (aggregate != null)
            {
                foreach (var item in aggregate.InnerExceptions)
                {
                    if (IsOutdated(item))
                    {
                        return true;
                    }
                }
            }
            EndpointException endpointException = ex as EndpointException;
            if (endpointException != null)
            {
                if (endpointException.StatusCode == System.Net.HttpStatusCode.ExpectationFailed)
                {
                    return true;
                }
            }
            return false;
        }
        protected virtual bool IsForbidden(Exception ex)
        {
            AggregateException aggregate = ex as AggregateException;
            if (aggregate != null)
            {
                foreach (var item in aggregate.InnerExceptions)
                {
                    if (IsForbidden(item))
                    {
                        return true;
                    }
                }
            }
            EndpointException endpointException = ex as EndpointException;
            if (endpointException != null)
            {
                if (endpointException.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return true;
                }
            }
            return false;
        }
        protected virtual bool IsUnauthorized(Exception ex)
        {
            AggregateException aggregate = ex as AggregateException;
            if (aggregate != null)
            {
                foreach (var item in aggregate.InnerExceptions)
                {
                    if (IsUnauthorized(item))
                    {
                        return true;
                    }
                }
            }
            EndpointException endpointException = ex as EndpointException;
            if (endpointException != null)
            {
                if (endpointException.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

    }
}
