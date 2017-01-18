using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.ViewModels
{
    public abstract class BaseViewModel : BaseViewModel<IViewModelView>
    {
        public BaseViewModel(IViewModelView view, string trackPrefix)
            : base(view, trackPrefix)
        {
        }
    }
    public abstract class BaseViewModel<TViewModelView> : BaseViewModelLanguage
    {
        public BaseViewModel(TViewModelView view, string trackPrefix)
            : base(trackPrefix)
        {
            this.View = view;
        }

        public bool HasStarted { get; set; }
        public bool HasAppeared { get; set; }
        public bool SeemsVisible { get; set; }

        public TViewModelView View { get; set; }

        public virtual void OnAppear()
        {
            base.ExecuteMethod("OnAppear", delegate ()
            {
                this.HasAppeared = true;
                this.SeemsVisible = true;
            });
        }
        public virtual void OnDisappear()
        {
            base.ExecuteMethod("OnDisappear", delegate ()
            {
                this.SeemsVisible = false;
            });
        }

        public virtual void Start()
        {
            base.ExecuteMethod("Start", delegate ()
            {
                this.HasStarted = true;
            });
        }

    }
}
