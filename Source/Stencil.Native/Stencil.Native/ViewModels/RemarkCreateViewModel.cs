using System;
using System.Threading.Tasks;
using Stencil.Native.Core;
using Stencil.Native.ViewModels;
using Stencil.SDK;
using Stencil.SDK.Models;

namespace Stencil.Native
{
    public class RemarkCreateViewModel : BaseViewModel
    {
        public RemarkCreateViewModel(IViewModelView view, Post post)
            : base(view, "RemarkCreateViewModel")
        {
            this.Post = post;
        }

        public Post Post { get; set; }

        public string Text_Title
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.RemarksCreate_Title, "New Remark");
            }
        }
        public string Text_Create
        {
            get
            {
                return this.StencilApp.GetLocalizedText(I18NToken.RemarksCreate_Create, "Create");
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

        public Task<ItemResult<Remark>> CreateRemarkAsync(string body)
        {
            return base.ExecuteFunctionAsync("CreateRemarkAsync", async delegate ()
            {
                string validationMessage = this.Validate(body);
                if(!string.IsNullOrEmpty(validationMessage))
                {
                    return new ItemResult<Remark>()
                    {
                        success = false,
                        message = validationMessage
                    };
                }
                else
                {
                    try
                    {
                        ItemResult<Remark> response = await this.StencilApp.RemarkCreateAsync(new Remark()
                        {
                            account_id = this.StencilApp.CurrentAccount.account_id,
                            post_id = this.Post.post_id,
                            stamp_utc = DateTime.UtcNow,
                            text = body
                        });
                        return response;
                    }
                    catch(Exception ex)
                    {
                        return new ItemResult<Remark>()
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
