using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;
#if USE_RESONITE_HOT_RELOAD_LIB
using ResoniteHotReloadLib;
#endif

namespace SpatialVariableGizmo;

/// <summary>Entry point for the mod.</summary>
public class SpatialVariableGizmoMod : ResoniteMod
{
    private static readonly Assembly Assembly = typeof(SpatialVariableGizmoMod).Assembly;
    private static readonly string HarmonyId = $"com.nekometer.esnya.{Assembly.GetName().Name}";
    private static readonly Harmony Harmony = new(HarmonyId);

    /// <inheritdoc />
    public override string Name =>
        Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

    /// <inheritdoc />
    public override string Author =>
        Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;

    /// <inheritdoc />
    public override string Version =>
        (
            Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? string.Empty
        ).Split('+')[0];

    /// <inheritdoc />
    public override string Link =>
        Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(meta => meta.Key == "RepositoryUrl")
            ?.Value ?? string.Empty;

    /// <inheritdoc />
    public override void OnEngineInit()
    {
        InitializeMod(this);
    }

#if USE_RESONITE_HOT_RELOAD_LIB
    /// <summary>Removes Harmony patches before hot reload.</summary>
    public static void BeforeHotReload()
    {
        Harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>Reapplies Harmony patches after hot reload.</summary>
    /// <param name="mod">The reloaded mod.</param>
    public static void OnHotReload(ResoniteMod mod)
    {
        InitializeMod(mod);
    }
#endif

    private static void InitializeMod(ResoniteMod mod)
    {
        ArgumentNullException.ThrowIfNull(mod);
#if USE_RESONITE_HOT_RELOAD_LIB
        HotReloader.RegisterForHotReload(mod);
#endif
        Harmony.PatchAll();
    }
}
