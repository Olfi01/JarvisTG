using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisTG.Types
{
    [AttributeUsage(AttributeTargets.Method)]
    public class JarvisActionAttribute : Attribute
    {
        public PermissionLevel PermissionLevel { get; init; } = PermissionLevel.Inherit;
        public AllowedChatTypes AllowedChatTypes { get; init; } = AllowedChatTypes.Inherit;

        public string ActionId { get; }

        public JarvisActionAttribute(string actionId)
        {
            ActionId = actionId;
        }
    }
}
