using BVNetwork.NotFound.Core.CustomRedirects;

namespace BVNetwork.NotFound
{
    public class CustomRedirectCollection_spec : CustomRedirect_spec
    {
        void describe_CustomRedirectCollection_with_exact_match()
        {
            const bool exactMatch = true;
            const bool skipWildcardAppend = false;

            context[$"Given the redirect rule is http://mysite/test => http://mysite, exactMatch={exactMatch}"] = () =>
            {
                before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite", skipWildcardAppend, exactMatch, false) };

                it["when url is http://mysite/test then it's redirected to http://mysite"] = () =>
                {
                    WhenUrlIs("http://mysite/test").ThenItRedirectsTo("http://mysite");
                };

                it["when url is http://mysite/test/ then it's redirected to http://mysite"] = () =>
                {
                    WhenUrlIs("http://mysite/test/").ThenItRedirectsTo("http://mysite");
                };

                it["when url is http://mysite/testme then it does not redirect"] = () =>
                {
                    WhenUrlIs("http://mysite/testme").ThenItDoesNotRedirect();
                };

                it["when url is http://mysite/test/me then it does not redirect"] = () =>
                {
                    WhenUrlIs("http://mysite/testme").ThenItDoesNotRedirect();
                };

                context["and skipQueryString=false"] = () =>
                {
                    before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite", skipWildcardAppend, exactMatch, false) };

                    it["when url is http://mysite/test?query=my-query then it's redirected to http://mysite?query=my-query"] = () =>
                    {
                        WhenUrlIs("http://mysite/test?query=my-query").ThenItRedirectsTo("http://mysite?query=my-query");
                    };
                };

                context["and skipQueryString=true"] = () =>
                {
                    before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite", skipWildcardAppend, exactMatch, true) };

                    it["when url is http://mysite/test then it's redirected to http://mysite"] = () =>
                    {
                        WhenUrlIs("http://mysite/test").ThenItRedirectsTo("http://mysite");
                    };

                    it["when url is http://mysite/test?query=my-query then it's redirected to http://mysite"] = () =>
                    {
                        WhenUrlIs("http://mysite/test?query=my-query").ThenItRedirectsTo("http://mysite");
                    };
                };
            };
        }

        void describe_CustomRedirectCollection_exact_match_false_and_skip_wildcard_append_true()
        {
            const bool exactMatch = false;
            const bool skipWildcardAppend = true;

            context[
                    $"Given the redirect rule is http://mysite/test => http://mysite, exactNatch={exactMatch}, skipWildcardAppend={skipWildcardAppend}"]
                = () =>
                {
                    before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite", skipWildcardAppend, exactMatch, false) };

                    it["when url is http://mysite/test then it's redirected to http://mysite"] = () =>
                    {
                        WhenUrlIs("http://mysite/test").ThenItRedirectsTo("http://mysite");
                    };

                    it["when url is http://mysite/test/ then it's redirected to http://mysite"] = () =>
                    {
                        WhenUrlIs("http://mysite/test/").ThenItRedirectsTo("http://mysite");
                    };

                    it["when url is http://mysite/testme then it's redirected to http://mysite"] = () =>
                    {
                        WhenUrlIs("http://mysite/testme").ThenItRedirectsTo("http://mysite");
                    };

                    it["when url is http://mysite/test/me then it's redirected to http://mysite"] = () =>
                    {
                        WhenUrlIs("http://mysite/test/me").ThenItRedirectsTo("http://mysite");
                    };

                    context["and skipQueryString=false"] = () =>
                    {
                        before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite", skipWildcardAppend, exactMatch, false) };

                        it["when url is http://mysite/test?query=my-query then it's redirected to http://mysite"] = () =>
                        {
                            WhenUrlIs("http://mysite/test?query=my-query")
                                .ThenItRedirectsTo("http://mysite");
                        };
                    };

                    context["and skipQueryString=true"] = () =>
                    {
                        before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite", skipWildcardAppend, exactMatch, false) };

                        it["when url is http://mysite/test?query=my-query then it's redirected to http://mysite"] = () =>
                        {
                            WhenUrlIs("http://mysite/test?query=my-query")
                                .ThenItRedirectsTo("http://mysite");
                        };
                    };
                };
        }

        void describe_CustomRedirectCollection_exact_match_false_and_skip_wildcard_append_false()
        {
            const bool exactMatch = false;
            const bool skipWildcardAppend = false;
            context[
                    $"Given the redirect rule is http://mysite/test => http://mysite, exactNatch={exactMatch}, skipWildcardAppend={skipWildcardAppend}"]
                = () =>
                {
                    before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite", skipWildcardAppend, exactMatch, false) };

                    it["when url is http://mysite/test then it's redirected to http://mysite"] = () =>
                    {
                        WhenUrlIs("http://mysite/test").ThenItRedirectsTo("http://mysite");
                    };

                    it["when url is http://mysite/test/ then it's redirected to http://mysite"] = () =>
                    {
                        WhenUrlIs("http://mysite/test/").ThenItRedirectsTo("http://mysite");
                    };

                    it["when url is http://mysite/testme then it's redirected to http://mysiteme"] = () =>
                    {
                        WhenUrlIs("http://mysite/testme").ThenItRedirectsTo("http://mysiteme");
                    };

                    it["when url is http://mysite/test/me then it's redirected to http://mysite/me"] = () =>
                    {
                        WhenUrlIs("http://mysite/test/me").ThenItRedirectsTo("http://mysite/me");
                    };
                    
                    context["and skipQueryString=false"] = () =>
                    {
                        before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite", skipWildcardAppend, exactMatch, false) };

                        it["when url is http://mysite/test?query=my-query then it's redirected to http://mysite?query=my-query"] = () =>
                        {
                            WhenUrlIs("http://mysite/test?query=my-query")
                                .ThenItRedirectsTo("http://mysite?query=my-query");
                        };
                    };

                    context["and skipQueryString=true"] = () =>
                    {
                        before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite", skipWildcardAppend, exactMatch, false) };

                        it["when url is http://mysite/test?query=my-query then it's redirected to http://mysite?query=my-query"] = () =>
                        {
                            WhenUrlIs("http://mysite/test?query=my-query")
                                .ThenItRedirectsTo("http://mysite?query=my-query");
                        };
                    };
                };
        }

        void describe_CustomRedirectCollection_query_string_merge()
        {
            const bool exactMatch = true;
            const bool skipWildcardAppend = false;
            const bool skipQueryString = false;

            context[$"Given the redirect rule is http://mysite/test => http://mysite?a=1&b=2, exactMatch={exactMatch}, skipQueryString={skipQueryString}"] = () =>
            {
                before = () => redirects = new CustomRedirectCollection { new CustomRedirect("http://mysite/test", "http://mysite?a=1&b=2", skipWildcardAppend, exactMatch, skipQueryString) };

                it["when url is http://mysite/test?a=0&x=0 then it's redirected to http://mysite?a=1&b=2&x=0"] = () =>
                {
                    WhenUrlIs("http://mysite/test?a=0&x=0").ThenItRedirectsTo("http://mysite?a=1&b=2&x=0");
                };

                it["when url is http://mysite/test/?x=0 then it's redirected to http://mysite?a=1&b=2&x=0"] = () =>
                {
                    WhenUrlIs("http://mysite/test/?x=0").ThenItRedirectsTo("http://mysite?a=1&b=2&x=0");
                };
            };
        }

        void describe_CustomRedirectCollection_protocol_invariant_rule()
        {
            const bool exactMatch = true;
            const bool skipWildcardAppend = false;
            const bool skipQueryString = false;

            context[$"Given the redirect rule is //mysite/test => //mysite, exactMatch={exactMatch}, skipWildcardAppend={skipWildcardAppend}"] = () =>
            {
                before = () => redirects = new CustomRedirectCollection { new CustomRedirect("//mysite/test", "//mysite", skipWildcardAppend, exactMatch, skipQueryString) };

                it["when url is http://mysite/test then it's redirected to //mysite"] = () =>
                {
                    WhenUrlIs("http://mysite/test").ThenItRedirectsTo("//mysite");
                };

                it["when url is https://mysite/test then it's redirected to //mysite"] = () =>
                {
                    WhenUrlIs("https://mysite/test").ThenItRedirectsTo("//mysite");
                };
            };

            context[$"Given the redirect rule is //mysite/test => https://mysite, exactMatch={exactMatch}, skipWildcardAppend={skipWildcardAppend}"] = () =>
            {
                before = () => redirects = new CustomRedirectCollection { new CustomRedirect("//mysite/test", "https://mysite", skipWildcardAppend, exactMatch, skipQueryString) };

                it["when url is http://mysite/test then it's redirected to https://mysite"] = () =>
                {
                    WhenUrlIs("http://mysite/test").ThenItRedirectsTo("https://mysite");
                };

                it["when url is https://mysite/test then it's redirected to https://mysite"] = () =>
                {
                    WhenUrlIs("https://mysite/test").ThenItRedirectsTo("https://mysite");
                };
            };
        }

        void describe_CustomRedirectCollection_wildcard_append_works_with_rule_for_relative_url()
        {
            context["Given the redirect rule is /test => http://mysite"] = () =>
            {
                before = () => redirects = new CustomRedirectCollection { new CustomRedirect("/test", "http://mysite", false, false, false) };

                it["when url is https://mysite/test then it's redirected to http://mysite"] = () =>
                {
                    WhenUrlIs("http://mysite/test").ThenItRedirectsTo("http://mysite");
                };

                it["when url is https://mysite/testme then it's redirected to http://mysiteme"] = () =>
                {
                    WhenUrlIs("http://mysite/testme").ThenItRedirectsTo("http://mysiteme");
                };

                it["when url is https://mysite/test/me then it's redirected to http://mysite/me"] = () =>
                {
                    WhenUrlIs("http://mysite/test/me").ThenItRedirectsTo("http://mysite/me");
                };
            };
        }
    }
}