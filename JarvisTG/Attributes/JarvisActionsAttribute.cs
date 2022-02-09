using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisTG.Types
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class JarvisActionsAttribute : Attribute
    {
        public PermissionLevel PermissionLevel { get; init; } = PermissionLevel.Any;
        public AllowedChatTypes AllowedChatTypes { get; init; } = AllowedChatTypes.Any;
    }
}
