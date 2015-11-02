using System;
using System.Collections;
using System.Web;
using BVNetwork.NotFound.Configuration;
using BVNetwork.NotFound.Core.Data;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
	/// <summary>
	/// A collection of custom urls
	/// </summary>
	public class CustomRedirectCollection: CollectionBase
	{
		/// <summary>
		/// Hashtable for quick lookup of old urls
		/// </summary>
		private Hashtable _quickLookupTable = null;

		public CustomRedirectCollection()
		{
			// Create case insensitive hash table
			_quickLookupTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		}


		// public methods...
		#region Add
		public int Add(CustomRedirect customRedirect)
		{
			// Add to quick look up table too
			_quickLookupTable.Add(customRedirect.OldUrl, customRedirect);
			return List.Add(customRedirect);
		}
		#endregion
		#region IndexOf
		public int IndexOf(CustomRedirect customRedirect)
		{
			for(int i = 0; i < List.Count; i++)
				if (this[i] == customRedirect)    // Found it
					return i;
			return -1;
		}
		#endregion
		#region Insert
		public void Insert(int index, CustomRedirect customRedirect)
		{
			_quickLookupTable.Add(customRedirect, customRedirect);
			List.Insert(index, customRedirect);
		}
		#endregion
		#region Remove
		public void Remove(CustomRedirect customRedirect)
		{
			_quickLookupTable.Remove(customRedirect);
			List.Remove(customRedirect);
		}
		#endregion
		#region Find
		// TODO: If desired, change parameters to Find method to search based on a property of CustomRedirect.
		public CustomRedirect Find(Uri urlNotFound)
		{
            // Handle absolute addresses first
		    string url = urlNotFound.AbsoluteUri;
            CustomRedirect foundRedirect = FindInternal(url);

            // Common case
            if (foundRedirect == null)
            {
                url = urlNotFound.PathAndQuery; ;
                foundRedirect = FindInternal(url);
            }

            // Handle legacy databases with encoded values
            if (foundRedirect == null)
            {
                url = HttpUtility.HtmlEncode(url);
                foundRedirect = FindInternal(url);
            }

		    return foundRedirect;
		}

	    private CustomRedirect FindInternal(string url)
	    {
	        object foundRedirect = _quickLookupTable[url];
	        if (foundRedirect != null)
	        {
	            return foundRedirect as CustomRedirect;
	        }
	        else
	        {
	            // No exact match could be done, so we'll check if the 404 url
	            // starts with one of the urls we're matching against. This
	            // will be kind of a wild card match (even though we only check
	            // for the start of the url
	            // Example: http://www.mysite.com/news/mynews.html is not found
	            // We have defined an "<old>/news</old>" entry in the config
	            // file. We will get a match on the /news part of /news/myne...
	            // Depending on the skip wild card append setting, we will either
	            // redirect using the <new> url as is, or we'll append the 404
	            // url to the <new> url.
	            IDictionaryEnumerator _enumerator = _quickLookupTable.GetEnumerator();
	            while (_enumerator.MoveNext())
	            {
	                // See if this "old" url (the one that cannot be found) starts with one 
	                if (url.StartsWith(_enumerator.Key.ToString(), StringComparison.InvariantCultureIgnoreCase))
	                {
	                    foundRedirect = _quickLookupTable[_enumerator.Key];
	                    CustomRedirect cr = foundRedirect as CustomRedirect;
	                    if (cr.State == (int) DataStoreHandler.State.Ignored)
	                    {
	                        return null;
	                    }
	                    if (cr.WildCardSkipAppend == true)
	                    {
	                        // We'll redirect without appending the 404 url
	                        return cr;
	                    }
	                    else
	                    {
	                        // We need to append the 404 to the end of the
	                        // new one. Make a copy of the redir object as we
	                        // are changing it.
	                        CustomRedirect redirCopy = new CustomRedirect(cr);
	                        redirCopy.NewUrl = redirCopy.NewUrl + url.Substring(_enumerator.Key.ToString().Length);
	                        return redirCopy;
	                    }
	                }
	            }
	        }
	        return null;
	    }

	    public CustomRedirect FindInProviders(string oldUrl)
        {
            // If no exact or wildcard match is found, try to parse the url through the custom providers
            if (Bvn404HandlerConfiguration.Instance.Bvn404HandlerProviders != null || Bvn404HandlerConfiguration.Instance.Bvn404HandlerProviders.Count != 0)
            {
                foreach (Bvn404HandlerProvider provider in Bvn404HandlerConfiguration.Instance.Bvn404HandlerProviders)
                {
                    Type type = (Type.GetType(provider.Type));
                    if (type != null)
                    {
                        INotFoundHandler handler = (INotFoundHandler)Activator.CreateInstance(type);
                        string newUrl = handler.RewriteUrl(oldUrl);
                        if (newUrl != null)
                            return new CustomRedirect(oldUrl, newUrl);
                    }
                }
            }
            return null;
        }
		#endregion
		#region Contains
		// TODO: If you changed the parameters to Find (above), change them here as well.
		public bool Contains(string oldUrl)
		{
			return _quickLookupTable.ContainsKey(oldUrl);
		}
		#endregion
 	
		// public properties...
		#region this[int aIndex]
		public CustomRedirect this[int index] 
		{
			get
			{
				return (CustomRedirect) List[index];
			}
			set
			{
				List[index] = value;
			}
		}
		#endregion
	}
 
}
