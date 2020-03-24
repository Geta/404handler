using System;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.PlugIn;

namespace Geta._404Handler.SandboxApp.Models.Properties
{
    /// <summary>
    /// Property type for storing a list of strings
    /// </summary>
    /// <remarks>For an example, see <see cref="Geta._404Handler.SandboxApp.Models.Pages.SitePageData"/> where this property type is used for the MetaKeywords property</remarks>
    [EditorHint(Global.SiteUIHints.Strings)]
    [PropertyDefinitionTypePlugIn(Description = "A property for list of strings", DisplayName = "String List")]
    public class PropertyStringList : PropertyLongString
    {
        protected String Separator = "\n";

        public String[] List
        {
            get
            {
                return (String[])Value;
            }
        }

        public override Type PropertyValueType
        {
            get
            {
                return typeof(String[]);
            }
        }

        public override object SaveData(PropertyDataCollection properties)
        {
            return LongString;
        }

        public override object Value
        {
            get
            {
                var value = base.Value as string;

                if (value == null)
                {
                    return null;
                }

                return value.Split(Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                if (value is String[])
                {
                    var s = String.Join(Separator, value as String[]);
                    base.Value = s;
                }
                else
                {
                    base.Value = value;
                }

            }
        }
    }
}
