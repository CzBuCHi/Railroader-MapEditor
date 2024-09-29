using System;
using Railloader;
using UI.Builder;
using UI.Common;
using Object = UnityEngine.Object;

namespace MapEditor.Features.Abstract;

public abstract class DialogBase(IUIHelper uiHelper)
{
    #region Manage

    protected static void Show<TDialogBase>(ref TDialogBase? instance, Func<TDialogBase> createDialog, Action<TDialogBase>? updateDialog = null)
        where TDialogBase : DialogBase {
        if (instance != null) {
            updateDialog?.Invoke(instance);
            instance.Window.OrderFront();
            return;
        }

        instance = createDialog()!;
        instance.Window.ShowWindow();
    }

    protected static void Close<TDialogBase>(ref TDialogBase? instance)
        where TDialogBase : DialogBase {
        if (instance == null ||  instance._Window == null) {
            return;
        }

        instance._Window.CloseWindow();
        instance = null;
    }

    #endregion

    private Window? _Window;
    private Window  Window => _Window ??= CreateWindow();

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
        Object.Destroy(Window);
        _Window = null;
    }

    protected virtual void OnWindowClosed() {
    }

    protected abstract void BuildWindow(UIPanelBuilder builder);

    protected abstract int             WindowWidth    { get; }
    protected abstract int             WindowHeight   { get; }
    protected abstract Window.Position WindowPosition { get; }
    protected abstract string          WindowTitle    { get; }
}
