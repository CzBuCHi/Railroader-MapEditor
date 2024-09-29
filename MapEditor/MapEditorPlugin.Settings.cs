namespace MapEditor;

public sealed partial class MapEditorPlugin
{
    private static MapEditorSettings? _Settings;
    public static MapEditorSettings Settings => _Settings ??= LoadSettings();

    private static MapEditorSettings LoadSettings() {
        return ModdingContext.LoadSettingsData<MapEditorSettings>(PluginIdentifier) ?? new MapEditorSettings();
    }

    public static void UpdateSettings(UpdateDelegate<MapEditorSettings> action) {
        _Settings = action(Settings);
    }

    public static void SaveSettings(MapEditorSettings settings) {
        ModdingContext.SaveSettingsData(PluginIdentifier, settings);
        _Settings = settings;
    }
}
