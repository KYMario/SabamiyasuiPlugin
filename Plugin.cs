using System;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;

namespace SabamiyasuiPlugin;

[BepInPlugin(PluginGuid, "sabamiyasuiPlugin", "1.0.0")]
public class Plugin : BasePlugin
{
    public static ConfigEntry<bool> Sort { get; private set; }
    public const string PluginGuid = "com.kymario.sabamiyasui";
    public Harmony Harmony { get; } = new Harmony(PluginGuid);
    public override void Load()
    {
        Sort = Config.Bind("Options", "sort", false);
        RegionMenuOpenPatch.IsSort = Sort.Value;
        Harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
class LateUpdatePatch
{
    public static void Postfix(ModManager __instance)
    {
        __instance.ShowModStamp();
    }
}

[HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
[HarmonyPriority(Priority.Last)]
class RegionMenuOpenPatch
{
    public static bool IsSort = false;
    public static PassiveButton Sortbutton;
    public static void Postfix(RegionMenu __instance)
    {
        var menu = GameObject.Find("NormalMenu/RegionMenu/");
        var RegionButton = GameObject.Find("NormalMenu/RegionButton");
        if (!menu || !RegionButton) return;

        Sort();
        if (Sortbutton.IsDestroyedOrNull())
        {
            Sortbutton = GameObject.Instantiate(RegionButton, menu.transform).GetComponent<PassiveButton>();
            Sortbutton.OnClick = new();
            Sortbutton.gameObject.transform.DestroyChildren();
            Sortbutton.AddOnClickListeners((Action)(() =>
            {
                IsSort = !IsSort;
                Plugin.Sort.Value = IsSort;
                Sort();
            }));
        }
        void Sort()
        {
            int i = 0;
            Component[] components = menu.GetComponentsInChildren<ServerListButton>();

            if (components[0].transform.localPosition.y != 2)
            {
                var rev = components.ToList();
                rev.Reverse();
                components = rev.ToArray();
            }

            foreach (Component component in components)
            {
                var button = component.gameObject;
                if (!button.active) continue;
                if (!IsSort)
                    button.transform.localPosition = new(0, 2 - (0.5f * i));
                else
                    button.transform.localPosition = new(-4 + (2 * (i % 5)), 2 - (0.5f * (i / 5)));
                i++;
            }
        }
    }
}