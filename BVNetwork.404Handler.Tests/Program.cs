using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSpec;
using NSpec.Assertions;
using NSpec.Domain;
using NSpec.Domain.Formatters;

namespace BVNetwork.NotFound
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var tagOrClassName = "CustomRedirects";

                var types = typeof(Program).GetTypeInfo().Assembly.GetTypes();
                // OR
                // var types = new Type[]{typeof(Some_Type_Containg_some_Specs)};

                var finder = new SpecFinder(types, "");

                var tagsFilter = new Tags().Parse(tagOrClassName);

                var builder = new ContextBuilder(finder, tagsFilter, new DefaultConventions());

                var runner = new ContextRunner(tagsFilter, new ConsoleFormatter(), false);

                var results = runner.Run(builder.Contexts().Build());

                //assert that there aren't any failures
                (results.Failures().Count() == 0).ShouldBeTrue();

                return 0;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
                return 1;
            }
        }
    }
}
