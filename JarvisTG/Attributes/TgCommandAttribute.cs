using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisTG.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class TgCommandAttribute : Attribute
    {
        public string Trigger { get; init; }
        public PermissionLevel PermissionLevel { get; init; }
    }
}
