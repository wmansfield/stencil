using System;
using Android.Widget;
using Android.Content;
using Android.Support.V7.App;
using System.Threading.Tasks;
using Stencil.Native.Core;

namespace Stencil.Native.Droid.Core
{
    public static class HUD
    {
        
        public static void ShowToast(Context context, string message)
        {
            Toast toast = Toast.MakeText(context, message, ToastLength.Long);
            toast.Show();
        }
        private static AlertDialog _recentDialog;
        private static object _dialogSyncRoot = new object();

        public static void Show(Context context, string message)
        {
            CoreUtility.ExecuteMethod("Show", delegate()
            {
                AlertDialog dialog = new AlertDialog.Builder(context)
                    .SetCancelable(false)
                    .SetMessage(message)
                    .Create();

                AlertDialog dismissDialog = null;
                lock(_dialogSyncRoot)
                {
                    dismissDialog = _recentDialog;
                    _recentDialog = dialog;
                }
                if (dismissDialog != null)
                {
                    dismissDialog.Dismiss();
                }
                dialog.Show();
            });
        }
        public static void ShowErrorWithStatus(Context context, string message, int timeoutMs)
        {
            CoreUtility.ExecuteMethod("ShowErrorWithStatus", delegate()
            {
                AlertDialog dialog = new AlertDialog.Builder(context)
                    .SetCancelable(false)
                    .SetIcon(Resource.Drawable.abc_ic_ab_back_material)
                    .SetMessage(message)
                    .Create();

                HUD.Dismiss();

                dialog.Show();
                Task.Delay(timeoutMs).ContinueWith(delegate(Task arg) 
                {
                    dialog.Dismiss();
                });

            });
        }
        public static void ShowSuccessWithStatus(Context context, string message, int timeoutMs)
        {
            CoreUtility.ExecuteMethod("ShowSuccessWithStatus", delegate()
            {
                AlertDialog dialog = new AlertDialog.Builder(context)
                    .SetCancelable(false)
                    .SetIcon(Resource.Drawable.abc_ic_commit_search_api_mtrl_alpha)
                    .SetMessage(message)
                    .Create();

                HUD.Dismiss();

                dialog.Show();
                Task.Delay(timeoutMs).ContinueWith(delegate(Task arg) 
                {
                    dialog.Dismiss();
                });

            });
        }
        public static void Dismiss()
        {
            CoreUtility.ExecuteMethod("Dismiss", delegate() 
            {
                AlertDialog recent = null;
                lock(_dialogSyncRoot)
                {
                    recent = _recentDialog;
                    _recentDialog = null;
                }
                if (recent != null)
                {
                    recent.Dismiss();
                }
            });


        }
    }
}

