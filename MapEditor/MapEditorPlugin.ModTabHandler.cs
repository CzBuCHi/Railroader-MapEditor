using MapEditor.Features.Milestones;
using MapEditor.Features.SceneView;
using MapEditor.Features.Settings;
using Railloader;
using UI.Builder;

namespace MapEditor;

public sealed partial class MapEditorPlugin : IModTabHandler
{
    public void ModTabDidOpen(UIPanelBuilder builder) {
        builder.AddButton("Map Editor", OpenEditorDialog);
        builder.AddButton("Milestone manager", () => MilestonesDialog.Show(_UiHelper));
        builder.AddButton("Settings", () => SettingsDialog.Show(_UiHelper));
        builder.AddButton("Scene View", () => SceneViewDialog.Show(_UiHelper));
#if DEBUG
        builder.AddButton("Testing", Testing.Execute);
#endif
    }

    public void ModTabDidClose() {
    }
}
