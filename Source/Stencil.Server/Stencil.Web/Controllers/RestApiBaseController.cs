using Codeable.Foundation.Common;
using Codeable.Foundation.Common.System;
using Codeable.Foundation.Core;
using Codeable.Foundation.UI.Web.Core.MVC;
using Stencil.Common.Exceptions;
using Stencil.Primary;
using Stencil.SDK;
using Stencil.Web.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stencil.Web.Controllers
{
    public abstract class RestApiBaseController : CoreApiController
    {
        #region Constructors

        [Obsolete("Should only be used via tooling, no developer should call this directly.", true)]
        public RestApiBaseController()
            : base()
        {
            this.API = CoreFoundation.Current.Resolve<StencilAPI>();
        }
        public RestApiBaseController(IFoundation iFoundation)
            : base(iFoundation)
        {
            this.API = iFoundation.Resolve<StencilAPI>();
        }
        public RestApiBaseController(IFoundation iFoundation, IHandleExceptionProvider iHandleExceptionProvider)
            : base(iFoundation, iHandleExceptionProvider)
        {
            this.API = iFoundation.Resolve<StencilAPI>();
        }

        #endregion

        public virtual StencilAPI API { get; set; }

        public IFoundation Foundation
        {
            get
            {
                return base.IFoundation;
            }
        }

        public virtual IDictionary<string, object> RequestProperties
        {
            get { return this.Request.Properties; }
        }

        protected virtual void ValidateNotNull<T>(Nullable<T> entity, string entityName)
            where T : struct
        {
            base.ExecuteMethod("ValidateNotNull", delegate ()
            {
                if (!entity.HasValue)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(string.Format("Invalid {0} provided", entityName)),
                        ReasonPhrase = string.Format("Invalid {0} provided", entityName),
                    });
                }
            });
        }

        protected virtual void ValidateNotNull<T>(T entity, string entityName)
            where T : class
        {
            base.ExecuteMethod("ValidateNotNull", delegate ()
            {
                if (entity == null)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(string.Format("Invalid {0} provided", entityName)),
                        ReasonPhrase = string.Format("Invalid {0} provided", entityName),
                    });
                }
            });
        }
        protected virtual void ValidateRouteMatch<T>(T routeId, T entityId, string entityName)
        {
            base.ExecuteMethod("ValidateRouteMatch", delegate ()
            {
                if (!routeId.Equals(entityId))
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(string.Format("Identifier mismatch for the {0} provided", entityName)),
                        ReasonPhrase = string.Format("Identifier mismatch for the {0} provided", entityName),
                    });
                }
            });
        }
        protected virtual void ValidateOffset(int value)
        {
            base.ExecuteMethod("ValidateOffset", delegate ()
            {
                if (value < 1)
                {
                    string message = "Offset must be greater than zero";
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(message),
                        ReasonPhrase = message,
                    });
                }
            });
        }
        protected virtual void ValidateLimit(int value, int max)
        {
            base.ExecuteMethod("ValidateOffset", delegate ()
            {
                if ((value > max) || (value < 1))
                {
                    string message = string.Format("Limit must be between {0} and {1}", 1, max);
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(message),
                        ReasonPhrase = message,
                    });
                }
            });
        }
        protected virtual HttpResponseMessage Http500(string reason)
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http500", delegate ()
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(reason),
                    ReasonPhrase = reason,
                };
            });
        }
        protected virtual HttpResponseMessage Http400(string reason)
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http400", delegate ()
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(reason),
                    ReasonPhrase = reason,
                };
            });
        }
        protected virtual HttpResponseMessage Http401(string reason)
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http401", delegate ()
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent(reason),
                    ReasonPhrase = reason,
                };
            });
        }
        protected virtual HttpResponseMessage Http404(string entityName)
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http404", delegate ()
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("Unable to find requested {0}", entityName)),
                    ReasonPhrase = string.Format("Unable to find requested {0}", entityName),
                };
            });
        }
        protected virtual HttpResponseMessage Http200(object body = null, string urlSuffix = "", string uriAuthority = "")
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http200", delegate ()
            {
                HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.OK, body);
                try
                {
                    string uriSuffix = string.Empty;
                    if (!string.IsNullOrEmpty(urlSuffix))
                    {
                        uriSuffix = urlSuffix.TrimStart('/');
                    }
                    if (string.IsNullOrWhiteSpace(uriAuthority))
                    {
                        uriAuthority = this.Request.RequestUri.GetLeftPart(UriPartial.Authority);
                    }
                    if (!string.IsNullOrEmpty(uriSuffix))
                    {
                        result.Headers.Location = new Uri(string.Format("{0}/{1}", uriAuthority, uriSuffix));
                    }
                }
                catch (Exception ex) // most likely bad uri
                {
                    this.IFoundation.LogError(ex, "Http200");
                }
                return result;
            });
        }
        protected virtual HttpResponseMessage Http301(string location)
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http301", delegate ()
            {
                HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.Moved);
                result.Headers.Location = new Uri(location);
                return result;
            });
        }
        protected virtual HttpResponseMessage Http201(object body, string urlSuffix, string uriAuthority = "")
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http201", delegate ()
            {
                HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.Created, body);
                try
                {
                    string uriSuffix = urlSuffix.TrimStart('/');
                    if (string.IsNullOrWhiteSpace(uriAuthority))
                    {
                        uriAuthority = this.Request.RequestUri.GetLeftPart(UriPartial.Authority);
                    }
                    result.Headers.Location = new Uri(string.Format("{0}/{1}", uriAuthority, uriSuffix));
                }
                catch (Exception ex) // most likely bad uri
                {
                    this.IFoundation.LogError(ex, "Http201");
                }
                return result;
            });
        }
        protected virtual HttpResponseMessage Http204(object body = null)
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http204", delegate ()
            {
                HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.NoContent, body);
                return result;
            });
        }
        protected virtual HttpResponseMessage Http409(object body = null)
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http409", delegate ()
            {
                HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.Conflict, body);
                return result;
            });
        }

        protected virtual HttpResponseMessage Http307(string url)
        {
            return base.ExecuteFunction<HttpResponseMessage>("Http307", delegate ()
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.TemporaryRedirect);
                response.Headers.Location = new Uri(url);
                return response;
            });
        }

        protected virtual HttpResponseMessage SimpleFileDownload(string fileName, string fullPath)
        {
            return base.ExecuteFunction<HttpResponseMessage>("SimpleFileDownload", delegate ()
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new FileStream(fullPath, FileMode.Open));
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = fileName;
                return response;
            });
        }
        protected virtual HttpResponseMessage SimpleFileDownload(string fileName, byte[] data)
        {
            return base.ExecuteFunction<HttpResponseMessage>("SimpleFileDownload", delegate ()
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(data));
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = fileName;
                return response;
            });
        }
        protected virtual HttpResponseMessage SimpleFileInline(string fileName, string mimeType, byte[] data)
        {
            return base.ExecuteFunction<HttpResponseMessage>("SimpleFileInline", delegate ()
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(data));
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("inline");
                response.Content.Headers.ContentDisposition.FileName = fileName;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                return response;
            });
        }
        protected virtual HttpResponseMessage SimpleHtmlString(string htmlString)
        {
            return base.ExecuteFunction<HttpResponseMessage>("SimpleHtmlString", delegate ()
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(htmlString);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                return response;

            });
        }
        protected virtual HttpResponseMessage SimpleXmlString(string xmlString)
        {
            return base.ExecuteFunction<HttpResponseMessage>("SimpleXmlString", delegate ()
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(xmlString);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                return response;

            });
        }


        protected override K ExecuteFunction<K>(string methodName, Func<K> function, params object[] parameters)
        {
            try
            {
                return base.ExecuteFunction<K>(methodName, function, parameters);
            }
            catch (Exception ex)
            {
                if (ex is UIException)
                {
                    string platform = this.GetClientPlatform();
                    if (platform == "ios" || platform == "android")
                    {
                        return (K)(object)Http200(new ActionResult()
                        {
                            success = false,
                            message = ex.Message
                        });
                    }
                    return (K)(object)Http400(ex.Message);
                }
                throw;
            }
        }

        //TODO:FOUNDATION:SHOULD: Move to Codeable Libraries
        [Obsolete("Incorrect api call, use the Async Version of this method", true)]
        protected void ExecuteMethod(string name, Func<Task> method, Action<Exception> onError = null, bool supressMethodLogging = false)
        {
        }
        protected virtual async Task<K> ExecuteFunctionAsync<K>(string name, Func<Task<K>> method, Action<Exception> onError = null, bool forceThrow = false)
        {
            try
            {
                return await this.ExecuteFunctionAsyncInternal<K>(name, method, onError, forceThrow);
            }
            catch (Exception ex)
            {
                if (ex is UIException)
                {
                    return (K)(object)Http400(ex.Message);
                }
                throw;
            }
        }

        //TODO:FOUNDATION:SHOULD: Move to Codeable Libraries
        private async Task<T> ExecuteFunctionAsyncInternal<T>(string name, Func<Task<T>> method, Action<Exception> onError = null, bool forceThrow = false)
        {
            try
            {
                T result = await method();
                return result;
            }
            catch (Exception ex)
            {
                base.IFoundation.LogError(ex, name);
                bool rethrow = false;
                Exception replacedException = null;
                bool expectsRethrow = false;
                IHandleException exceptionHandler = this.IHandleExceptionProvider.CreateHandler();
                if (exceptionHandler != null)
                {
                    if (exceptionHandler.HandleException(ex, out expectsRethrow, out replacedException))
                    {
                        rethrow = expectsRethrow;
                        if (replacedException != null)
                        {
                            throw replacedException;
                        }
                    }
                }

                if (forceThrow || rethrow)
                {
                    throw;
                }
                return default(T);
            }
        }

    }
}
