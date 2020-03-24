using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using Geta._404Handler.SandboxApp.Models.ViewModels;
using EPiServer.Web.Mvc;

namespace Geta._404Handler.SandboxApp.Business.Rendering
{
    /// <summary>
    /// Wraps an MvcContentRenderer and adds error handling to ensure that blocks and other content
    /// rendered as parts of pages won't crash the entire page if a non-critical exception occurs while rendering it.
    /// </summary>
    /// <remarks>
    /// Prints an error message for editors so that they can easily report errors to developers.
    /// </remarks>
    public class ErrorHandlingContentRenderer : IContentRenderer
    {
        private readonly MvcContentRenderer _mvcRenderer;
        public ErrorHandlingContentRenderer(MvcContentRenderer mvcRenderer)
        {
            _mvcRenderer = mvcRenderer;
        }

        /// <summary>
        /// Renders the contentData using the wrapped renderer and catches common, non-critical exceptions.
        /// </summary>
        public void Render(HtmlHelper helper, PartialRequest partialRequestHandler, IContentData contentData, TemplateModel templateModel)
        {
            try
            {
                _mvcRenderer.Render(helper, partialRequestHandler, contentData, templateModel);
            }
            catch (NullReferenceException ex)
            {
                if (HttpContext.Current.IsDebuggingEnabled)
                {
                    //If debug="true" we assume a developer is making the request
                    throw;
                }
                HandlerError(helper, contentData, ex);
            }
            catch (ArgumentException ex)
            {
                if (HttpContext.Current.IsDebuggingEnabled)
                {
                    throw;
                }
                HandlerError(helper, contentData, ex);
            }
            catch (ApplicationException ex)
            {
                if (HttpContext.Current.IsDebuggingEnabled)
                {
                    throw;
                }
                HandlerError(helper, contentData, ex);
            }
            catch (InvalidOperationException ex)
            {
                if (HttpContext.Current.IsDebuggingEnabled)
                {
                    throw;
                }
                HandlerError(helper, contentData, ex);
            }
            catch (NotImplementedException ex)
            {
                if (HttpContext.Current.IsDebuggingEnabled)
                {
                    throw;
                }
                HandlerError(helper, contentData, ex);
            }
            catch (IOException ex)
            {
                if (HttpContext.Current.IsDebuggingEnabled)
                {
                    throw;
                }
                HandlerError(helper, contentData, ex);
            }
            catch (EPiServerException ex)
            {
                if (HttpContext.Current.IsDebuggingEnabled)
                {
                    throw;
                }
                HandlerError(helper, contentData, ex);
            }
        }

        private void HandlerError(HtmlHelper helper, IContentData contentData, Exception renderingException)
        {
            if (PrincipalInfo.HasEditAccess)
            {
                var errorModel = new ContentRenderingErrorModel(contentData, renderingException);
                helper.RenderPartial("TemplateError", errorModel);
            }
        }
    }
}
