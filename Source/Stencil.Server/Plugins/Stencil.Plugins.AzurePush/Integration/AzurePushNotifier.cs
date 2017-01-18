using Codeable.Foundation.Common;
using Codeable.Foundation.Common.Aspect;
using Codeable.Foundation.Core.Caching;
using Codeable.Foundation.Core.Unity;
using Stencil.Common;
using Stencil.Common.Integration;
using Stencil.Domain;
using Stencil.Primary;
using Microsoft.ServiceBus.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Stencil.Plugins.AzurePush.Integration
{
    public class AzurePushNotifier : ChokeableClass, IPushNotifications
    {
        #region Constructors

        public AzurePushNotifier(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.API = iFoundation.Resolve<StencilAPI>();
            this.Cache = new AspectCache("AzurePushNotifier", iFoundation, new ExpireStaticLifetimeManager("AzurePushNotifier.Life15", System.TimeSpan.FromMinutes(15), false));
            try
            {
                // known to have bad config in debug
                this.HubClient = NotificationHubClient.CreateClientFromConnectionString(this.AzurePush_Connection, this.AzurePush_HubName);
            }
            catch (Exception ex)
            {
                iFoundation.LogError(ex, "AzurePushNotifier");
            }
        }

        #endregion

        #region Public Properties

        public StencilAPI API { get; set; }
        public AspectCache Cache { get; set; }
        public NotificationHubClient HubClient { get; protected set; }

        protected string AzurePush_HubName
        {
            get
            {
                return this.Cache.PerLifetime("AzurePush_HubName", delegate ()
                {
                    return this.API.Integration.SettingsResolver.GetSetting(CommonAssumptions.APP_KEY_AZUREPUSH_HUBNAME);
                });
            }
        }
        protected string AzurePush_Connection
        {
            get
            {
                return this.Cache.PerLifetime("AzurePush_Connection", delegate ()
                {
                    return this.API.Integration.SettingsResolver.GetSetting(CommonAssumptions.APP_KEY_AZUREPUSH_CONNECTION);
                });
            }
        }

        #endregion

        #region Stencil Specific - IPushNotifications Methods

        public void NotifyGenericAlert(List<Guid> account_ids, string message, string ignoreGroup)
        {
            base.ExecuteMethod("NotifyGenericAlert", delegate ()
            {
                List<string> targets = new List<string>();
                foreach (var item in account_ids)
                {
                    targets.Add(string.Format(PushAssumptions.TARGET_ACCOUNT_FORMAT, item.ToString().ToLower()));
                }

                this.NotifyGenericAlert(targets, message, ignoreGroup);
            });
        }
        public void NotifyGenericAlert(List<string> targets, string message, string ignoreGroup, string type, string typeParameter)
        {
            base.ExecuteMethod("NotifySpecificAlert", delegate ()
            {
                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(typeParameter))
                {
                    this.NotifyGenericAlert(targets, message, ignoreGroup);
                }
                else
                {
                    if (targets == null || string.IsNullOrEmpty(message))
                    {
                        return;
                    }

                    this.SendAlertTyped(targets, message, ignoreGroup, type, typeParameter);
                }
            });
        }
        public void NotifyGenericAlert(List<string> targets, string message, string ignoreGroup)
        {
            base.ExecuteMethod("NotifyGenericAlert", delegate ()
            {
                if (targets == null || string.IsNullOrEmpty(message))
                {
                    return;
                }

                this.SendAlertOnly(targets, message, ignoreGroup);
            });
        }
        
        public void NotifyBadge(List<Tuple<Guid, int>> account_id_badges)
        {
            base.ExecuteMethod("NotifyBadge", delegate ()
            {
                List<string> targets = new List<string>();

                foreach (Tuple<Guid, int> pair in account_id_badges)
                {
                    string target = string.Format(PushAssumptions.TARGET_ACCOUNT_FORMAT, pair.Item1.ToString().ToLower());
                    this.SendBadgeOnly(pair.Item2, new List<string>() { target });
                }
            });
        }

        public void NotifySpecificAlert(string ignoreGroup, List<string> targets, string internalType, string internalTypeParameter, string internalTypeParameterExtra, string localeKey, params string[] localeArgs)
        {
            base.ExecuteMethod("NotifySpecificAlert", delegate ()
            {
                if (string.IsNullOrEmpty(internalType) || string.IsNullOrEmpty(internalTypeParameter) || string.IsNullOrEmpty(localeKey))
                {
                    return;
                }
                this.SendSpecificAlert(null, ignoreGroup, targets, internalType, internalTypeParameter, internalTypeParameterExtra, localeKey, localeArgs);
            });
        }
        public void NotifySpecificAlert(int badge, string ignoreGroup, string target, string internalType, string internalTypeParameter, string internalTypeParameterExtra, string localeKey, params string[] localeArgs)
        {
            base.ExecuteMethod("NotifySpecificAlert", delegate ()
            {
                if (string.IsNullOrEmpty(internalType) || string.IsNullOrEmpty(internalTypeParameter) || string.IsNullOrEmpty(localeKey))
                {
                    return;
                }
                this.SendSpecificAlert(badge, ignoreGroup, new List<string>() { target }, internalType, internalTypeParameter, internalTypeParameterExtra, localeKey, localeArgs);
            });
        }

        public void NotifySpecificAlertSample(List<Guid> account_ids, Guid route_id, string userName, string message)
        {
            base.ExecuteMethod("NotifySpecificAlertSample", delegate ()
            {
                List<string> targets = new List<string>();
                foreach (var item in account_ids)
                {
                    targets.Add(string.Format(PushAssumptions.TARGET_ACCOUNT_FORMAT, item.ToString().ToLower()));
                }
                this.NotifySpecificAlert("no_snl_sample", targets, "snl_sample", route_id.ToString(), "", "ALERT_SAMPLE", userName, message);
            });
        }


        #endregion

        #region Registration Methods

        public Task RegisterApple(Guid account_id, string deviceToken)
        {
            return Task.Run(async delegate ()
            {
                try
                {
                    NotificationHubClient hubClient = this.HubClient;

                    Account account = this.API.Direct.Accounts.GetById(account_id);
                    string previousRegistrationID = account.push_ios;

                    if (!string.IsNullOrEmpty(previousRegistrationID))
                    {
                        if (await hubClient.RegistrationExistsAsync(previousRegistrationID))
                        {
                            await hubClient.DeleteRegistrationAsync(previousRegistrationID);
                        }
                    }

                    string accountTag = string.Format(PushAssumptions.TARGET_ACCOUNT_FORMAT, account_id.ToString().ToLower());

                    var registrations = await hubClient.GetRegistrationsByTagAsync(accountTag, 100);

                    foreach (RegistrationDescription registration in registrations)
                    {
                        if (registration.Tags.Contains("ios"))
                        {
                            await hubClient.DeleteRegistrationAsync(registration);
                        }
                    }

                    List<string> tags = new List<string>();
                    tags.Add(accountTag);
                    tags.Add("ios");


                    AppleRegistrationDescription newRegistration = await hubClient.CreateAppleNativeRegistrationAsync(deviceToken.Replace(" ", "").Replace("<", "").Replace(">", ""), tags);

                    if (newRegistration != null)
                    {
                        this.API.Direct.Accounts.UpdatePushTokenApple(account.account_id, newRegistration.RegistrationId);
                    }
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex);
                }
            });
        }

        public Task RegisterGoogle(Guid account_id, string deviceToken)
        {
            return Task.Run(async delegate ()
            {
                try
                {
                    NotificationHubClient hubClient = this.HubClient;

                    Account account = this.API.Direct.Accounts.GetById(account_id);
                    string previousRegistrationID = account.push_google;

                    if (!string.IsNullOrEmpty(previousRegistrationID))
                    {
                        if (await hubClient.RegistrationExistsAsync(previousRegistrationID))
                        {
                            await hubClient.DeleteRegistrationAsync(previousRegistrationID);
                        }
                    }

                    string accountTag = string.Format(PushAssumptions.TARGET_ACCOUNT_FORMAT, account_id.ToString().ToLower());

                    var registrations = await hubClient.GetRegistrationsByTagAsync(accountTag, 100);

                    foreach (RegistrationDescription registration in registrations)
                    {
                        if (registration.Tags.Contains("droid"))
                        {
                            await hubClient.DeleteRegistrationAsync(registration);
                        }
                    }

                    List<string> tags = new List<string>();
                    tags.Add(accountTag);
                    tags.Add("droid");


                    GcmRegistrationDescription newRegistration = await hubClient.CreateGcmNativeRegistrationAsync(deviceToken, tags);

                    if (newRegistration != null)
                    {
                        this.API.Direct.Accounts.UpdatePushTokenGoogle(account.account_id, newRegistration.RegistrationId);
                    }
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex);
                }
            });
        }


        public async Task<CollectionQueryResult<RegistrationDescription>> GetAllRegistrationsAsync(string tagName, int top)
        {
            return await base.ExecuteFunction("GetAllRegistrations", async delegate ()
            {
                return await this.HubClient.GetRegistrationsByTagAsync(tagName, top);
            });
        }

        #endregion

        #region MultiSend Methods

        public void SendSpecificAlert(int? badge, string ignoreGroup, List<string> targets, string internalType, string internalTypeParameter, string internalTypeParameterExtra, string localeKey, params string[] localeArgs)
        {
            base.ExecuteMethod("SendSpecificAlert", delegate ()
            {
                Task.Run(delegate () // MVC Issue, Async attempts to re-attach to calling thread (even with await(false))
                {
                    Task iosTask = IOSSendSpecificAlertAsync(badge, ignoreGroup, targets, internalType, internalTypeParameter, internalTypeParameterExtra, localeKey, localeArgs);
                    Task androidTask = AndroidSendSpecificAlertAsync(ignoreGroup, targets, internalType, internalTypeParameter, internalTypeParameterExtra, localeKey, localeArgs);
                    Task.WaitAll(iosTask, androidTask);
                });
            });
        }
        public void SendBadgeOnly(int badge, List<string> targets)
        {
            base.ExecuteMethod("SendBadgeOnly", delegate ()
            {
                Task.Run(delegate () // MVC Issue, Async attempts to re-attach to calling thread (even with await(false))
                {
                    Task iosTask = IOSSendBadgeOnlyAsync(targets, badge, string.Empty);
                    Task.WaitAll(iosTask);
                });
            });
        }

        public void SendAlertOnly(List<string> targets, string message, string ignoreGroup)
        {
            base.ExecuteMethod("SendAlertOnly", delegate ()
            {
                Task.Run(delegate () // MVC Issue, Async attempts to re-attach to calling thread (even with await(false))
                {
                    Task iosTask = IOSSendAlertOnlyAsync(targets, message, ignoreGroup);
                    Task androidTask = AndroidSendAlertOnlyAsync(targets, message, ignoreGroup);
                    Task.WaitAll(iosTask, androidTask);

                });
            });
        }
        public void SendAlertTyped(List<string> targets, string message, string ignoreGroup, string type, string typeParameter)
        {
            base.ExecuteMethod("SendAlertTyped", delegate ()
            {
                Task.Run(delegate () // MVC Issue, Async attempts to re-attach to calling thread (even with await(false))
                {
                    Task iosTask = IOSSendAlertTypedAsync(targets, message, ignoreGroup, type, typeParameter);
                    Task androidTask = AndroidSendAlertTypedAsync(targets, message, ignoreGroup, type, typeParameter);
                    Task.WaitAll(iosTask, androidTask);
                });
            });
        }


        #endregion

        #region IOS Raw Push Methods

        /// <summary>
        /// %0%: Locale Key
        /// %1%: Locale Arguments
        /// %2%: Type of Message
        /// %3%: Type Argument
        /// %4%: Extra Type Argument
        /// %5%: Nonce
        /// </summary>
        private const string IOS_SPECIFIC_ALERT_FORMAT = "{\"aps\":{\"alert\":{\"loc-key\":\"%0%\",\"loc-args\":[%1%]}},\"%2%\":\"%3%\",\"x\":\"%4%\",\"z\":\"%5%\"}"; //ORDER IS IMPORTANT!

        /// <summary>
        /// %badge: Badge
        /// %0%: Locale Key
        /// %1%: Locale Arguments
        /// %2%: Type of Message
        /// %3%: Type Argument
        /// %4%: Extra Type Argument
        /// %5%: Nonce
        /// </summary>
        private const string IOS_SPECIFIC_ALERT_BADGED_FORMAT = "{\"aps\":{\"badge\":%badge%,\"alert\":{\"loc-key\":\"%0%\",\"loc-args\":[%1%]}},\"%2%\":\"%3%\",\"x\":\"%4%\",\"z\":\"%5%\"}"; //ORDER IS IMPORTANT!


        /// <summary>
        /// %0%: Badge Number
        /// </summary>
        private const string IOS_BADGE_ONLY_FORMAT = "{\"aps\":{\"badge\":%0%}}";
        /// <summary>
        /// %0%: Available Download Number
        /// </summary>
        private const string IOS_SYNC_ONLY_FORMAT = "{\"aps\":{\"content-available\":%0%}}";
        /// <summary>
        /// %0%: Badge Number
        /// %1%: Available Download Number
        /// </summary>
        private const string IOS_SYNC_BADGE_AND_DATA_FORMAT = "{\"aps\":{\"badge\":%0%,\"content-available\":%1%}}";
        /// <summary>
        /// %0%: Alert
        /// </summary>
        private const string IOS_ALERT_ONLY_FORMAT = "{\"aps\":{\"alert\":\"%0%\"}}";
        /// <summary>
        /// %0%: Alert
        /// %1%: Type of Message
        /// %2%: Type Argument
        /// </summary>
        private const string IOS_ALERT_TYPED_FORMAT = "{\"aps\":{\"alert\":\"%0%\"},\"%1%\":\"%2%\"}"; //ORDER IS IMPORTANT!

        public async Task IOSSendSpecificAlertAsync(int? badge, string ignoreGroup, List<string> targets, string internalType, string internalTypeParameter, string internalTypeParameterExtra, string localeKey, params string[] localeArgs)
        {
            await base.ExecuteFunction<Task>("IOSSendSpecificAlert", async delegate ()
            {
                if (string.IsNullOrEmpty(internalType) || string.IsNullOrEmpty(internalTypeParameter) || string.IsNullOrEmpty(localeKey))
                {
                    return;
                }
                string args = "";
                for (int i = 0; i < localeArgs.Length; i++)
                {
                    if (i > 0)
                    {
                        args += ",";
                    }
                    args += "\"" + localeArgs[i] + "\"";
                }
                string nonce = ShortGuid.NewGuid().Value.Substring(0, 4);
                string jsonData = "";
                if (badge.HasValue)
                {
                    jsonData = IOS_SPECIFIC_ALERT_BADGED_FORMAT
                                .Replace("%badge%", badge.ToString())
                                .Replace("%0%", localeKey)
                                .Replace("%1%", args)
                                .Replace("%2%", internalType)
                                .Replace("%3%", internalTypeParameter)
                                .Replace("%4%", internalTypeParameterExtra)
                                .Replace("%5%", nonce);
                }
                else
                {
                    jsonData = IOS_SPECIFIC_ALERT_FORMAT
                                        .Replace("%0%", localeKey)
                                        .Replace("%1%", args)
                                        .Replace("%2%", internalType)
                                        .Replace("%3%", internalTypeParameter)
                                        .Replace("%4%", internalTypeParameterExtra)
                                        .Replace("%5%", nonce);
                }

                await this.IOSSendRawAsync(targets, jsonData, ignoreGroup);
            });
        }
        public async Task IOSSendBadgeOnlyAsync(List<string> targets, int badge, string ignoreGroup)
        {
            await base.ExecuteFunction<Task>("SendBadgeOnlyAsync", async delegate ()
            {
                try
                {
                    string alert = IOS_BADGE_ONLY_FORMAT.Replace("%0%", badge.ToString());
                    await this.IOSSendRawAsync(targets, alert, ignoreGroup);
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "SendBadgeOnlyAsync");
                }
            });
        }
        public async Task IOSSendDataOnlyAsync(List<string> targets, int contentCount, string ignoreGroup)
        {
            await base.ExecuteFunction<Task>("SendDataOnlyAsync", async delegate ()
            {
                try
                {
                    string alert = IOS_SYNC_ONLY_FORMAT.Replace("%0%", contentCount.ToString());
                    await this.IOSSendRawAsync(targets, alert, ignoreGroup);
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "SendDataOnlyAsync");
                }
            });
        }
        public async Task IOSSendAlertOnlyAsync(List<string> targets, string message, string ignoreGroup)
        {
            await base.ExecuteFunction<Task>("SendAlertOnlyAsync", async delegate ()
            {
                try
                {
                    string alert = IOS_ALERT_ONLY_FORMAT.Replace("%0%", message);
                    await this.IOSSendRawAsync(targets, alert, ignoreGroup);
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "SendAlertOnlyAsync");
                }
            });
        }
        public async Task IOSSendAlertTypedAsync(List<string> targets, string message, string ignoreGroup, string type, string typeParameter)
        {
            await base.ExecuteFunction<Task>("IOSSendAlertTypedAsync", async delegate ()
            {
                try
                {
                    string alert = IOS_ALERT_TYPED_FORMAT
                        .Replace("%0%", message)
                        .Replace("%1%", type)
                        .Replace("%2%", typeParameter);
                    await this.IOSSendRawAsync(targets, alert, ignoreGroup);
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "IOSSendAlertTypedAsync");
                }
            });
        }
        public async Task IOSSendBadgeAndDataAsync(List<string> targets, int badge, int contentCount, string ignoreGroup)
        {
            await base.ExecuteFunction<Task>("SendBadgeAndDataAsync", async delegate ()
            {
                try
                {
                    string alert = IOS_SYNC_BADGE_AND_DATA_FORMAT
                        .Replace("%0%", badge.ToString())
                        .Replace("%1%", contentCount.ToString());
                    await this.IOSSendRawAsync(targets, alert, ignoreGroup);
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "SendBadgeAndDataAsync");
                }
            });
        }
        public async Task IOSSendRawAsync(List<string> targets, string jsonData, string ignoreGroup)
        {
            await base.ExecuteFunction<Task>("SendRawAsync", async delegate ()
            {
                try
                {
                    if ((targets == null) || (targets.Count == 0) || string.IsNullOrEmpty(jsonData))
                    {
                        return;
                    }
                    int maxExpressions = 19; // limitations of hub send
                    if (!string.IsNullOrEmpty(ignoreGroup))
                    {
                        maxExpressions = 5;
                    }
                    int iterations = (int)Math.Ceiling(targets.Count / (decimal)maxExpressions);
                    int counter = -1;
                    for (int x = 0; x < iterations; x++)
                    {
                        string tagExpression = string.Empty;
                        for (int i = 0; i < maxExpressions; i++)
                        {
                            counter++;
                            if (counter > targets.Count - 1)
                            {
                                break;
                            }
                            if (i > 0)
                            {
                                tagExpression += " || ";
                            }
                            tagExpression += targets[counter];
                        }
                        if (!string.IsNullOrEmpty(ignoreGroup))
                        {
                            tagExpression = string.Format("({0}) && !{1}", tagExpression, ignoreGroup);
                        }
                        var result = await this.HubClient.SendAppleNativeNotificationAsync(jsonData, tagExpression);
                        base.IFoundation.LogTrace(result.Success.ToString());
                    }
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "SendRawAsync");
                }
            });
        }

        #endregion

        #region Android Raw Push Methods

        /// <summary>
        /// %0%: Locale Key
        /// %1%: Locale Arguments
        /// %2%: Type of Message
        /// %3%: Type Argument
        /// %4%: Extra Type Argument
        /// </summary>
        private const string ANDROID_SPECIFIC_ALERT_FORMAT = "{\"data\":{\"type\":\"%2%\",\"type-arg\":\"%3%\",\"loc-key\":\"%0%\",\"loc-args\":[%1%],\"x\":\"%4%\"}}";  //ORDER IS IMPORTANT!

        /// <summary>
        /// %0%: Alert
        /// </summary>
        private const string ANDROID_ALERT_ONLY_FORMAT = "{\"data\":{\"alert\":\"%0%\"}}";

        /// <summary>
        /// %0%: Alert
        /// %1%: Type of Message
        /// %2%: Type Argument
        /// </summary>
        private const string ANDROID_ALERT_TYPED_FORMAT = "{\"data\":{\"type\":\"%1%\",\"type-arg\":\"%2%\",\"alert\":\"%0%\"}}";

        public async Task AndroidSendSpecificAlertAsync(string ignoreGroup, List<string> targets, string internalType, string internalTypeParameter, string internalTypeParameterExtra, string localeKey, params string[] localeArgs)
        {
            await base.ExecuteFunction<Task>("AndroidSendSpecificAlertAsync", async delegate ()
            {
                if (string.IsNullOrEmpty(internalType) || string.IsNullOrEmpty(internalTypeParameter) || string.IsNullOrEmpty(localeKey))
                {
                    return;
                }
                string args = "";
                for (int i = 0; i < localeArgs.Length; i++)
                {
                    if (i > 0)
                    {
                        args += ",";
                    }
                    args += "\"" + localeArgs[i] + "\"";
                }
                string jsonData = ANDROID_SPECIFIC_ALERT_FORMAT
                                    .Replace("%0%", localeKey)
                                    .Replace("%1%", args)
                                    .Replace("%2%", internalType)
                                    .Replace("%3%", internalTypeParameter)
                                    .Replace("%4%", internalTypeParameterExtra);

                await this.AndroidSendRawAsync(targets, jsonData, ignoreGroup);
            });
        }
        public async Task AndroidSendAlertOnlyAsync(List<string> targets, string message, string ignoreGroup)
        {
            await base.ExecuteFunction<Task>("AndroidSendAlertOnlyAsync", async delegate ()
            {
                try
                {
                    string alert = ANDROID_ALERT_ONLY_FORMAT.Replace("%0%", message.ToString());
                    await this.AndroidSendRawAsync(targets, alert, ignoreGroup);
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "SendAlertOnlyAsync");
                }
            });
        }
        public async Task AndroidSendAlertTypedAsync(List<string> targets, string message, string ignoreGroup, string type, string typeParameter)
        {
            await base.ExecuteFunction<Task>("AndroidSendAlertTypedAsync", async delegate ()
            {
                try
                {
                    string alert = ANDROID_ALERT_TYPED_FORMAT
                        .Replace("%0%", message)
                        .Replace("%1%", type)
                        .Replace("%2%", typeParameter);

                    await this.AndroidSendRawAsync(targets, alert, ignoreGroup);
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "AndroidSendAlertTypedAsync");
                }
            });
        }
        public async Task AndroidSendRawAsync(List<string> targets, string jsonData, string ignoreGroup)
        {
            await base.ExecuteFunction<Task>("AndroidSendRawAsync", async delegate ()
            {
                try
                {
                    if ((targets == null) || (targets.Count == 0) || string.IsNullOrEmpty(jsonData))
                    {
                        return;
                    }
                    int maxExpressions = 19; // limitations of hub send
                    if (!string.IsNullOrEmpty(ignoreGroup))
                    {
                        maxExpressions = 5;
                    }
                    int iterations = (int)Math.Ceiling(targets.Count / (decimal)maxExpressions);
                    int counter = -1;
                    for (int x = 0; x < iterations; x++)
                    {
                        string tagExpression = string.Empty;
                        for (int i = 0; i < maxExpressions; i++)
                        {
                            counter++;
                            if (counter > targets.Count - 1)
                            {
                                break;
                            }
                            if (i > 0)
                            {
                                tagExpression += " || ";
                            }
                            tagExpression += targets[counter];
                        }
                        if (!string.IsNullOrEmpty(ignoreGroup))
                        {
                            tagExpression = string.Format("({0}) && !{1}", tagExpression, ignoreGroup);
                        }
                        var result = await this.HubClient.SendGcmNativeNotificationAsync(jsonData, tagExpression);
                        base.IFoundation.LogTrace(result.Success.ToString());
                    }
                }
                catch (Exception ex)
                {
                    base.IFoundation.LogError(ex, "AndroidSendRawAsync");
                }
            });
        }

        #endregion

        #region QA Payloads


        /*
        ANDROID:
        
        {"data":{"type":"snl_sample","type-arg":"f9b9fafd-4b69-622b-0539-ef80ea6f5795","loc-key":"ALERT_SAMPLE","loc-args":["User Name","Specific Text From Server"],"x":"C5CFE782-E7AB-45B9-AB68-80A77C8BF7AC"}}

        IOS:

        {"aps":{"alert":{"loc-key":"ALERT_SAMPLE","loc-args":["User Name","Specific Text From Server"]}},"snl_sample":"f9b9fafd-4b69-622b-0539-ef80ea6f5795","x":"C5CFE782-E7AB-45B9-AB68-80A77C8BF7AC"}
        
         *  */
        #endregion

    }
}