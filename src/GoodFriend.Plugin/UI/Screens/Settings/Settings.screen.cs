namespace GoodFriend.UI.Screens.Settings;

using System;
using System.Numerics;
using ImGuiNET;
using GoodFriend.Base;
using GoodFriend.Utils;
using GoodFriend.Interfaces;
using GoodFriend.UI.Components;
using GoodFriend.Managers;

sealed public class SettingsScreen : IScreen
{
    /// <summary> The associated presenter for this screen. </summary>
    public SettingsPresenter presenter = new SettingsPresenter();

    public void Draw() { if (!this.presenter.isVisible) return; this.DrawRoot(); }
    public void Show() => presenter.isVisible = true;
    public void Hide() => presenter.isVisible = false;
    public void Dispose() => this.presenter.Dispose();


    /////////////////////////////
    ///      Core Windows     ///
    /////////////////////////////

    /// <summary> Draws all elements associated with the root of the screen. </summary>
    private void DrawRoot()
    {
        ImGui.SetNextWindowSize(new Vector2(600, 350), ImGuiCond.FirstUseEver);
        if (ImGui.Begin(PStrings.pluginName, ref this.presenter.isVisible, ImGuiWindowFlags.NoResize))
        {
            if (ImGui.BeginTabBar("##TabBar"))
            {

                this.DrawSettingsTab();
                this.DrawConnectionTab();
#if DEBUG
                this.DrawDebugWindow();
#endif
                ImGui.EndTabBar();
            }

            ImGui.End();
        }
    }


    /// <summary> Draws the configuration child tab. </summary>
    private void DrawSettingsTab()
    {
        if (ImGui.BeginTabItem(TStrings.SettingsTabSettings()))
        {
            this.DrawNormSettings();
            if (this._showAdvanced) this.DrawAdvancedSettings();
            ImGui.EndTabItem();
        }
    }


    /// <summary> Draws the connection child tab. </summary>
    private void DrawConnectionTab()
    {
        if (ImGui.BeginTabItem(TStrings.SettingsTabConnection()))
        {
            ImGui.BeginChild("##ConnectionTab");

            // Player is connected to the API
            if (PluginService.APIClientManager.APIClient.IsConnected)
            {
                Colours.TextWrappedColoured(Colours.Success, TStrings.SettingsAPIConnected());
                ImGui.TextWrapped(TStrings.SettingsAPIConnectedDesc());
                ImGui.Dummy(new Vector2(0, 5));
                if (ImGui.Button(TStrings.SettingsSupportText())) Common.OpenLink(PStrings.supportButtonUrl);

                ImGui.Dummy(new Vector2(0, 15));
                this.DrawEventTable();
            }

            // Player is not logged in & not connected
            else if (!PluginService.ClientState.IsLoggedIn) ImGui.TextWrapped(TStrings.SettingsAPINotLoggedIn());

            // Player is logged in & not connected
            else
            {
                Colours.TextWrappedColoured(Colours.Error, TStrings.SettingsAPIDisconnected());
                ImGui.TextWrapped(TStrings.SettingsAPIDisconnectedDesc());
                ImGui.BeginDisabled(this.presenter.ReconnectButtonDisabled);
                ImGui.Dummy(new Vector2(0, 5));
                if (ImGui.Button(TStrings.SettingsAPITryReconnect())) this.presenter.ReconnectWithCooldown();
                ImGui.EndDisabled();
            }

            ImGui.EndChild();
            ImGui.EndTabItem();
        }
    }


#if DEBUG
    /// <summary> Draws the debug child window </summary>
    private unsafe void DrawDebugWindow()
    {
        if (ImGui.BeginTabItem(TStrings.SettingsTabDebug()))
        {
            ImGui.BeginChild("##DebugTab");

            // Draw the dialog manage & Export Localization Button for CheapLOC.
            this.presenter.dialogManager.Draw();
            if (ImGui.Button("Export Localizable")) this.presenter.dialogManager.OpenFolderDialog("Export LOC", this.presenter.OnDirectoryPicked);

            // Draw a list of every friend and their ID for API Debugging.
            ImGui.TextDisabled("Detected Friends - Click to Copy Hash");
            ImGui.Separator();
            ImGui.BeginChild("##Friends");

            foreach (var friend in FriendList.Get())
                if (ImGui.Selectable(friend->Name.ToString())) ImGui.SetClipboardText(Hashing.HashSHA512(friend->ContentId.ToString()));

            ImGui.EndChild();
            ImGui.EndChild();
        }
    }
#endif


    /////////////////////////////
    ///     Sub-Components    ///
    /////////////////////////////

    /// <summary> Draws the event log table </summary>
    private void DrawEventTable()
    {
        try
        {
            ImGui.TextDisabled(TStrings.SettingsAPILogTitle());
            ImGui.Separator();
            ImGui.BeginChild("##EventLogTable");
            ImGui.BeginTable("##EventLogTable", 3);
            ImGui.TableSetupColumn(TStrings.SettingsAPILogTime());
            ImGui.TableSetupColumn(TStrings.SettingsAPILogPlayer());
            ImGui.TableSetupColumn(TStrings.SettingsAPILogEvent());
            ImGui.TableHeadersRow();

            foreach (var entry in this.presenter.FetchAPILog())
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text(entry.Time.ToString("HH:mm:ss"));
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(entry.Friend.Name.ToString());
                ImGui.TableSetColumnIndex(2);
                ImGui.Text(entry?.Event?.ToString() ?? "?");
            }

            ImGui.EndTable();
            ImGui.EndChild();
        }
        catch { }
    }


    /// <summary> Should advanced settings be drawn? </summary>
    private bool _showAdvanced = false;


    /// <summary> Draws the normal settings </summary>
    private void DrawNormSettings()
    {
        var showAdvanced = this._showAdvanced;
        var notificationType = Enum.GetName(typeof(NotificationType), PluginService.Configuration.NotificationType);
        var loginMessage = PluginService.Configuration.FriendLoggedInMessage;
        var logoutMessage = PluginService.Configuration.FriendLoggedOutMessage;
        var hideSameFC = PluginService.Configuration.HideSameFC;
        var friendshipCode = PluginService.Configuration.FriendshipCode;

        // Hide FC Members dropdown.
        if (ImGui.BeginCombo(TStrings.SettingsHideSameFC(), hideSameFC ? TStrings.SettingsHideSameFCEnabled() : TStrings.SettingsHideSameFCDisabled()))
        {
            if (ImGui.Selectable(TStrings.SettingsHideSameFCEnabled(), hideSameFC))
            {
                PluginService.Configuration.HideSameFC = true;
            }
            if (ImGui.Selectable(TStrings.SettingsHideSameFCDisabled(), !hideSameFC))
            {
                PluginService.Configuration.HideSameFC = false;
            }

            PluginService.Configuration.Save();
            ImGui.EndCombo();
        }
        Tooltips.Questionmark(TStrings.SettingsHideSameTCTooltip());


        // Notification type dropdown
        if (ImGui.BeginCombo(TStrings.SettingsNotificationType(), notificationType))
        {
            foreach (var notification in Enum.GetNames(typeof(NotificationType)))
            {
                if (ImGui.Selectable(notification, notification == notificationType))
                {
                    PluginService.Configuration.NotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), notification);
                    PluginService.Configuration.Save();
                }
            }
            ImGui.EndCombo();
        }
        Tooltips.Questionmark(TStrings.SettingsNotificationTypeTooltip());


        // Login message input
        if (ImGui.InputText(TStrings.SettingsLoginMessage(), ref loginMessage, 64))
        {
            bool error = false;
            try { string.Format(loginMessage, "test"); }
            catch { error = true; }

            if (!error && loginMessage.Contains("{0}"))
            {
                PluginService.Configuration.FriendLoggedInMessage = loginMessage.Trim();
                PluginService.Configuration.Save();
            }
        }
        Tooltips.Questionmark(TStrings.SettingsLoginMessageTooltip());


        // Logout message input
        if (ImGui.InputText(TStrings.SettingsLogoutMessage(), ref logoutMessage, 64))
        {
            bool error = false;
            try { string.Format(logoutMessage, "test"); }
            catch { error = true; }

            if (!error && logoutMessage.Contains("{0}"))
            {
                PluginService.Configuration.FriendLoggedOutMessage = logoutMessage.Trim();
                PluginService.Configuration.Save();
            }
        }
        Tooltips.Questionmark(TStrings.SettingsLogoutMessageTooltip());


        // Secret code input
        if (ImGui.InputTextWithHint(TStrings.SettingsFriendshipCode(), TStrings.SettingsFriendshipCodeHint(), ref friendshipCode, 64))
        {
            PluginService.Configuration.FriendshipCode = friendshipCode;
            PluginService.Configuration.Save();
        }
        Tooltips.Questionmark(TStrings.SettingsFriendshipCodeTooltip());


        // Advanced settings
        ImGui.NewLine();
        if (ImGui.Checkbox(TStrings.SettingsShowAdvanced(), ref showAdvanced)) this._showAdvanced = showAdvanced;
    }


    /// <summary> Draws the advanced settings </summary>
    private void DrawAdvancedSettings()
    {
        var APIUrl = PluginService.Configuration.APIUrl.ToString();
        var saltMethod = PluginService.Configuration.SaltMethod;

        // Salt Mode dropdown
        if (ImGui.BeginCombo(TStrings.SettingsSaltMode(), saltMethod.ToString()))
        {
            foreach (var method in Enum.GetValues(typeof(SaltMethods)))
            {
                if (ImGui.Selectable(method.ToString(), saltMethod == (SaltMethods)method))
                {
                    PluginService.Configuration.SaltMethod = (SaltMethods)method;
                }
            }

            PluginService.Configuration.Save();
            ImGui.EndCombo();
        }
        Tooltips.Questionmark(TStrings.SettingsSaltModeTooltip());


        // API URL input
        if (ImGui.InputText(TStrings.SettingsAPIURL(), ref APIUrl, 64))
        {
            // Validate the URL and save it if it is valid.
            bool error = false;
            try { new Uri(APIUrl); }
            catch { error = true; }

            if (!error) { PluginService.Configuration.APIUrl = new Uri(APIUrl); PluginService.Configuration.Save(); }
            else PluginService.Configuration.ResetApiUrl();
        }
        Tooltips.Questionmark(TStrings.SettingsAPIURLTooltip());
    }
}