﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisTG.Types
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TgCommandAttribute : Attribute
    {
        public string Trigger { get; }
        public PermissionLevel PermissionLevel { get; init; } = PermissionLevel.Inherit;
        public AllowedChatTypes AllowedChatTypes { get; init; } = AllowedChatTypes.Inherit;

        public TgCommandAttribute(string trigger)
        {
            Trigger = trigger;
        }
    }
}
