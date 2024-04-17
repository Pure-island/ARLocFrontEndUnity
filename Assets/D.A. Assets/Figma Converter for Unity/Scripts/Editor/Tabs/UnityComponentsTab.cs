using DA_Assets.Shared;
using UnityEngine;

#pragma warning disable IDE0003
#pragma warning disable CS0649

namespace DA_Assets.FCU
{
    internal class UnityComponentsTab : ScriptableObjectBinder<FcuSettingsWindow, FigmaConverterUnity>
    {
        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_import_components.Localize(), FcuLocKey.tooltip_import_components.Localize());
            gui.Space15();

            monoBeh.Settings.ComponentSettings.ImageComponent = gui.EnumField(
                new GUIContent(FcuLocKey.label_image_component.Localize(), FcuLocKey.tooltip_image_component.Localize()),
                monoBeh.Settings.ComponentSettings.ImageComponent, true);

            monoBeh.Settings.ComponentSettings.TextComponent = gui.EnumField(
                new GUIContent(FcuLocKey.label_text_component.Localize(), FcuLocKey.tooltip_text_component.Localize()),
                monoBeh.Settings.ComponentSettings.TextComponent, true);

            monoBeh.Settings.ComponentSettings.ShadowComponent = gui.EnumField(
                new GUIContent(FcuLocKey.label_shadow_type.Localize(), FcuLocKey.tooltip_shadow_type.Localize()),
                monoBeh.Settings.ComponentSettings.ShadowComponent, true);

            monoBeh.Settings.ComponentSettings.ButtonComponent = gui.EnumField(
                new GUIContent(FcuLocKey.label_button_type.Localize(), FcuLocKey.tooltip_button_type.Localize()),
                monoBeh.Settings.ComponentSettings.ButtonComponent, true, null);

            monoBeh.Settings.ComponentSettings.UseI2Localization = gui.Toggle(
                new GUIContent(FcuLocKey.label_use_i2localization.Localize(), FcuLocKey.tooltip_use_i2localization.Localize()),
                monoBeh.Settings.ComponentSettings.UseI2Localization);

            gui.Space15();

            switch (monoBeh.Settings.ComponentSettings.ImageComponent)
            {
                case ImageComponent.UnityImage:
                case ImageComponent.RawImage:
                    this.UnityImageSettingsTab.Draw();
                    break;
                case ImageComponent.Shape:
                    this.Shapes2DSettingsTab.Draw();
                    break;
                case ImageComponent.ProceduralImage:
                    this.PuiSettingsTab.Draw();
                    break;
                case ImageComponent.MPImage:
                    this.MPImageSettingsTab.Draw();
                    break;
            }

            gui.Space15();

            switch (monoBeh.Settings.ComponentSettings.TextComponent)
            {
                case TextComponent.UnityText:
                    this.DefaultTextSettingsTab.Draw();
                    break;
                case TextComponent.TextMeshPro:
                    this.TextMeshProSettingsTab.Draw();
                    break;
            }
        }

        private UnityImageSettings unityImageSettingsTab;
        internal UnityImageSettings UnityImageSettingsTab => unityImageSettingsTab.Bind(scriptableObject, monoBeh);

        private Shapes2DSettings shapesSettingsTab;
        internal Shapes2DSettings Shapes2DSettingsTab => shapesSettingsTab.Bind(scriptableObject, monoBeh);

        private PuiSettings puiSettingsTab;
        internal PuiSettings PuiSettingsTab => puiSettingsTab.Bind(scriptableObject, monoBeh);

        private MPImageSettings mpImageSettingsTab;
        internal MPImageSettings MPImageSettingsTab => mpImageSettingsTab.Bind(scriptableObject, monoBeh);

        private TextMeshProSettings textMeshProSettingsTab;
        internal TextMeshProSettings TextMeshProSettingsTab => textMeshProSettingsTab.Bind(scriptableObject, monoBeh);

        private UnityTextSettings defaultTextSettingsTab;
        internal UnityTextSettings DefaultTextSettingsTab => defaultTextSettingsTab.Bind(scriptableObject, monoBeh);
    }
}