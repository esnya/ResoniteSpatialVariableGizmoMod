using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using colorX = Elements.Core.colorX;
using float3 = Elements.Core.float3;
using floatQ = Elements.Core.floatQ;

namespace SpatialVariableGizmo;

[HarmonyPatch(typeof(WorkerInspector), "BuildUIForComponent")]
internal static class SpatialVariableInspectorPatch
{
    private static readonly MethodInfo PanelMethod = AccessTools.Method(typeof(UIBuilder), nameof(UIBuilder.Panel));
    private static readonly MethodInfo InjectMethod =
        AccessTools.Method(typeof(SpatialVariableInspectorPatch), nameof(AddVisualizationButton));
#pragma warning disable IDE0028 // Analyzer misfires on ConditionalWeakTable initialization
    private static ConditionalWeakTable<Button, Worker> ButtonTargets { get; } = new();
#pragma warning restore IDE0028

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool injected = false;

        foreach (CodeInstruction instruction in instructions)
        {
            if (!injected && instruction.Calls(PanelMethod))
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0); // UIBuilder
                yield return new CodeInstruction(OpCodes.Ldarg_1); // worker
                yield return new CodeInstruction(OpCodes.Call, InjectMethod);
                injected = true;
            }

            yield return instruction;
        }
    }

    private static void AddVisualizationButton(UIBuilder ui, Worker worker)
    {
        if (!SpatialVariableVisualizer.IsSupported(worker))
        {
            return;
        }

        ui.Style.MinHeight = 24f;
        Button button = ui.Button("Inspector.Collider.Visualize".AsLocaleKey());
        ButtonTargets.Remove(button);
        ButtonTargets.Add(button, worker);
        button.LocalPressed += HandleVisualizationButton;
    }

    private static void HandleVisualizationButton(IButton button, ButtonEventData _)
    {
        if (button is not Button concrete)
        {
            return;
        }

        if (ButtonTargets.TryGetValue(concrete, out Worker? target))
        {
            SpatialVariableVisualizer.StartVisualization(target, button);
        }
    }
}

internal static class SpatialVariableVisualizer
{
    private static readonly ConcurrentDictionary<Type, FieldInfo?> RadiusFieldCache = new();
    private static readonly ConcurrentDictionary<Type, FieldInfo?> SizeFieldCache = new();
    private static readonly colorX VisualizationColor = colorX.Green.SetA(0.53f);

    public static bool IsSupported(Worker worker)
    {
        if (worker == null)
        {
            return false;
        }

        Type type = worker.GetType();
        return IsSubtypeOfGeneric(type, typeof(BoxSpatialVariable<>))
            || IsSubtypeOfGeneric(type, typeof(SphereSpatialVariable<>));
    }

    public static void StartVisualization(Worker worker, IButton button)
    {
        if (worker == null || button == null)
        {
            return;
        }

        if (!IsSupported(worker))
        {
            return;
        }

        if (button is Component buttonComponent)
        {
            buttonComponent.Enabled = false;
        }
        worker.StartTask(() => VisualizeLoop(worker, button));
    }

    private static async Task VisualizeLoop(Worker worker, IButton button)
    {
        if (button is not Worker buttonWorker)
        {
            return;
        }

        while (!buttonWorker.IsRemoved && !worker.IsRemoved)
        {
            Draw(worker);
            await default(NextUpdate);
        }
    }

    private static void Draw(Worker worker)
    {
        if (worker is not Component component || component.Slot == null || component.Slot.IsDestroyed)
        {
            return;
        }

        Type type = worker.GetType();
        if (IsSubtypeOfGeneric(type, typeof(BoxSpatialVariable<>)))
        {
            if (TryReadSize(worker, out float3 size))
            {
                DrawBox(component, size);
            }
        }
        else if (IsSubtypeOfGeneric(type, typeof(SphereSpatialVariable<>)))
        {
            if (TryReadRadius(worker, out float radius))
            {
                DrawSphere(component, radius);
            }
        }
    }

    private static void DrawBox(Component component, in float3 size)
    {
        Slot slot = component.Slot;
        float3 globalSize = slot.LocalScaleToGlobal(size);
        float3 center = slot.LocalPointToGlobal(float3.Zero);
        floatQ rotation = slot.GlobalRotation;
        component.Debug.Box(
            in center,
            in globalSize,
            in VisualizationColor,
            in rotation,
            duration: 0f,
            local: false);
    }

    private static void DrawSphere(Component component, float radius)
    {
        Slot slot = component.Slot;
        float scaledRadius = slot.LocalScaleToGlobal(radius);
        float3 center = slot.LocalPointToGlobal(float3.Zero);
        component.Debug.Sphere(
            in center,
            scaledRadius,
            in VisualizationColor,
            2,
            duration: 0f,
            local: false);
    }

    private static bool TryReadRadius(Worker worker, out float radius)
    {
        radius = 0f;
        FieldInfo? field = RadiusFieldCache.GetOrAdd(worker.GetType(), t => AccessTools.Field(t, "Radius"));
        if (field?.GetValue(worker) is IField<float> radiusField)
        {
            radius = radiusField.Value;
            return true;
        }

        return false;
    }

    private static bool TryReadSize(Worker worker, out float3 size)
    {
        size = float3.Zero;
        FieldInfo? field = SizeFieldCache.GetOrAdd(worker.GetType(), t => AccessTools.Field(t, "Size"));
        if (field?.GetValue(worker) is IField<float3> sizeField)
        {
            size = sizeField.Value;
            return true;
        }

        return false;
    }

    private static bool IsSubtypeOfGeneric(Type type, Type openGeneric)
    {
        Type? current = type;
        while (current != null && current != typeof(object))
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == openGeneric)
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}
