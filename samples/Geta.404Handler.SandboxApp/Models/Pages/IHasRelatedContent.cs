using EPiServer.Core;

namespace Geta._404Handler.SandboxApp.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
