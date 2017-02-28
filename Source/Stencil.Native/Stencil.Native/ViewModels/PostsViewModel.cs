using System;
using System.Threading.Tasks;
using Stencil.Native.Caching;
using Stencil.Native.ViewModels;
using Stencil.SDK.Models;

namespace Stencil.Native
{
    public class PostsViewModel : BaseDataListViewModel<Post>
    {
        #region Constructor

        public PostsViewModel(IDataListViewModelView<Post> view)
            : base(view, "PostsViewModel")
        {
        }

        #endregion

        #region Language


        public string Text_Title
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.Posts_Title, "Posts").ToUpper();
            }
        }

        #endregion

        #region Properties

        public const string TOKEN_TEXT = "text";
        public const string TOKEN_POST = "post";


        public override int ScrollThresholdCount
        {
            get
            {
                return this.StencilApp.AppConfig.Posts.ScrollThresholdCount;
            }
        }
        public override int ScrollThresholdSize
        {
            get
            {
                return this.StencilApp.AppConfig.Posts.ScrollThresholdSize;
            }
        }


        #endregion

        #region Protected Methods

        public override void Start()
        {
            base.ExecuteMethod("Start", delegate ()
            {
                base.Start();
            });
        }

        protected override Task PerformRefreshData(RequestToken requestToken, bool force)
        {
            return base.ExecuteFunction("PerformRefreshData", delegate ()
            {
                return this.StencilApp.PostsGetAsync(requestToken, true, force, 0, this.StencilApp.AppConfig.Posts.PageSize, base.OnDataRetrieved);
            });
        }
        protected override Task PerformGetMoreData(RequestToken requestToken, int skip)
        {
            return base.ExecuteFunction("PerformGetMoreData", delegate ()
            {
                return this.StencilApp.PostsGetAsync(requestToken, false, false, skip, this.StencilApp.AppConfig.Posts.PageSize, base.OnMoreDataRetrieved);
            });
        }

        #endregion

        #region Public Methods


        #endregion
    }
}
