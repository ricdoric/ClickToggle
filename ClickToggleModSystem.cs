using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ClickToggle;

public class ClickToggleModSystem : ModSystem
{
    private ICoreClientAPI Api;
    private bool autoClickEnabled = false;
    private bool isRightClick = false;
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

        Api.Input.RegisterHotKey(
            hotkeyCode: "autoClickToggleRight",
            name: "(ClickToggle) Toggle auto-right-click",
            key: GlKeys.N,
            type: HotkeyType.CharacterControls,
            shiftPressed: true
        );

        Api.Input.SetHotKeyHandler("autoClickToggle", OnHotkeyClickToggleLeft);
        Api.Input.SetHotKeyHandler("autoClickToggleRight", OnHotkeyClickToggleRight);

        Api.Event.RegisterGameTickListener(OnGameTick, 0);

        Api.ChatCommands.Create("clicktoggle")
            .WithDescription("ClickToggle mod settings")
            .BeginSubCommand("debug")
                .WithDescription("Toggle debug messages")
                .HandleWith(OnDebugCommand)
            .EndSubCommand();

        // Cancel auto clicking if user manually clicks mouseleft or mouseright
        Api.Event.MouseDown += OnMouseDown;
        Api.Event.KeyDown += OnKeyDown;
    }

    private bool OnHotkeyClickToggleLeft(KeyCombination kc)
    {
        autoClickEnabled = !autoClickEnabled;
        isRightClick = false;

        if (autoClickEnabled)
        {
            DebugMessage("ClickToggle left click enabled via hotkey.");
        }
        else
        {
            // Ensure mouse button is released when disabling
            ReleaseMouseButton();
            DebugMessage("ClickToggle left click disabled via hotkey.");
        }

        return true;
    }

    private bool OnHotkeyClickToggleRight(KeyCombination kc)
    {
        autoClickEnabled = !autoClickEnabled;
        isRightClick = true;

        if (autoClickEnabled)
        {
            DebugMessage("ClickToggle right click enabled via hotkey.");
        }
        else
        {
            // Ensure mouse button is released when disabling
            ReleaseMouseButton();
            DebugMessage("ClickToggle right click disabled via hotkey.");
        }

        return true;
    }

    private void OnMouseDown(MouseEvent e)
    {
        if (!autoClickEnabled) return;
        if (e.Button == EnumMouseButton.Left || e.Button == EnumMouseButton.Right)
        {
            autoClickEnabled = false;
            isRightClick = false;
            ReleaseMouseButton();
            DebugMessage("ClickToggle disabled via manual mouse click.");
        }
    }

    private void OnKeyDown(KeyEvent e)
    {
        if (!autoClickEnabled) return;
        if (e.KeyCode == (int)GlKeys.Escape)
        {
            autoClickEnabled = false;
            isRightClick = false;
            ReleaseMouseButton();
            DebugMessage("ClickToggle disabled via escape key.");
        }
    }

    private void OnGameTick(float dt)
    {
        if (!autoClickEnabled) return;

        // Emulate holding down left or right mouse button by triggering the mouse button down event each tick
        if (isRightClick)
        {
            Api.Input.InWorldMouseButton.Right = true;
        }
        else
        {
            Api.Input.InWorldMouseButton.Left = true;
        }
    }

    private void ReleaseMouseButton()
    {
        if (Api?.Input?.InWorldMouseButton != null)
        {
            Api.Input.InWorldMouseButton.Left = false;
            Api.Input.InWorldMouseButton.Right = false;
            isRightClick = false;
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
        {
            Api.Event.MouseDown -= OnMouseDown;
            Api.Event.KeyDown -= OnKeyDown;
        }

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
