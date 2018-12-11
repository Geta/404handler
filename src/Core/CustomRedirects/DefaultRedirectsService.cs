using System;
using System.Collections.Generic;
using System.Linq;
using BVNetwork.NotFound.Core.Data;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public class DefaultRedirectsService : IRedirectsService
    {
        private readonly IRepository<CustomRedirect> _repository;
        private readonly IRedirectLoader _redirectLoader;

        public DefaultRedirectsService(
            IRepository<CustomRedirect> repository, IRedirectLoader redirectLoader)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _redirectLoader = redirectLoader ?? throw new ArgumentNullException(nameof(redirectLoader));
        }

        public IEnumerable<CustomRedirect> GetAll()
        {
            return _redirectLoader.GetAll();
        }

        public IEnumerable<CustomRedirect> GetAllExcludingIgnored()
        {
            return _redirectLoader.GetByState(RedirectState.Saved);
        }

        public IEnumerable<CustomRedirect> GetIgnored()
        {
            return _redirectLoader.GetByState(RedirectState.Ignored);
        }

        public IEnumerable<CustomRedirect> GetDeleted()
        {
            return _redirectLoader.GetByState(RedirectState.Deleted);
        }

        public IEnumerable<CustomRedirect> Search(string searchText)
        {
            return _redirectLoader.Find(searchText);
        }

        public void AddOrUpdate(CustomRedirect redirect)
        {
            var match = _redirectLoader.GetByOldUrl(redirect.OldUrl);

            //if there is a match, replace the value.
            if (match != null)
            {
                redirect.Id = match.Id;
            }
            _repository.Save(redirect);
        }

        public void AddOrUpdate(IEnumerable<CustomRedirect> redirects)
        {
            foreach (var redirect in redirects)
            {
                AddOrUpdate(redirect);
            }
        }

        public void DeleteByOldUrl(string oldUrl)
        {
            var match = _redirectLoader.GetByOldUrl(oldUrl);
            if (match != null)
            {
                _repository.Delete(match);
            }
        }

        public int DeleteAll()
        {
            // In order to avoid a database timeout, we delete the items one by one.
            var redirects = GetAll().ToList();
            foreach (var redirect in redirects)
            {
                _repository.Delete(redirect);
            }
            return redirects.Count;
        }

        public int DeleteAllIgnored()
        {
            // In order to avoid a database timeout, we delete the items one by one.
            var ignoredRedirects = GetIgnored().ToList();
            foreach (var redirect in ignoredRedirects)
            {
                _repository.Delete(redirect);
            }
            return ignoredRedirects.Count;
        }
    }
}