namespace BVNetwork.NotFound.Core.Logging
{
    public interface IRequestLogger
    {
        void LogRequest(string oldUrl, string referrer);
    }
}