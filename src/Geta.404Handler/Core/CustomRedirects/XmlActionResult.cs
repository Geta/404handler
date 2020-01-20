using System;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public sealed class XmlActionResult : ActionResult
    {
        private readonly XmlDocument _document;

        public Formatting Formatting { get; set; }
        public string MimeType { get; set; }

        public XmlActionResult(XmlDocument document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));

            MimeType = "text/xml";
            Formatting = Formatting.Indented;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.Buffer = true;
            context.HttpContext.Response.ContentType = MimeType;
            context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=customRedirects.xml");

            using (var writer = new XmlTextWriter(context.HttpContext.Response.OutputStream, Encoding.UTF8) { Formatting = Formatting })
                _document.WriteTo(writer);
        }
    }
}
