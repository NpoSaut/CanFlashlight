using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFramesCodegen.PropertyDescriptions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PropertyTypeNameAttribute : Attribute
    {
        public HashSet<String> TypeNames { get; private set; }

        public PropertyTypeNameAttribute(params string[] TypeNames)
        {
            this.TypeNames = new HashSet<string>(TypeNames);
        }
        public PropertyTypeNameAttribute(String TypeName)
        {
            this.TypeNames = new HashSet<string>() { TypeName };
        }

        public static PropertyTypeNameAttribute Get(Type t)
        {
            return t.GetCustomAttributes(typeof(PropertyTypeNameAttribute), false).OfType<PropertyTypeNameAttribute>().FirstOrDefault();
        }
    }
}
