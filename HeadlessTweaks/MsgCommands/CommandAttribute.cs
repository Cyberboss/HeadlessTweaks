using System;

namespace HeadlessTweaks
{
    public partial class MessageCommands
    {
        // Command Attributes
        // Name: The name of the command
        // Description: The description of the command
        // Category: The category of the command
        // PermissionLevel: The permission level required to use the command
        // Alliases: The aliases of the command
        // Usage: The arguments of the command
        // WorldScoped: If world scoped perms are used. Execution will be granted based on the users max permission level across worlds. Method MUST check CheckWorldPermission for the target worlds for the user before actioning itself.

        [AttributeUsage(AttributeTargets.Method)]
        public class CommandAttribute(
            string name,
            string description,
            string category,
            PermissionLevel permissionLevel = PermissionLevel.None,
            string[] aliases = null,
            string usage = null,
            bool worldScoped = false
        ) : Attribute
        {
            public string Name { get; set; } = name;
            public string Description { get; set; } = description;
            public string Category { get; set; } = category;
            public PermissionLevel PermissionLevel { get; set; } = permissionLevel;
            public string[] Aliases { get; set; } = aliases ?? [];
            public string Usage { get; set; } = usage;
            public bool WorldScoped { get; set; } = worldScoped;
        }
    }
}
