using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.Core
{
    public partial interface IViewPlatform
    {
        string ShortName { get; }
        string VersionNumber { get; }

        void NavigateToFirstScreen();

        void OnLoggedOff();
        void OnLoggedOn();
        void OnOutDated(string message);

        void OnAccountRefreshed();

        void UpdatePushNotificationToken();
        void RegisterForPushNotificationsWithPrePromptIfNeeded(string factionName);
        void UnRegisterForPushNotifications();


        void ShowToast(string message);
        void DisplayNotification(string title, string message);


        void SyncBadge();
        void SetBadge(int badge);

        void OnMemoryWarning();

    }
}
