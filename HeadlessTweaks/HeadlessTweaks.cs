using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader;

using SkyFrost.Base;

namespace HeadlessTweaks
{
    public class HeadlessTweaks : ResoniteMod
    {
        public override string Name => "HeadlessTweaks";
        public override string Author => "New_Project_Final_Final_WIP";
        public override string Version => "2.2.0";
        public override string Link =>
            "https://github.com/New-Project-Final-Final-WIP/HeadlessTweaks";

        public static bool isDiscordLoaded = false;

        public static ModConfiguration config;

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<List<string>> AutoInviteOptOutList = new(
            "AutoInviteOptOut",
            "Auto Invite Opt Out",
            () => [],
            internalAccessOnly: true
        );

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<
            Dictionary<string, PermissionLevel>
        > PermissionLevels = new("PermissionLevels", "Permission Levels", () => []);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<
            Dictionary<string, Dictionary<string, PermissionLevel>>
        > WorldScopedPermissions = new(
            "WorldScopedPermissions",
            "World Scoped Permissions",
            () => []
        );

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<Dictionary<string, string>> WorldRoster = new(
            "WorldRoster",
            "World Roster",
            () => []
        );

        // Rename sessions in webhook
        // SessionIds to Name
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<Dictionary<string, string>> SessionIdToName =
            new("SessionIdToName", "SessionIdToName", () => []);

        // Default session access level for new sessions
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<SessionAccessLevel> DefaultSessionAccessLevel =
            new(
                "DefaultSessionAccessLevel",
                "Default Session Access Level",
                () => SessionAccessLevel.ContactsPlus
            );

        // Default session hidden status for new sessions
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> DefaultSessionHidden = new(
            "DefaultSessionHidden",
            "Default Session Hidden",
            () => true
        );

        // secondary alpha for lists in messages float
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<float?> AlternateListAlpha = new(
            "AlternateListAlpha",
            "Alternate List Alpha",
            () => 0.4f
        );

        // Disable autosave if no one is in world
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> SmartAutosaveEnabled = new(
            "SmartAutosave",
            "Disable autosave if there are no users in current world",
            () => false
        );

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> AutoHandleInviteRequests = new(
            "AutoHandleInviteRequests",
            "Allow headless tweaks automatically handle direct invite requests based on the same rules as reqInvite",
            () => true
        );

        /// <summary>
        /// Disables the interactive commandline.
        /// </summary>
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> DisableInteractivePrompt = new(
            "DisableInteractivePrompt",
            "Disables interactive console command behavior. Useful if you have no direct stdin access to your executable (i.e. systemd/Windows service). Requires a restart to take effect",
            () => false
        );

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            // Initialize default values

            config.Save(true);
            Harmony harmony = new("page.newweb.HeadlessTweaks");

            // If we are not loaded by a headless client skip the rest
            if (!ModLoader.IsHeadless)
            {
                Warn("Headless Not Detected! Skipping headless specific modules");
                return;
            }

            NewHeadlessCommands.Init(harmony);
            MessageCommands.Init();
            AutoInviteOptOut.Init(harmony);
            SmartAutosave.Init(harmony);

            if (config.GetValue(DisableInteractivePrompt))
                DisableInteractiveCommandLine.Init(harmony);
            else
                Debug("Not applying non-interactive command line patch");

            Engine.Current.RunPostInit(() => SystemdSend("READY=1"));
        }

        public static void SystemdSend(string text)
        {
            string socketPath = Environment.GetEnvironmentVariable("NOTIFY_SOCKET");
            if (string.IsNullOrEmpty(socketPath)) return;

            // Abstract namespace sockets start with '@'
            if (socketPath.StartsWith("@"))
            {
                socketPath = "\0" + socketPath.Substring(1);
            }

            try
            {
                var endPoint = new UnixDomainSocketEndPoint(socketPath);
                using (var client = new Socket(AddressFamily.Unix, SocketType.Dgram, ProtocolType.Unspecified))
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(text);
                    client.SendTo(buffer, endPoint);
                }
            }
            catch (Exception ex)
            {
                Error($"Failed to send sd_notify message \"{text}\": {ex}");
            }
        }
    }
}
