using Foundation;
using MessageUI;
using Stencil.Native.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Stencil.Native.iOS.Core
{
    public abstract class BaseUIViewController : UIViewController, ICoreViewController
    {
        #region Constructors

        public BaseUIViewController()
        {
        }
        public BaseUIViewController(string trackPrefix)
        {
            this.TrackPrefix = trackPrefix;
        }
        public BaseUIViewController(IntPtr handle) : base(handle)
        {
        }
        public BaseUIViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
        }
        public BaseUIViewController(NSObjectFlag t) : base(t)
        {
        }
        public BaseUIViewController(NSCoder coder) : base(coder)
        {
        }

        #endregion


        #region Public Properties
        public string TrackPrefix { get; set; }

        public virtual IStencilApp StencilApp
        {
            get
            {
                return Stencil.Native.Core.Container.StencilApp;
            }
        }

        public bool SupressRefreshDataOnAppear { get; set; }
        public virtual bool IsControllerVisible { get; set; }

        #endregion

        #region Rotate Methods

        public static bool AllowRotate = true; //TODO:COULD: Customize orientation

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            if (AllowRotate)
            {
                return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.LandscapeLeft | UIInterfaceOrientationMask.LandscapeRight;
            }
            return UIInterfaceOrientationMask.Portrait;
        }

        #endregion

        public override void ViewDidAppear(bool animated)
        {
            this.ExecuteMethod("ViewDidAppear", delegate ()
            {
                base.ViewDidAppear(animated);

                this.MailController = this.MailController.DisposeSafe();
                this.SMSController = this.SMSController.DisposeSafe();

                this.SupressRefreshDataOnAppear = false;

                this.ViewControllerToDiposeOnAppear = this.ViewControllerToDiposeOnAppear.DisposeSafe();

            });
        }
        

        #region Overrides


        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.IsControllerVisible = true;
        }
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            this.IsControllerVisible = false;
            this.RaiseViewWillDissapear();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if DEBUG
                if (CoreUtility.ALLOW_DISPOSE_TRACE)
                {
                    this.LogTrace("ViewController.Disposing: " + this.TrackPrefix);
                }
#endif
                this.CustomBackButton = this.CustomBackButton.DisposeSafe();
                if (this.ReturnDismissFields != null)
                {
                    foreach (var item in ReturnDismissFields)
                    {
                        item.ShouldReturn = null;
                    }
                    this.ReturnDismissFields.Clear();
                    this.ReturnDismissFields = null;
                }
                if (this.ReturnDismissViews != null)
                {
                    foreach (var item in ReturnDismissViews)
                    {
                        item.ShouldChangeText = null;
                    }
                    this.ReturnDismissViews.Clear();
                    this.ReturnDismissViews = null;
                }

                if (this.ReturnMoveToFields != null)
                {
                    foreach (var item in ReturnMoveToFields)
                    {
                        item.Key.ShouldReturn = null;
                    }
                    this.ReturnMoveToFields.Clear();
                    this.ReturnMoveToFields = null;
                }
                if (this.NewLineMoveToFields != null)
                {
                    foreach (var item in NewLineMoveToFields)
                    {
                        item.Key.ShouldChangeText = null;
                    }
                    this.NewLineMoveToFields.Clear();
                    this.NewLineMoveToFields = null;
                }
                if (this.ReturnExecuteFields != null)
                {
                    foreach (var item in ReturnExecuteFields)
                    {
                        item.Key.ShouldReturn = null;
                    }
                    this.ReturnExecuteFields.Clear();
                    this.ReturnExecuteFields = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Scroll Methods

        public virtual void ScrollToTop()
        {
            // for inheritance
        }

        #endregion

        #region ICoreViewController Members

        public event EventHandler ViewWillDissappear;
        protected virtual void RaiseViewWillDissapear()
        {
            CoreUtility.ExecuteMethod("RaiseViewWillDissapear", delegate ()
            {
                EventHandler handler = this.ViewWillDissappear;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            });
        }

        #endregion

        #region Navigation Helpers

        public UIViewController ViewControllerToDiposeOnAppear { get; set; }

        public virtual void PushViewControllerWithDisposeOnReturn(UIViewController controller, bool animated)
        {
            this.ExecuteMethod("PushViewControllerWithDisposeOnReturn", delegate ()
            {
                this.ViewControllerToDiposeOnAppear = controller;
                this.NavigationController.PushViewController(controller, true);
            });
        }
        public virtual void PresentViewControllerWithDisposeOnReturn(UIViewController controller, bool animated, Action completion)
        {
            this.ExecuteMethod("PresentViewControllerWithDisposeOnReturn", delegate ()
            {
                this.ViewControllerToDiposeOnAppear = controller;
                this.PresentViewController(controller, animated, completion);
            });
        }

        protected virtual void MakeBackButtonBlank()
        {
            this.ExecuteMethod("MakeBackButtonBlank", delegate ()
            {
                if (this.NavigationItem != null)
                {
                    this.NavigationItem.HidesBackButton = true;
                    this.CustomBackButton = new UIBarButtonItem(UIImage.FromBundle("button_arrow_header.png"), UIBarButtonItemStyle.Plain, BackButton_Clicked);
                    this.NavigationItem.LeftBarButtonItem = this.CustomBackButton;
                }
            });
        }
        protected virtual void BackButton_Clicked(object sender, EventArgs args)
        {
            this.ExecuteMethod("BackButton_Clicked", delegate ()
            {
                this.NavigationController.PopViewController(true);
            });
        }

        /// <summary>
        /// Not always calld
        /// </summary>
        public virtual void OnBecameFocus()
        {
        }
        #endregion

        #region Form Navigation Methods

        protected UIBarButtonItem CustomBackButton { get; set; }
        protected Dictionary<UITextField, UIResponder> ReturnMoveToFields { get; set; }
        protected Dictionary<UITextView, UIResponder> NewLineMoveToFields { get; set; }
        protected Dictionary<UITextField, Action> ReturnExecuteFields { get; set; }
        protected List<UITextField> ReturnDismissFields { get; set; }
        protected List<UITextView> ReturnDismissViews { get; set; }

        public virtual void OnReturnMoveTo(UITextView textView, UIResponder moveToField)
        {
            this.ExecuteMethod("OnReturnMoveTo", delegate ()
            {
                if (this.NewLineMoveToFields == null)
                {
                    this.NewLineMoveToFields = new Dictionary<UITextView, UIResponder>();
                }
                textView.ShouldChangeText += OnNewLineMoveTo_ShouldChangeText;
                this.NewLineMoveToFields[textView] = moveToField;
            });
        }
        public virtual void OnReturnMoveTo(UITextField textField, UIResponder moveToField)
        {
            this.ExecuteMethod("OnReturnMoveTo", delegate ()
            {
                if (this.ReturnMoveToFields == null)
                {
                    this.ReturnMoveToFields = new Dictionary<UITextField, UIResponder>();
                }
                textField.ShouldReturn += OnReturnMoveTo_ShouldReturn;
                this.ReturnMoveToFields[textField] = moveToField;
            });
        }
        public virtual void OnReturnDismiss(UITextView textView)
        {
            this.ExecuteMethod("OnReturnDismiss", delegate ()
            {
                if (this.ReturnDismissViews == null)
                {
                    this.ReturnDismissViews = new List<UITextView>();
                }
                textView.ShouldChangeText += OnNewLineDismiss_ShouldChangeText;
                this.ReturnDismissViews.Add(textView);
            });
        }
        public virtual void OnReturnDismiss(UITextField textField)
        {
            this.ExecuteMethod("OnReturnDismiss", delegate ()
            {
                if (this.ReturnDismissFields == null)
                {
                    this.ReturnDismissFields = new List<UITextField>();
                }
                textField.ShouldReturn += OnReturnDismiss_ShouldReturn;
                this.ReturnDismissFields.Add(textField);
            });
        }
        public virtual void OnReturnExecute(UITextField textField, Action action)
        {
            this.ExecuteMethod("OnReturnExecute", delegate ()
            {
                if (this.ReturnExecuteFields == null)
                {
                    this.ReturnExecuteFields = new Dictionary<UITextField, Action>();
                }
                textField.ShouldReturn += OnReturnExecute_ShouldReturn;
                this.ReturnExecuteFields[textField] = action;
            });
        }

        protected virtual bool OnNewLineDismiss_ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            return this.ExecuteFunction("OnNewLineDismiss_ShouldChangeText", delegate ()
            {
                if (text == "\n")
                {
                    textView.ResignFirstResponder();
                    return false;
                }
                return true;
            });
        }
        protected virtual bool OnNewLineMoveTo_ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            return this.ExecuteFunction("OnNewLineMoveTo_ShouldChangeText", delegate ()
            {
                if (text == "\n")
                {
                    if (this.NewLineMoveToFields.ContainsKey(textView))
                    {
                        this.NewLineMoveToFields[textView].BecomeFirstResponder();
                    }
                    else
                    {
                        textView.ResignFirstResponder();
                    }
                    return false;
                }
                return true;
            });
        }
        protected virtual bool OnReturnMoveTo_ShouldReturn(UITextField textField)
        {
            return this.ExecuteFunction("OnReturnMoveTo_ShouldReturn", delegate ()
            {
                if (this.ReturnMoveToFields.ContainsKey(textField))
                {
                    this.ReturnMoveToFields[textField].BecomeFirstResponder();
                }
                else
                {
                    textField.ResignFirstResponder();
                }
                return true;
            });
        }
        protected virtual bool OnReturnDismiss_ShouldReturn(UITextField textField)
        {
            return this.ExecuteFunction("OnReturnDismiss_ShouldReturn", delegate ()
            {
                textField.ResignFirstResponder();
                return true;
            });
        }
        protected virtual bool OnReturnExecute_ShouldReturn(UITextField textField)
        {
            return this.ExecuteFunction("OnReturnExecute_ShouldReturn", delegate ()
            {
                textField.ResignFirstResponder();
                if (this.ReturnExecuteFields.ContainsKey(textField))
                {
                    this.ReturnExecuteFields[textField]();
                }
                return true;
            });
        }


        #endregion

        #region Email/SMS Helpers

        protected MFMailComposeViewController MailController { get; set; }
        protected MFMessageComposeViewController SMSController { get; set; }


        public virtual bool SendEmail(string subject, string message)
        {
            return CoreUtility.ExecuteFunction("SendEmail", delegate ()
            {
                if (MFMailComposeViewController.CanSendMail)
                {
                    this.MailController = new MFMailComposeViewController();

                    this.MailController.SetSubject(subject);
                    this.MailController.SetMessageBody(message, false);

                    this.MailController.Finished += MailController_Finished;
                    if (this.NavigationController != null)
                    {
                        this.NavigationController.PresentViewController(this.MailController, true, null);
                    }
                    else
                    {
                        this.PresentViewController(this.MailController, true, null);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }
        public virtual bool SendSMS(string message)
        {
            return CoreUtility.ExecuteFunction("SendSMS", delegate ()
            {
                if (MFMessageComposeViewController.CanSendText)
                {
                    this.SMSController = new MFMessageComposeViewController();

                    this.SMSController.Body = message;
                    this.SMSController.Finished += SMSController_Finished;
                    if (this.NavigationController != null)
                    {
                        this.NavigationController.PresentViewController(this.SMSController, true, null);
                    }
                    else
                    {
                        this.PresentViewController(this.SMSController, true, null);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        protected virtual void MailController_Finished(object sender, MFComposeResultEventArgs args)
        {
            this.ExecuteMethodOnMainThread("MailController_Finished", delegate ()
            {
                args.Controller.Finished -= MailController_Finished;
                args.Controller.DismissViewController(true, null);
            });
        }
        protected virtual void SMSController_Finished(object sender, MFMessageComposeResultEventArgs args)
        {
            this.ExecuteMethodOnMainThread("SMSController_Finished", delegate ()
            {
                args.Controller.Finished -= SMSController_Finished;
                args.Controller.DismissViewController(true, null);
            });
        }

        #endregion


        #region Aspect Methods

        public virtual void ExecuteMethodOnMainThread(string name, Action method)
        {
            if (NSThread.IsMain)
            {
                this.ExecuteMethod(name, method);
            }
            else
            {
                this.InvokeOnMainThread(delegate ()
                {
                    this.ExecuteMethod(name, method);
                });
            }
        }
        protected virtual void ExecuteMethodOnMainThreadBegin(string name, Action method)
        {
            this.BeginInvokeOnMainThread(delegate ()
            {
                this.ExecuteMethod(name, method);
            });
        }


        protected virtual void ExecuteMethod(string name, Action method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
            CoreUtility.ExecuteMethod(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError, supressMethodLogging);
        }
        protected virtual Task ExecuteMethodAsync(string name, Func<Task> method, Action<Exception> onError = null)
        {
            return CoreUtility.ExecuteMethodAsync(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError);
        }
        protected virtual T ExecuteFunction<T>(string name, Func<T> method, Action<Exception> onError = null)
        {
            return CoreUtility.ExecuteFunction<T>(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError);
        }
        protected virtual Task<T> ExecuteFunctionAsync<T>(string name, Func<Task<T>> method, Action<Exception> onError = null)
        {
            return CoreUtility.ExecuteFunctionAsync<T>(string.Format("{0}.{1}", this.TrackPrefix, name), method, onError);
        }

        public virtual Action CreateNSAction(string name, Action method, Action<Exception> onError = null)
        {
            return delegate ()
            {
                ExecuteMethod(name, method, onError);
            };
        }
        public virtual Action CreateNSActionOnMainThread(string name, Action method, Action<Exception> onError = null)
        {
            return delegate ()
            {
                ExecuteMethodOnMainThread(name, method);
            };
        }


        private HashSet<string> _executingCommands = new HashSet<string>();
        protected virtual HashSet<string> executingCommands
        {
            get
            {
                return _executingCommands;
            }
        }

        /// <summary>
        /// Executes the command unless the command is already running, then its skipped
        /// </summary>
        protected virtual void ExecuteMethodOrSkip(string name, Action method, Action<Exception> onError = null)
        {
            this.ExecuteMethod("ExecuteOrSkip", delegate ()
            {
                bool added = _executingCommands.Add(name);
                if (!added) { return; }
                try
                {
                    method();
                }
                finally
                {
                    _executingCommands.Remove(name);
                }
            });
        }
        /// <summary>
        /// Executes the command unless the command is already running, then its skipped
        /// </summary>
        protected virtual Task ExecuteMethodOrSkipAsync(string name, Func<Task> method, Action<Exception> onError = null)
        {
            return this.ExecuteMethodAsync("ExecuteMethodOrSkipAsync", async delegate ()
            {
                bool added = _executingCommands.Add(name);
                if (!added) { return; }
                try
                {
                    await method();
                }
                finally
                {
                    _executingCommands.Remove(name);
                }
            });
        }

        protected virtual bool IsExecutingCommand(string name)
        {
            return this.ExecuteFunction("IsExecutingCommand", delegate ()
            {
                return _executingCommands.Contains(name);
            });
        }

        #endregion

        #region Tracking Methods

        protected virtual void LogWarning(string message, string tag = "")
        {
            Container.Track.LogWarning(this.TrackPrefix + ":" + message, tag);
        }
        protected virtual void LogTrace(string message, string tag = "")
        {
            Container.Track.LogTrace(this.TrackPrefix + ":" + message, tag);
        }
        protected virtual void LogError(Exception ex, string tag = "")
        {
            Container.Track.LogError(ex, this.TrackPrefix + ":" + tag);
        }
        protected virtual void LogError(NSError error, string tag = "")
        {
            Container.Track.LogError(error.ConvertToException(), this.TrackPrefix + ":" + tag);
        }

        #endregion


    }
}

