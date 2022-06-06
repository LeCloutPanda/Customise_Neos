using BaseX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Collections.Generic;

namespace CustomiseLogixBrowser
{
    public class CustomiseLogixBrowser : NeosMod
    {
        public override string Name => "Customise-LogixBrowser";
        public override string Author => "LeCloutPanda";
        public override string Version => "1.1.1";

        public static ModConfiguration config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("Enabled", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<string> TITLE_TEXT = new ModConfigurationKey<string>("Title Text", "", () => "Logix Browser");
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> TITLE_COLOR = new ModConfigurationKey<color>("Title Color", "", () => new color(0f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> ALBEDO = new ModConfigurationKey<color>("Albedo Color", "", () => new color(1f, 0.87f, 0.55f, 0.2f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> METALIC = new ModConfigurationKey<float>("Metalic Amount", "", () => 0.5f);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> BLUR_ENABLED = new ModConfigurationKey<bool>("Enable Blur Material", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<string> BACKGROUND_IMAGE = new ModConfigurationKey<string>("Background Image", "", () => "neosdb:///63ef318d96b5d0d0ceba6e04a4e622b1158335cdc67c49e27839132c6f655058.png");
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> BACKGROUND_IMAGE_COLOR = new ModConfigurationKey<color>("Background Image Color", "", () => new color(0f, 0f));
        //[AutoRegisterConfigKey]
        //private static ModConfigurationKey<float3> BACKGROUND_IMAGE_ROTATION = new ModConfigurationKey<float3>("Background Image Rotation", "", () => new float3(0f, 0f, 0f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float2> CANVAS_SIZE = new ModConfigurationKey<float2>("Canvas Size", "", () => new float2(700, 700));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> SPAWN_SCALE = new ModConfigurationKey<float>("Spawn Scale Multiplier", "", () => 1);

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> FOLDER_COLOR = new ModConfigurationKey<color>("LogiX Browser Folders Color", "", () => new color(1f, 1f, 0.8f, 1));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> NODE_COLOR = new ModConfigurationKey<color>("LogiX Browser Node Color", "", () => new color(0.8f, 0.9f, 1f, 1));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> DELEGATE_COLOR = new ModConfigurationKey<color>("LogiX Browser Delegate Color", "", () => new color(0.8f, 1f, 0.8f, 1));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> BACK_COLOR = new ModConfigurationKey<color>("LogiX Browser Back Button Color", "", () => new color(1f, 0.8f, 0.8f, 1));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> TEXT_COLOR = new ModConfigurationKey<color>("LogiX Browser Text Color", "", () => new color(0f, 0f, 0f, 1f));

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            config.Save(true);

            Harmony harmony = new Harmony($"dev.{Author}.{Name}");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(LogixNodeSelector), "OnAttach")]
        class LogixNodeSelectorPatcher
        {
            [HarmonyPrefix]
            static void Prefix(ref LogixNodeSelector __instance)
            {
                if (!config.GetValue(ENABLED))
                    return;

                __instance.Slot.Name = "Custom Logix Browser";

                Slot Assets = __instance.Slot.AddSlot("Assets");
                Assets.Tag = "Customise.Assets";
            }

            [HarmonyPostfix]
            static void Postfix(ref LogixNodeSelector __instance)
            {
                if (!config.GetValue(ENABLED) && __instance.Slot.Name != "Custom LogiX Browser")
                    return;

                __instance.Slot.GlobalScale = __instance.Slot.GlobalScale * config.GetValue(SPAWN_SCALE);

                Slot panelSlot = __instance.Slot.FindChild(ch => ch.Name.Equals("Panel"), 1);
                Slot handleSlot = __instance.Slot.FindChild(ch => ch.Name.Equals("Handle"), 1);
                Slot titleMeshSlot = __instance.Slot.FindChild(ch => ch.Name.Equals("Title Mesh"), 2);
                Slot titleSlotText = __instance.Slot.FindChild(ch => ch.Name.Equals("Title"), 2);
                Slot contentSlot = __instance.Slot.FindChild(ch => ch.Name.Equals("Content"), 1);
                Slot assetsSlot = __instance.Slot.FindChild(ch => ch.Tag.Equals("Customise.Assets"));

                PBS_RimMetallic newMaterial = assetsSlot.AttachComponent<PBS_RimMetallic>(true, null);
                newMaterial.OffsetFactor.Value = 1;
                newMaterial.OffsetUnits.Value = 1;
                newMaterial.ForceZWrite.Value = true;
                newMaterial.Transparent.Value = true;
                newMaterial.RenderQueue.Value = 2995;
                newMaterial.AlbedoColor.Value = config.GetValue(ALBEDO);
                newMaterial.RimColor.Value = new color(0f, 0f);
                newMaterial.Metallic.Value = config.GetValue(METALIC);

                bool enableBlur = config.GetValue(BLUR_ENABLED);

                DoFunny(panelSlot, newMaterial, enableBlur);
                DoFunny(handleSlot, newMaterial, enableBlur);
                DoFunny(titleMeshSlot, newMaterial, enableBlur);

                AddBackgroundImage(__instance.Slot, contentSlot.GetComponent<Canvas>());

                TextRenderer textRenderer = titleSlotText.GetComponents<TextRenderer>(null, false)[0];
                textRenderer.Color.Value = config.GetValue(TITLE_COLOR);

                Canvas canvas = contentSlot.GetComponent<Canvas>();
                canvas.Size.Value = config.GetValue(CANVAS_SIZE);

                NeosPanel __result = __instance.Slot.GetComponent<NeosPanel>();

                __result.RunInUpdates(3, () => {
                    __result.MarkChangeDirty();
                    __result.Title = config.GetValue(TITLE_TEXT);
                });
            }
        }

        [HarmonyPatch(typeof(LogixNodeSelector), "BuildUI")]
        class LogixNodeSelectorButtonPatcher
        {
            [HarmonyPostfix]
            static void PrettifyButtons(ref LogixNodeSelector __instance)
            {
                if (!config.GetValue(ENABLED) && __instance.Slot.Name != "Custom LogiX Browser")
                    return;

                Slot slot = __instance.Slot;
                Slot containerSlot = __instance.Slot.FindChild((Slot c) => c.Name == "Content").FindChild((Slot c) => c.Name == "Container");
                Slot contentSlot = containerSlot[containerSlot.ChildrenCount - 1].FindChild((Slot c) => c.Name == "Scroll Area")[0];
                Slot assetsSlot = __instance.Slot.FindChild((Slot c) => c.Tag == "InspectorCustomizer.Assets");

                var GridLayout = contentSlot.GetComponent<GridLayout>();
                GridLayout.ChildAlignment = Alignment.MiddleLeft;
                GridLayout.PaddingLeft.Value = 2;
                GridLayout.PaddingTop.Value = 2;

                foreach (Slot c in contentSlot.Children)
                {
                    var Image = c.GetComponent<Image>();

                    color val = Image.Tint.Value;
                    color FolderColor = new color(1f, 1f, 0.8f, 1);
                    color NodeColor = new color(0.8f, 0.9f, 1f, 1);
                    color DelegateColor = new color(0.8f, 1f, 0.8f, 1);
                    color BackColor = new color(1f, 0.8f, 0.8f, 1);

                    if (val == FolderColor) Image.Tint.Value = config.GetValue(FOLDER_COLOR);
                    else if (val == NodeColor) Image.Tint.Value = config.GetValue(NODE_COLOR);
                    else if (val == DelegateColor) Image.Tint.Value = config.GetValue(DELEGATE_COLOR);
                    else if (val == BackColor) Image.Tint.Value = config.GetValue(BACK_COLOR);

                    Slot TextSlot = c[0];
                    Text text = TextSlot.GetComponent<Text>();
                    text.Color.Value = config.GetValue(TEXT_COLOR);

                    RectTransform rect = TextSlot.GetComponent<RectTransform>();
                    rect.AnchorMin.Value = new float2(0.03f, 0f);
                    rect.AnchorMax.Value = new float2(0.97f, 1f);
                }
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

        public static void AddBackgroundImage(Slot parent, Canvas canvas)
        {
            Slot backImageSlot = parent.AddSlot("Background", true);
            Canvas backImageCanvas = backImageSlot.AttachComponent<Canvas>();
            Image backImageImage = backImageSlot.AttachComponent<Image>();
            ValueCopy<float2> backImageValueCopy = backImageSlot.AttachComponent<ValueCopy<float2>>();

            backImageSlot.LocalPosition = new float3(0, 0, 0.011f);
            backImageSlot.LocalScale = canvas.Slot.LocalScale;
            //backImageSlot.LocalRotation = new floatQ(config.GetValue(BACKGROUND_IMAGE_ROTATION).x, config.GetValue(BACKGROUND_IMAGE_ROTATION).y, config.GetValue(BACKGROUND_IMAGE_ROTATION).z, 360f);
            
            backImageValueCopy.Source.Value = canvas.Size.ReferenceID;
            backImageValueCopy.Target.Value = backImageCanvas.Size.ReferenceID;

            backImageImage.Sprite.Target = backImageImage.Slot.AttachSprite(new Uri(config.GetValue(BACKGROUND_IMAGE)), true, false, true, null);
            backImageImage.Tint.Value = config.GetValue(BACKGROUND_IMAGE_COLOR);
        }
    }
}
