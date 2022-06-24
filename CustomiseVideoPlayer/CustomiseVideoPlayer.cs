using BaseX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using System.Collections.Generic;

namespace Customise_Modular
{
    public class CustomiseVideoPlayer : NeosMod
    {
        public override string Name => "Customise-VideoPlayer";
        public override string Author => "LeCloutPanda";
        public override string Version => "1.1.2";

        public static ModConfiguration config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("Enabled", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> MAIN_ALBEDO = new ModConfigurationKey<color>("Main Albedo Color", "", () => new color(1f, 0.73f, 0.09f, 0.2f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> MAIN_METALLIC = new ModConfigurationKey<float>("Main Metallic Amount", "", () => 0.5f);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> MAIN_RIM_COLOR = new ModConfigurationKey<color>("Main Rim Color", "", () => new color(0f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> MAIN_RIM_INTENSITY = new ModConfigurationKey<float>("Main Rim Intensity", "", () => 0f);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> LEFT_RIGHT_ALBEDO = new ModConfigurationKey<color>("Left/Right Albedo Color", "", () => new color(1f, 0.73f, 0.09f, 0.2f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> LEFT_RIGHT_METALLIC = new ModConfigurationKey<float>("Left/Right Metallic Amount", "", () => 0.5f);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> LEFT_RIGHT_RIM_COLOR = new ModConfigurationKey<color>("Left/Right Rim Color", "", () => new color(0f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> LEFT_RIGHT_RIM_INTENSITY = new ModConfigurationKey<float>("Left/Right Rim Intensity", "", () => 0f);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> CURSOR_ALBEDO = new ModConfigurationKey<color>("Cursor Albedo Color", "", () => new color(1f, 0.73f, 0.09f, 0.2f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> CURSOR_METALLIC = new ModConfigurationKey<float>("Cursor Metallic Amount", "", () => 0.5f);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> CURSOR_RIM_COLOR = new ModConfigurationKey<color>("Cursor Rim Color", "", () => new color(0f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> CURSOR_RIM_INTENSITY = new ModConfigurationKey<float>("Cursor Rim Intensity", "", () => 0f);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> SPAWN_SCALE = new ModConfigurationKey<float>("Spawn Scale Multiplier", "", () => 1);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> LOCAL_VOLUME_ENABLED = new ModConfigurationKey<bool>("Local volume slider", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> DEFAULT_STREAM_ENABLED = new ModConfigurationKey<bool>("Enable Stream by default", "", () => true);

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            config.Save(true);

            Harmony harmony = new Harmony($"dev.{Author}.{Name}");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(VideoPlayer), "OnAttach")]
        class VideoPlayerPatcher
        {
            [HarmonyPrefix]
            static void Prefix(ref VideoPlayer __instance)
            {
                if (!config.GetValue(ENABLED))
                    return;

                __instance.Slot.Name = "Custom Video Player";

                Slot Assets = __instance.Slot.AddSlot("Assets");
                Assets.Tag = "Customise.Assets";
            }

            [HarmonyPostfix]
            static void PrettifyPanel(ref VideoPlayer __instance)
            {
                if (!config.GetValue(ENABLED))
                    return;
              
                if (__instance.Slot.Name != "Custom Video Player")
                    return;

                //Scale Video Player based on scaler
                __instance.Slot.GlobalScale = __instance.Slot.GlobalScale * config.GetValue(SPAWN_SCALE);

                // Generic grab slot code again hahahahahahahahahaha
                Slot frameSlot = __instance.Slot.FindChild(ch => ch.Name.Equals("Frame"), 1);

                Slot timelineSlot = __instance.Slot.FindChild(ch => ch.Name.Equals("Timeline"), 1);
                Slot timelineLeftSlot = timelineSlot.FindChild(ch => ch.Name.Equals("Left"), 1);
                Slot timelineRightSlot = timelineSlot.FindChild(ch => ch.Name.Equals("Right"), 1);
                Slot timelineCursorSlot = timelineSlot.FindChild(ch => ch.Name.Equals("Cursor"), 1);

                Slot volumeSlot = __instance.Slot.FindChild(ch => ch.Name.Equals("Volume"), 1);
                Slot volumeLeftSlot = volumeSlot.FindChild(ch => ch.Name.Equals("Left"), 1);
                Slot volumeRightSlot = volumeSlot.FindChild(ch => ch.Name.Equals("Right"), 1);
                Slot volumeCursorSlot = volumeSlot.FindChild(ch => ch.Name.Equals("Cursor"), 1);
                Slot assetsSlot = __instance.Slot.FindChild(ch => ch.Tag.Equals("Customise.Assets"));

                PBS_RimMetallic newMaterial1 = assetsSlot.AttachComponent<PBS_RimMetallic>(true, null);
                // Stupid RimMetallic Stuff
                newMaterial1.OffsetFactor.Value = 1;
                newMaterial1.OffsetUnits.Value = 1;
                newMaterial1.ForceZWrite.Value = true;
                newMaterial1.Transparent.Value = true;
                newMaterial1.RenderQueue.Value = 2995;
                newMaterial1.AlbedoColor.Value = config.GetValue(MAIN_ALBEDO);
                newMaterial1.RimColor.Value = config.GetValue(MAIN_RIM_COLOR);
                newMaterial1.RimPower.Value = config.GetValue(MAIN_RIM_INTENSITY);
                newMaterial1.Metallic.Value = config.GetValue(MAIN_METALLIC);

                PBS_RimMetallic newMaterial2 = assetsSlot.AttachComponent<PBS_RimMetallic>(true, null);
                // Stupid RimMetallic Stuff
                newMaterial2.OffsetFactor.Value = 1;
                newMaterial2.OffsetUnits.Value = 1;
                newMaterial2.ForceZWrite.Value = true;
                newMaterial2.Transparent.Value = true;
                newMaterial2.RenderQueue.Value = 2995;
                newMaterial2.AlbedoColor.Value = config.GetValue(LEFT_RIGHT_ALBEDO);
                newMaterial2.RimColor.Value = config.GetValue(LEFT_RIGHT_RIM_COLOR);
                newMaterial2.RimPower.Value = config.GetValue(LEFT_RIGHT_RIM_INTENSITY);
                newMaterial2.Metallic.Value = config.GetValue(LEFT_RIGHT_METALLIC);

                PBS_RimMetallic newMaterial3 = assetsSlot.AttachComponent<PBS_RimMetallic>(true, null);
                // Stupid RimMetallic Stuff
                newMaterial3.OffsetFactor.Value = 1;
                newMaterial3.OffsetUnits.Value = 1;
                newMaterial3.ForceZWrite.Value = true;
                newMaterial3.Transparent.Value = true;
                newMaterial3.RenderQueue.Value = 2995;
                newMaterial3.AlbedoColor.Value = config.GetValue(CURSOR_ALBEDO);
                newMaterial3.RimColor.Value = config.GetValue(CURSOR_RIM_COLOR);
                newMaterial3.RimPower.Value = config.GetValue(CURSOR_RIM_INTENSITY);
                newMaterial3.Metallic.Value = config.GetValue(CURSOR_METALLIC);

                DoFunny(frameSlot, newMaterial1);

                DoFunny(timelineSlot, newMaterial1);
                DoFunny(timelineLeftSlot, newMaterial2);
                DoFunny(timelineRightSlot, newMaterial2);
                DoFunny(timelineCursorSlot, newMaterial3);

                DoFunny(volumeSlot, newMaterial1);
                DoFunny(volumeLeftSlot, newMaterial2);
                DoFunny(volumeRightSlot, newMaterial2);
                DoFunny(volumeCursorSlot, newMaterial3);           

                // Apply local volume slider
                if (config.GetValue(LOCAL_VOLUME_ENABLED))
                {
                    ValueUserOverride<float> valueUserOverride = __instance.Slot.AttachComponent<ValueUserOverride<float>>();
                    valueUserOverride.CreateOverrideOnWrite.Value = true;
                    AudioOutput output = __instance.Slot.GetComponent<AudioOutput>();
                    valueUserOverride.Target.Value = output.Volume.ReferenceID;
                    valueUserOverride.Default.Value = 1f;
                }

                __instance.Slot.GetComponent<VideoTextureProvider>().Stream.Value = config.GetValue(DEFAULT_STREAM_ENABLED);
            }
        }

        public static void DoFunny(Slot slot, MaterialProvider material, bool blur = false)
        {
            List<MeshRenderer> mesh = slot.GetComponents<MeshRenderer>();
            mesh[0].Material.Target = material;

            if (blur != true)
            {
                for (int i = 1; i < mesh.Count; i++)
                {
                    mesh[i].Destroy();
                }
            }
        }
    }
}
