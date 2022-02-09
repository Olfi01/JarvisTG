using JarvisTG.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisTG.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TgCallbackAttribute : Attribute
    {
        public string Trigger { get; }
        public PermissionLevel PermissionLevel { get; init; }
        public TgCallbackAttribute(string trigger)
        {
            Trigger = trigger;
        }
    }
}
