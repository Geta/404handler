using System;
using System.Collections.Generic;
using System.Data;

namespace BVNetwork.NotFound.Core.Data
{
    public class DataHandler
    {
        public static string UknownReferer = "Uknown referers";

        public static Dictionary<string, int> GetRedirects()
        {
            var keyCounts = new Dictionary<string, int>();
            var dabe = DataAccessBaseEx.GetWorker();
            using (var allkeys = dabe.GetAllClientRequestCount())
            {
                foreach (DataTable table in allkeys.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var oldUrl = row[0].ToString();
                        keyCounts.Add(oldUrl, Convert.ToInt32(row[1]));
                    }
                }
            }

            return keyCounts;
        }

        public static Dictionary<string, int> GetReferers(string url)
        {
            var dataAccess = DataAccessBaseEx.GetWorker();
            var referers = new Dictionary<string, int>();

            using (var referersDs = dataAccess.GetRequestReferers(url))
            {
                var table = referersDs.Tables[0];
                if (table == null) return referers;

                var unknownReferers = 0;
                foreach (DataRow row in table.Rows)
                {
                    var referer = row[0].ToString();
                    var count = Convert.ToInt32(row[1].ToString());
                    if (referer.Trim() != string.Empty
                        && !referer.Contains("(null)"))
                    {
                        if (!referer.Contains("://")) referer = referer.Insert(0, "/");
                        referers.Add(referer, count);
                    }
                    else
                    {
                        unknownReferers += count;
                    }

                }
                if (unknownReferers > 0)
                {
                    referers.Add(UknownReferer, unknownReferers);
                }
            }

            return referers;
        }

        public static int GetTotalSuggestionCount()
        {
            var dataAccess = DataAccessBaseEx.GetWorker();
            return dataAccess.GetTotalNumberOfSuggestions();
        }
    }
}