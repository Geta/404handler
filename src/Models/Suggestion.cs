using BVNetwork.NotFound.Core.CustomRedirects;

namespace BVNetwork.NotFound.Models
{
    public class Suggestion
    {
        public CustomRedirect CustomRedirect { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchWord { get; set; }

        public Suggestion(CustomRedirect customRedirect, int pageNumber, int pageSize, string searchWord)
        {
            CustomRedirect = customRedirect;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchWord = searchWord;
        }
    }
}