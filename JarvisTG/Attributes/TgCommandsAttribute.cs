using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisTG.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class TgCommandsAttribute : Attribute
    {
        public PermissionLevel PermissionLevel { get; init; }
    }
}
