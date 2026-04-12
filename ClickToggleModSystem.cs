using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ClickToggle;

public class ClickToggleModSystem : ModSystem
{
    private ICoreClientAPI Api;
    private bool autoClickEnabled = false;
    private static bool _debug = false;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI api)
    {
        Api = api;

        // Register keybind: default key N, customizable in controls settings
        Api.Input.RegisterHotKey(
            hotkeyCode: "autoClickToggle",
            name: "(ClickToggle) Toggle auto-left-click",
            key: GlKeys.N,
            type: HotkeyType.CharacterControls
        );

        Api.Input.SetHotKeyHandler("autoClickToggle", OnToggleAutoClick);

        Api.Event.RegisterGameTickListener(OnGameTick, 0);

        Api.ChatCommands.Create("clicktoggle")
            .WithDescription("ClickToggle mod settings")
            .BeginSubCommand("debug")
                .WithDescription("Toggle debug messages")
                .HandleWith(OnDebugCommand)
            .EndSubCommand();

        // Cancel auto clicking if user manually clicks mouseleft
        Api.Event.MouseDown += OnMouseDown;
    }

    private bool OnToggleAutoClick(KeyCombination kc)
    {
        autoClickEnabled = !autoClickEnabled;

        if (autoClickEnabled)
        {
            DebugMessage("ClickToggle enabled via hotkey.");
        }
        else
        {
            // Ensure mouse button is released when disabling
            ReleaseMouseButton();
            DebugMessage("ClickToggle disabled via hotkey.");
        }

        return true;
    }

    private void OnMouseDown(MouseEvent e)
    {
        if (!autoClickEnabled) return;
        if (e.Button == EnumMouseButton.Left)
        {
            autoClickEnabled = false;
            ReleaseMouseButton();
            DebugMessage("ClickToggle disabled via manual left-click.");
        }
    }

    private void OnGameTick(float dt)
    {
        if (!autoClickEnabled) return;

        // Emulate holding down left mouse button by triggering the mouse button down event each tick
        Api.Input.InWorldMouseButton.Left = true;
    }

    private void ReleaseMouseButton()
    {
        if (Api?.Input?.InWorldMouseButton != null)
        {
            Api.Input.InWorldMouseButton.Left = false;
        }
    }

    public override void Dispose()
    {
        if (autoClickEnabled)
        {
            ReleaseMouseButton();
            autoClickEnabled = false;
        }

        if (Api != null)
            Api.Event.MouseDown -= OnMouseDown;

        base.Dispose();
    }

    private TextCommandResult OnDebugCommand(TextCommandCallingArgs args)
    {
        _debug = !_debug;
        return TextCommandResult.Success($"ClickToggle debug messages: {(_debug ? "ON" : "OFF")}");
    }

    private void DebugMessage(string msg)
    {
        if (_debug) Api.ShowChatMessage(msg);
    }
}
