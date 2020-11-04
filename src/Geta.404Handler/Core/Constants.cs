namespace BVNetwork.NotFound.Core
{
    public static class Constants
    {
        public const int Gone = 410;
        public const int Permanent = 301;
        public const int Temporary = 302;
        
        public enum RedirectTypeCode
        {
            Permanent = 301,
            Temporary = 302
        }
    }
}
