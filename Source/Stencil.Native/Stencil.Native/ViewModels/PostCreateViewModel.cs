using System;
using System.Threading.Tasks;
using Stencil.Native.Core;
using Stencil.Native.ViewModels;
using Stencil.SDK;
using Stencil.SDK.Models;

namespace Stencil.Native
{
    public class PostCreateViewModel : BaseViewModel
    {
        public PostCreateViewModel(IViewModelView view)
            : base(view, "PostCreateViewModel")
        {

        }

        public string Text_Title
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.PostCreate_Title, "New Post");
            }
        }
        public string Text_Create
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.PostCreate_Create, "Create");
            }
        }
      

        public string Validate(string body)
        {
            return base.ExecuteFunction("Validate", delegate ()
            {
                if(string.IsNullOrEmpty(body))
                {
                    return this.Text_General_MustProvideText;
                }

                return string.Empty;
            });
        }

        public Task<ItemResult<Post>> CreatePostAsync(string body)
        {
            return base.ExecuteFunctionAsync("CreatePostAsync", async delegate ()
            {
                string validationMessage = this.Validate(body);
                if(!string.IsNullOrEmpty(validationMessage))
                {
                    return new ItemResult<Post>()
                    {
                        success = false,
                        message = validationMessage
                    };
                }
                else
                {
                    try
                    {
                        ItemResult<Post> response = await this.StencilApp.PostCreateAsync(new Post()
                        {
                            account_id = this.StencilApp.CurrentAccount.account_id,
                            stamp_utc = DateTime.UtcNow,
                            body = body
                        });
                        return response;
                    }
                    catch(Exception ex)
                    {
                        return new ItemResult<Post>()
                        {
                            success = false,
                            message = ex.FirstNonAggregateException().Message
                        };
                    }
                }
            });
        }
    }
}
