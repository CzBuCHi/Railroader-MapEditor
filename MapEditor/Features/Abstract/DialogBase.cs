using System;
using Railloader;
using Serilog;
using UI.Builder;
using UI.Common;
using Object = UnityEngine.Object;

namespace MapEditor.Features.Abstract;

public abstract class DialogBase(IUIHelper uiHelper)
{
    #region Manage

    protected static void Show<TDialogBase>(ref TDialogBase? instance, Func<TDialogBase> createDialog, Action<TDialogBase>? updateDialog = null)
        where TDialogBase : DialogBase {

        Log.Information("Dialog:Show [" + typeof(TDialogBase).Name + "]");
        lock (_Lock) {
            if (instance != null) {
                Log.Information("instance != null");
                updateDialog?.Invoke(instance);
                instance.Window.ShowWindow();
                instance.Window.OrderFront();
                return;
            }

            instance = createDialog()!;
            instance.Window.ShowWindow();
        }
    }

    protected static void Close<TDialogBase>(ref TDialogBase? instance)
        where TDialogBase : DialogBase {
        Log.Information("Dialog:Close [" + typeof(TDialogBase).Name + "]");
        lock (_Lock) {
            if (instance == null) {
                return;
            }

            Log.Information("CloseWindow");
            instance._Window!.CloseWindow();
            Object.Destroy(instance._Window);
            instance = null;
        }
    }

    #endregion

    private static readonly object  _Lock = new();
    private                 Window? _Window;
    private                 Window  Window => _Window ??= CreateWindow();

    private Window CreateWindow() {
        var window = uiHelper.CreateWindow(WindowWidth, WindowHeight, WindowPosition);
        window.Title = WindowTitle;
        ConfigureWindow(window);
        window.OnShownDidChange += WindowOnOnShownDidChange;
        uiHelper.PopulateWindow(window, BuildWindow);
        return window;
    }

    protected virtual void ConfigureWindow(Window window) {
    }

    private void WindowOnOnShownDidChange(bool isShown) {
        if (isShown) {
            return;
        }

        OnWindowClosed();
    }

    protected virtual void OnWindowClosed() {
    }

    protected abstract void BuildWindow(UIPanelBuilder builder);

    protected abstract int             WindowWidth    { get; }
    protected abstract int             WindowHeight   { get; }
    protected abstract Window.Position WindowPosition { get; }
    protected abstract string          WindowTitle    { get; }
}
