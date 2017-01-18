using System;
using Newtonsoft.Json;

namespace Stencil.Native.Droid.Core
{
    public abstract class BaseRoutedFragment<TRoute> : BaseFragment
        where TRoute : class
    {
        public BaseRoutedFragment(string trackPrefix)
            : base(trackPrefix)
        {
            this.TrackPrefix = trackPrefix;
        }

        private TRoute _routeData;
        public virtual TRoute RouteData
        {
            get
            {
                this.EnsureRouteData(this.Arguments);
                return _routeData;
            }
            set
            {
                _routeData = value;
            }
        }


        #region Overrides

        protected virtual void EnsureRouteData(Android.OS.Bundle bundle)
        {
            this.ExecuteMethod("EnsureRouteData", delegate()
            {
                if (_routeData == null)
                {
                    string json = bundle.GetString(AndroidAssumptions.ROUTE_KEY);
                    if(!string.IsNullOrEmpty(json))
                    {
                        this.RouteData = JsonConvert.DeserializeObject<TRoute>(json);
                    }
                }
            });
        }

        public override void OnSaveInstanceState(Android.OS.Bundle outState)
        {
            this.ExecuteMethod("OnSaveInstanceState", delegate()
            {
                base.OnSaveInstanceState(outState);

                if(this.RouteData != null)
                {
                    outState.PutString(AndroidAssumptions.ROUTE_KEY, JsonConvert.SerializeObject(this.RouteData));
                }
            });
        }
        public override void OnActivityCreated(Android.OS.Bundle savedInstanceState)
        {
            this.ExecuteMethod("OnActivityCreated", delegate()
            {
                base.OnActivityCreated(savedInstanceState);

                this.EnsureRouteData(savedInstanceState);
            });
        }

        #endregion

    }
}

