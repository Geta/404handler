using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BVNetwork.FileNotFound.Redirects;

namespace BVNetwork.FileNotFound
{
    public class RedirectIndexViewData
    {
        public List<CustomRedirect> CustomRedirectList { get; set; }

        public int PageSize {  get; set; }
        public int PageNumber { get; set; }
        public int PagerSize {  get;  set; }
        public int TotalItemsCount { get; set; }
        public string ActionInformation { get; set; }
        public string SearchWord { get; set; }
        public bool IsSuggestions { get; set; }
        public int HighestSuggestionValue { get; set; }
        public int LowestSuggestionValue { get; set; }


        public IEnumerable<int> Pages
        {
            get
            {
                List<int> list2 = new List<int>();
                list2.Add(1);
                List<int> list = list2;
                if (((this.PageNumber - this.PagerSize) - 1) > 1)
                {
                    list.Add(0);
                }
                for (int i = this.PageNumber - this.PagerSize; i <= (this.PageNumber + this.PagerSize); i++)
                {
                    if ((i > 1) && (i < this.TotalPagesCount))
                    {
                        list.Add(i);
                    }
                }
                if (((this.PageNumber + this.PagerSize) + 1) < this.TotalPagesCount)
                {
                    list.Add(0);
                }
                if (this.TotalPagesCount > 1)
                {
                    list.Add(this.TotalPagesCount);
                }
                return list;
            }
        }


        public int TotalPagesCount
        {
            get
            {
                return (((this.TotalItemsCount - 1) / this.PageSize) + 1);
            }
        }

        public int MaxIndexOfItem
        {
            get
            {
                if ((this.PageNumber * this.PageSize) <= this.TotalItemsCount)
                {
                    return (this.PageNumber * this.PageSize);
                }
                return this.TotalItemsCount;
            }
        }

        public int MinIndexOfItem
        {
            get
            {
                if (this.TotalItemsCount <= 0)
                {
                    return 0;
                }
                return (((this.PageNumber - 1) * this.PageSize) + 1);
            }
        }



    }   
}
