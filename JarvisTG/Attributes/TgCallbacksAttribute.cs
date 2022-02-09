using JarvisTG.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisTG.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TgCallbacksAttribute : Attribute
    {
        public PermissionLevel PermissionLevel { get; init; }
    }
}
