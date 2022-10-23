namespace GoodFriend.Base
{
    using System;
    using GoodFriend.Utils;
    using GoodFriend.Enums;
    using Dalamud.Configuration;

    [Serializable]
    public sealed class Configuration : IPluginConfiguration
    {
        /// <summary>
        ///     The current configuration version, incremented on breaking changes.
        /// </summary>
        public int Version { get; set; } = 0;

        /// <summary>
        ///     The notification type to use for friend notifications.
        /// </summary>
        public NotificationType NotificationType { get; set; } = NotificationType.Chat;

        /// <summary>
        ///     The message to display when a friend logs in.
        /// </summary>
        public string FriendLoggedInMessage { get; set; } = "{0} has logged in.";

        /// <summary>
        ///     The message to display when a friend logs out.
        /// </summary>
        public string FriendLoggedOutMessage { get; set; } = "{0} has logged out.";

        /// <summary> 
        ///     Whether or not to hide notifications for the same free company.
        /// </summary>
        public bool HideSameFC { get; set; } = true;

        /// <summary>
        ///     Whether or not to hide notifications from users in different homeworlds.
        /// </summary>
        public bool HideDifferentHomeworld { get; set; } = false;

        /// <summary>
        ///     Whether or not to hide notifications from users in different territories.
        /// </summary>
        public bool HideDifferentTerritory { get; set; } = false;

        /// <summary>
        ///     Whether or not to show API events as notifications.
        /// </summary>
        public bool ShowAPIEvents { get; set; } = false;

        /// <summary> 
        ///     The extra salt to apply when hashing the players ContentID, used to create "closed"  notification groups.
        /// </summary>
        public string FriendshipCode { get; set; } = "";

        /// <summary>
        ///     The salt method to use when hashing.
        ///     <see cref="SaltMethods.Relaxed"/>: No extra salt, just the <see cref="FriendshipCode"/>.
        ///     <see cref="SaltMethods.Strict"/>: The <see cref="FriendshipCode"/> and the plugin assembly ModuleVersionId.
        /// </summary>
        public SaltMethods SaltMethod { get; set; } = SaltMethods.Strict;

        /// <summary>
        ///     The BaseURL to use when interacting with the API.
        /// </summary>
        public Uri APIUrl { get; set; } = PStrings.defaultAPIUrl;

        /// <summary>
        ///     The authentication attached to all requests to the API.
        /// </summary>
        public string APIAuthentication { get; set; } = string.Empty;

        /// <summary>
        ///     Saves the current configuration to disk.
        /// </summary>
        internal void Save() => PluginService.PluginInterface.SavePluginConfig(this);

        /// <summary>
        ///     Sets the configuration to the default values.
        /// </summary>
        internal void SetDefault() => PluginService.PluginInterface.SavePluginConfig(new Configuration());
    }
}