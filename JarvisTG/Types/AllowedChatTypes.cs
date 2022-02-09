using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisTG.Types
{
    [Flags]
    public enum AllowedChatTypes
    {
        Inherit = 0,
        Private = 1,
        Sender = 2,
        Channel = 4,
        Group = 8,
        Supergroup = 16,
        AnyGroup = Group | Supergroup,
        PrivateOrAnyGroup = Private | AnyGroup,
        Any = Private | Sender | Channel | Group | Supergroup
    }
}
