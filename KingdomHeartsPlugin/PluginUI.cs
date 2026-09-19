using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Bindings.ImGui;
using KingdomHeartsPlugin.Configuration;
using KingdomHeartsPlugin.Enums;
using KingdomHeartsPlugin.UIElements.Experience;
using KingdomHeartsPlugin.UIElements.HealthBar;
using KingdomHeartsPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility;

namespace KingdomHeartsPlugin
{
    public class PluginUI : IDisposable
    {
        internal Settings Configuration;
        public readonly HealthFrame HealthFrame;
        private readonly FileDialogManager _dialogManager;

        private bool visible = true;
        public bool Visible
        {
            get => visible;
            set => visible = value;
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        public PluginUI(Settings configuration)
        {
            Configuration = configuration;
            HealthFrame = new HealthFrame();
            _dialogManager = SetupDialogManager();
        }

        public void Dispose()
        {
            HealthFrame?.Dispose();
            Portrait.Dispose();
            ImageDrawing.Dispose();
        }

        public void OnUpdate()
        {
        }

        public void Draw()
        {
            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            Visible = true;

            CheckNpcTalkingVisibility();

            if (!Visible || !KingdomHeartsPlugin.Ui.Configuration.Enabled)
            {
                return;
            }

            ImGuiWindowFlags window_flags = 0;
            window_flags |= ImGuiWindowFlags.NoTitleBar;
            window_flags |= ImGuiWindowFlags.NoScrollbar;
            if (Configuration.Locked)
            {
                window_flags |= ImGuiWindowFlags.NoMove;
                window_flags |= ImGuiWindowFlags.NoMouseInputs;
                window_flags |= ImGuiWindowFlags.NoNav;
            }
            window_flags |= ImGuiWindowFlags.AlwaysAutoResize;
            window_flags |= ImGuiWindowFlags.NoBackground;

            var size = new Vector2(320, 320);
            ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
            
            if (ImGui.Begin("KH Frame", ref visible, window_flags))
            {
                HealthFrame.Draw();
            }
            ImGui.End();
        }

        private unsafe void CheckNpcTalkingVisibility()
        {
            var actionBarWidget = (AtkUnitBase*)KingdomHeartsPlugin.Gui.GetAddonByName("_ActionBar", 1).Address;
            var actionCrossWidget = (AtkUnitBase*)KingdomHeartsPlugin.Gui.GetAddonByName("_ActionCross", 1).Address;

            if (actionBarWidget == null || actionCrossWidget == null || !KingdomHeartsPlugin.Ui.Configuration.HideWhenNpcTalking) return;

            if (!actionBarWidget->IsVisible && !actionCrossWidget->IsVisible)
                Visible = false;
        }

        private void GeneralSettings()
        {
            if (!ImGui.BeginTabItem("General")) return;

            SectionHeader("Visibility & State");
            var enabled = Configuration.Enabled;
            if (ImGui.Checkbox("Visible", ref enabled)) Configuration.Enabled = enabled;

            var hideWhenNpcTalking = Configuration.HideWhenNpcTalking;
            if (ImGui.Checkbox("Hide when dialogue box is shown", ref hideWhenNpcTalking)) Configuration.HideWhenNpcTalking = hideWhenNpcTalking;

            var locked = Configuration.Locked;
            if (ImGui.Checkbox("Lock UI Position", ref locked)) Configuration.Locked = locked;

            ImGui.Spacing();
            SectionHeader("Transform");
            var scale = Configuration.Scale;
            if (ImGui.SliderFloat("Overall Scale", ref scale, 0.25f, 3.0f, "%.2f"))
            {
                Configuration.Scale = scale;
            }

            ImGui.EndTabItem();
        }

        private void HealthSettings()
        {
            if (!ImGui.BeginTabItem("Health")) return;

            var enabled = Configuration.HpBarEnabled;
            if (ImGui.Checkbox("Enable Health Bar", ref enabled)) Configuration.HpBarEnabled = enabled;

            ImGui.Spacing();
            SectionHeader("Standard Length Scaling");
            ImGui.Indent();

            var fullRing = Configuration.HpForFullRing;
            if (ImGui.InputInt("HP for full ring", ref fullRing, 5, 50)) Configuration.HpForFullRing = Math.Max(1, fullRing);
            HoverTooltip($"How much HP will make the ring max out, then goes long bar.\nDefault: {Defaults.HpForFullRing}");

            var hpPerPixel = Configuration.HpPerPixelLongBar;
            if (ImGui.InputFloat("HP per pixel for long bar", ref hpPerPixel, 5, 50)) Configuration.HpPerPixelLongBar = Math.Max(0.0001f, hpPerPixel);
            HoverTooltip($"Defines the width of the long bar.\nDefault: {Defaults.HpPerPixelLongBar}");

            var minLength = Configuration.MinimumHpForLength;
            if (ImGui.InputInt("Max HP for minimum length", ref minLength, 5, 50)) Configuration.MinimumHpForLength = Math.Max(1, minLength);
            HoverTooltip($"Defines when the total bar size will stop getting smaller.\nDefault: {Defaults.MinimumHpForLength}");

            var maxLength = Configuration.MaximumHpForMaximumLength;
            if (ImGui.InputInt("Max HP for maximum total length", ref maxLength, 5, 50)) Configuration.MaximumHpForMaximumLength = Math.Max(1, maxLength);
            HoverTooltip($"Defines when the total bar size will stop getting larger.\nDefault: {Defaults.MaximumHpForMaximumLength}");

            var lengthByLevel = Configuration.LengthByLevel;
            if (ImGui.Checkbox("Scale Max Length by Level", ref lengthByLevel)) Configuration.LengthByLevel = lengthByLevel;
            HoverTooltip("Overrides manual Max HP limits. Replaces actual Max HP with a simulated pool based on Level.");

            if (Configuration.LengthByLevel)
            {
                ImGui.Indent();
                var hpPerLevel = Configuration.HpPerLevel;
                if (ImGui.InputInt("Simulated HP added per Level", ref hpPerLevel, 10, 100)) Configuration.HpPerLevel = Math.Max(1, hpPerLevel);
                ImGui.Unindent();
            }
            ImGui.Unindent();

            ImGui.Spacing();
            SectionHeader("PvP Length Scaling");
            ImGui.Indent();

            var fullRingPvp = Configuration.PvpHpForFullRing;
            if (ImGui.InputInt("PvP HP for full ring", ref fullRingPvp, 5, 50)) Configuration.PvpHpForFullRing = Math.Max(1, fullRingPvp);

            var hpPerPixelPvp = Configuration.PvpHpPerPixelLongBar;
            if (ImGui.InputFloat("PvP HP per pixel long bar", ref hpPerPixelPvp, 5, 50)) Configuration.PvpHpPerPixelLongBar = Math.Max(0.0001f, hpPerPixelPvp);

            var minLengthPvp = Configuration.PvpMinimumHpForLength;
            if (ImGui.InputInt("PvP Max HP for min length", ref minLengthPvp, 5, 50)) Configuration.PvpMinimumHpForLength = Math.Max(1, minLengthPvp);

            var maxLengthPvp = Configuration.PvpMaximumHpForMaximumLength;
            if (ImGui.InputInt("PvP Max HP for max length", ref maxLengthPvp, 5, 50)) Configuration.PvpMaximumHpForMaximumLength = Math.Max(1, maxLengthPvp);

            ImGui.Unindent();

            ImGui.Spacing();
            SectionHeader("Value Text");

            var showHpVal = Configuration.ShowHpVal;
            if (ImGui.Checkbox("Show HP Value", ref showHpVal)) Configuration.ShowHpVal = showHpVal;

            if (Configuration.ShowHpVal)
            {
                ImGui.Indent();
                var hpTextPos = new Vector2(Configuration.HpValueTextPositionX, Configuration.HpValueTextPositionY);
                if (ImGui.DragFloat2("Text Position (X, Y)", ref hpTextPos))
                {
                    Configuration.HpValueTextPositionX = hpTextPos.X;
                    Configuration.HpValueTextPositionY = hpTextPos.Y;
                }

                var hpTextSize = Configuration.HpValueTextSize;
                if (ImGui.InputFloat("Text Size", ref hpTextSize)) Configuration.HpValueTextSize = hpTextSize;

                Configuration.HpValueTextAlignment = (int)DrawEnumCombo("Text Alignment", (TextAlignment)Configuration.HpValueTextAlignment);
                HoverTooltip("Center and right alignments may not hold perfect positioning.");

                Configuration.HpValueTextStyle = DrawEnumCombo("Text Formatting", Configuration.HpValueTextStyle,
                    val => $"{val.GetDescription()} ({StringFormatting.FormatDigits(1234567, val)})");
                ImGui.Unindent();
            }

            ImGui.Spacing();
            SectionHeader("Effects & Misc");
            var lowHpPercent = Configuration.LowHpPercent;
            if (ImGui.SliderFloat("Trigger Low HP Warning (%)", ref lowHpPercent, 0, 100)) Configuration.LowHpPercent = lowHpPercent;

            var hpDamageWobbleIntensity = Configuration.HpDamageWobbleIntensity;
            if (ImGui.SliderFloat("Damage Wobble Intensity (%)", ref hpDamageWobbleIntensity, 0, 200)) Configuration.HpDamageWobbleIntensity = hpDamageWobbleIntensity;

            var showHpRecovery = Configuration.ShowHpRecovery;
            if (ImGui.Checkbox("Show HP Recovery Animation", ref showHpRecovery)) Configuration.ShowHpRecovery = showHpRecovery;
            HoverTooltip("Shows a blue bar for when HP is recovered then gradually fills the green bar.");

            var hpAnimSpeed = Configuration.HpAnimationSpeed;
            if (ImGui.SliderFloat("Animation Speed (% Max HP/s)", ref hpAnimSpeed, 10f, 200f)) Configuration.HpAnimationSpeed = hpAnimSpeed;
            HoverTooltip("How fast the damaged/restored HP catches up to current HP.");

            var hpAnimDelay = Configuration.HpAnimationDelay;
            if (ImGui.SliderFloat("Animation Delay (s)", ref hpAnimDelay, 0f, 3f)) Configuration.HpAnimationDelay = hpAnimDelay;
            HoverTooltip("How long to wait before the HP drain/fill animation starts after a change.");

            ImGui.EndTabItem();
        }

        private void ResourceSettings()
        {
            if (!ImGui.BeginTabItem("MP/GP/CP")) return;

            SectionHeader("General Options");
            var enabled = Configuration.ResourceBarEnabled;
            if (ImGui.Checkbox("Enable Resource Bar", ref enabled)) Configuration.ResourceBarEnabled = enabled;

            var resourcePos = new Vector2(Configuration.ResourceBarPositionX, Configuration.ResourceBarPositionY);
            if (ImGui.DragFloat2("Position (X, Y)", ref resourcePos))
            {
                Configuration.ResourceBarPositionX = resourcePos.X;
                Configuration.ResourceBarPositionY = resourcePos.Y;
            }

            ImGui.Spacing();
            SectionHeader("Value Text");
            var showVal = Configuration.ShowResourceVal;
            if (ImGui.Checkbox("Show Resource Value", ref showVal)) Configuration.ShowResourceVal = showVal;

            if (Configuration.ShowResourceVal)
            {
                ImGui.Indent();
                var resourceTextPos = new Vector2(Configuration.ResourceTextPositionX, Configuration.ResourceTextPositionY);
                if (ImGui.DragFloat2("Text Position (X, Y)", ref resourceTextPos))
                {
                    Configuration.ResourceTextPositionX = resourceTextPos.X;
                    Configuration.ResourceTextPositionY = resourceTextPos.Y;
                }

                var resourceTextSize = Configuration.ResourceTextSize;
                if (ImGui.InputFloat("Text Size", ref resourceTextSize)) Configuration.ResourceTextSize = resourceTextSize;

                Configuration.ResourceTextAlignment = (int)DrawEnumCombo("Text Alignment", (TextAlignment)Configuration.ResourceTextAlignment);
                Configuration.ResourceTextStyle = DrawEnumCombo("Text Formatting", Configuration.ResourceTextStyle, 
                    val => $"{val.GetDescription()} ({StringFormatting.FormatDigits(10000, val)})");
                ImGui.Unindent();
            }

            ImGui.Spacing();
            SectionHeader("Scaling Options");
            
            var resourceLengthByLevel = Configuration.ResourceLengthByLevel;
            if (ImGui.Checkbox("Scale Max Length by Level##Resource", ref resourceLengthByLevel)) Configuration.ResourceLengthByLevel = resourceLengthByLevel;
            HoverTooltip("Overrides manual limits with a simulated pool based on Level.");

            ImGui.Spacing();
            ImGui.TextColored(new Vector4(0.5f, 0.8f, 1f, 1f), "MP Setup");
            var mpPerPixel = Configuration.MpPerPixelLength;
            if (ImGui.InputFloat("MP per pixel", ref mpPerPixel, 0.1f, 0.5f, "%.2f")) Configuration.MpPerPixelLength = Math.Max(0.0001f, mpPerPixel);

            var minimumMpLength = Configuration.MinimumMpLength;
            if (ImGui.InputInt("Min MP for scaling", ref minimumMpLength, 1, 25)) Configuration.MinimumMpLength = Math.Max(1, minimumMpLength);

            if (Configuration.ResourceLengthByLevel)
            {
                var mpPerLevel = Configuration.MpPerLevel;
                if (ImGui.InputInt("Simulated MP per Level", ref mpPerLevel, 10, 50)) Configuration.MpPerLevel = Math.Max(1, mpPerLevel);
            }
            else
            {
                var maximumMpLength = Configuration.MaximumMpLength;
                if (ImGui.InputInt("Max MP for scaling", ref maximumMpLength, 1, 25)) Configuration.MaximumMpLength = Math.Max(1, maximumMpLength);
            }

            var truncate = Configuration.TruncateMp;
            if (ImGui.Checkbox("Truncate MP Value (e.g. 10000 -> 100)", ref truncate)) Configuration.TruncateMp = truncate;

            ImGui.Spacing();
            ImGui.TextColored(new Vector4(0.5f, 0.8f, 1f, 1f), "GP Setup");
            var gpPerPixel = Configuration.GpPerPixelLength;
            if (ImGui.InputFloat("GP per pixel", ref gpPerPixel, 0.1f, 0.5f, "%.2f")) Configuration.GpPerPixelLength = Math.Max(0.0001f, gpPerPixel);

            var minimumGpLength = Configuration.MinimumGpLength;
            if (ImGui.InputInt("Min GP for scaling", ref minimumGpLength, 1, 25)) Configuration.MinimumGpLength = Math.Max(1, minimumGpLength);

            if (Configuration.ResourceLengthByLevel)
            {
                var gpPerLevel = Configuration.GpPerLevel;
                if (ImGui.InputInt("Simulated GP per Level", ref gpPerLevel, 1, 10)) Configuration.GpPerLevel = Math.Max(1, gpPerLevel);
            }
            else
            {
                var maximumGpLength = Configuration.MaximumGpLength;
                if (ImGui.InputInt("Max GP for scaling", ref maximumGpLength, 1, 25)) Configuration.MaximumGpLength = Math.Max(1, maximumGpLength);
            }

            ImGui.Spacing();
            ImGui.TextColored(new Vector4(0.5f, 0.8f, 1f, 1f), "CP Setup");
            var cpPerPixel = Configuration.CpPerPixelLength;
            if (ImGui.InputFloat("CP per pixel", ref cpPerPixel, 0.1f, 0.5f, "%.2f")) Configuration.CpPerPixelLength = Math.Max(0.0001f, cpPerPixel);

            var minimumCpLength = Configuration.MinimumCpLength;
            if (ImGui.InputInt("Min CP for scaling", ref minimumCpLength, 1, 25)) Configuration.MinimumCpLength = Math.Max(1, minimumCpLength);

            if (Configuration.ResourceLengthByLevel)
            {
                var cpPerLevel = Configuration.CpPerLevel;
                if (ImGui.InputInt("Simulated CP per Level", ref cpPerLevel, 1, 10)) Configuration.CpPerLevel = Math.Max(1, cpPerLevel);
            }
            else
            {
                var maximumCpLength = Configuration.MaximumCpLength;
                if (ImGui.InputInt("Max CP for scaling", ref maximumCpLength, 1, 25)) Configuration.MaximumCpLength = Math.Max(1, maximumCpLength);
            }

            ImGui.EndTabItem();
        }

        private void LimitSettings()
        {
            if (!ImGui.BeginTabItem("Limit Gauge")) return;

            var enabled = Configuration.LimitBarEnabled;
            if (ImGui.Checkbox("Enabled", ref enabled)) Configuration.LimitBarEnabled = enabled;
            
            var limitAlwaysShow = Configuration.LimitGaugeAlwaysShow;
            if (ImGui.Checkbox("Always Show", ref limitAlwaysShow)) Configuration.LimitGaugeAlwaysShow = limitAlwaysShow;
            
            var limitDiadem = Configuration.LimitGaugeDiadem;
            if (ImGui.Checkbox("Show for Diadem Compressed Aether", ref limitDiadem)) Configuration.LimitGaugeDiadem = limitDiadem;
            
            var limitPosX = Configuration.LimitGaugePositionX;
            if (ImGui.InputFloat("X Position", ref limitPosX, 1, 25)) Configuration.LimitGaugePositionX = limitPosX;
            
            var limitPosY = Configuration.LimitGaugePositionY;
            if (ImGui.InputFloat("Y Position", ref limitPosY, 1, 25)) Configuration.LimitGaugePositionY = limitPosY;

            ImGui.EndTabItem();
        }

        private void ClassSettings()
        {
            if (!ImGui.BeginTabItem("Class Info")) return;

            SectionHeader("Experience Bar");
            var expBarEnabled = Configuration.ExpBarEnabled;
            if (ImGui.Checkbox("EXP Bar Enabled", ref expBarEnabled)) Configuration.ExpBarEnabled = expBarEnabled;

            var expTextEnabled = Configuration.ExpValueTextEnabled;
            if (ImGui.Checkbox("Show EXP Value", ref expTextEnabled)) Configuration.ExpValueTextEnabled = expTextEnabled;

            if (Configuration.ExpValueTextEnabled)
            {
                ImGui.Indent();
                var expTextPos = new Vector2(Configuration.ExpValueTextPositionX, Configuration.ExpValueTextPositionY);
                if (ImGui.DragFloat2("Position (X, Y)##EXP", ref expTextPos))
                {
                    Configuration.ExpValueTextPositionX = expTextPos.X;
                    Configuration.ExpValueTextPositionY = expTextPos.Y;
                }

                var expTextSize = Configuration.ExpValueTextSize;
                if (ImGui.InputFloat("Size##EXP", ref expTextSize)) Configuration.ExpValueTextSize = expTextSize;

                Configuration.ExpValueTextAlignment = (int)DrawEnumCombo("Alignment##EXP", (TextAlignment)Configuration.ExpValueTextAlignment);
                Configuration.ExpValueTextFormatStyle = DrawEnumCombo("Formatting##EXP", Configuration.ExpValueTextFormatStyle, 
                    val => $"{val.GetDescription()} ({StringFormatting.FormatDigits(12345, val)}/{StringFormatting.FormatDigits(9999999, val)})");
                ImGui.Unindent();
            }

            ImGui.Spacing();
            SectionHeader("Level Text");
            var levelTextEnabled = Configuration.LevelEnabled;
            if (ImGui.Checkbox("Level Text Enabled", ref levelTextEnabled)) Configuration.LevelEnabled = levelTextEnabled;

            if (Configuration.LevelEnabled)
            {
                ImGui.Indent();
                var levelTextPos = new Vector2(Configuration.LevelTextX, Configuration.LevelTextY);
                if (ImGui.DragFloat2("Position (X, Y)##Level", ref levelTextPos))
                {
                    Configuration.LevelTextX = levelTextPos.X;
                    Configuration.LevelTextY = levelTextPos.Y;
                }
                
                var levelTextSize = Configuration.LevelTextSize;
                if (ImGui.InputFloat("Size##Level", ref levelTextSize)) Configuration.LevelTextSize = levelTextSize;

                Configuration.LevelTextAlignment = DrawEnumCombo("Alignment##Level", Configuration.LevelTextAlignment);
                ImGui.Unindent();
            }

            ImGui.Spacing();
            SectionHeader("Class Icon");
            var classIconEnabled = Configuration.ClassIconEnabled;
            if (ImGui.Checkbox("Class Icon Enabled", ref classIconEnabled)) Configuration.ClassIconEnabled = classIconEnabled;

            if (Configuration.ClassIconEnabled)
            {
                ImGui.Indent();
                var classIconPos = new Vector2(Configuration.ClassIconX, Configuration.ClassIconY);
                if (ImGui.DragFloat2("Position (X, Y)##Icon", ref classIconPos))
                {
                    Configuration.ClassIconX = classIconPos.X;
                    Configuration.ClassIconY = classIconPos.Y;
                }

                var scale = Configuration.ClassIconScale;
                if (ImGui.SliderFloat("Scale##Icon", ref scale, 0.1f, 3f)) Configuration.ClassIconScale = scale;
                ImGui.Unindent();
            }

            ImGui.EndTabItem();
        }

        private void PortraitSettings()
        {
            string supportedImages = "Image Files{.png,.jpg,.jpeg,.bmp}";

            if (!ImGui.BeginTabItem("Portrait")) return;

            SectionHeader("Transform & State");
            var portraitPos = new Vector2(Configuration.PortraitX, Configuration.PortraitY);
            if (ImGui.DragFloat2("Position (X, Y)", ref portraitPos))
            {
                Configuration.PortraitX = portraitPos.X;
                Configuration.PortraitY = portraitPos.Y;
            }

            var portraitScale = Configuration.PortraitScale;
            if (ImGui.DragFloat("Scale##Portrait", ref portraitScale, 0.01f, 0.1f, 10f)) Configuration.PortraitScale = portraitScale;

            var redWhenDamaged = Configuration.PortraitRedWhenDamaged;
            if (ImGui.Checkbox("Red Color Filter When Damaged", ref redWhenDamaged)) Configuration.PortraitRedWhenDamaged = redWhenDamaged;

            var redWhenDanger = Configuration.PortraitRedWhenDanger;
            if (ImGui.Checkbox("Red Color Filter on Danger Status", ref redWhenDanger)) Configuration.PortraitRedWhenDanger = redWhenDanger;

            ImGui.Spacing();
            SectionHeader("Image Paths");

            DrawPortraitPathInput("Normal", Configuration.PortraitNormalImage, path =>
            {
                Configuration.PortraitNormalImage = path;
                Portrait.SetPortraitNormal(path);
            }, supportedImages);

            DrawPortraitPathInput("Hurt", Configuration.PortraitHurtImage, path =>
            {
                Configuration.PortraitHurtImage = path;
                Portrait.SetPortraitHurt(path);
            }, supportedImages);

            DrawPortraitPathInput("Danger", Configuration.PortraitDangerImage, path =>
            {
                Configuration.PortraitDangerImage = path;
                Portrait.SetPortraitDanger(path);
            }, supportedImages);

            DrawPortraitPathInput("Combat", Configuration.PortraitCombatImage, path =>
            {
                Configuration.PortraitCombatImage = path;
                Portrait.SetPortraitCombat(path);
            }, supportedImages);

            ImGui.EndTabItem();
        }

        private void SoundSettings()
        {
            if (!ImGui.BeginTabItem("Sound")) return;
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 0.5f, 0, 1), "Notice:");
            ImGui.TextWrapped("This feature has been migrated to a dedicated plugin called 'Audible Character Status'.");
            ImGui.EndTabItem();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible) return;

            ImGui.SetNextWindowSize(new Vector2(650, 600), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Kingdom Hearts Bars Configuration", ref settingsVisible, ImGuiWindowFlags.NoCollapse))
            {
                if (ImGui.BeginTabBar("KhTabBar"))
                {
                    GeneralSettings();
                    HealthSettings();
                    ResourceSettings();
                    LimitSettings();
                    ClassSettings();
                    PortraitSettings();
                    SoundSettings();
                    ImGui.EndTabBar();
                }

                _dialogManager.Draw();
                ImGui.Separator();
                
                if (ImGui.Button("Save Configuration", new Vector2(150, 0)))
                {
                    Configuration.Save();
                }
            }
            ImGui.End();
        }

        // --- Helper Methods ---

        private void SectionHeader(string title)
        {
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            // Bullet icon for slight emphasis if desired, or skip it.
            ImGui.PopFont();
            ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1f), title);
            ImGui.Separator();
            ImGui.Spacing();
        }

        private void HoverTooltip(string message)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(message);
            }
        }

        private T DrawEnumCombo<T>(string label, T currentValue, Func<T, string>? formatDisplay = null) where T : Enum
        {
            T result = currentValue;
            string displayValue = formatDisplay != null ? formatDisplay(result) : result.ToString();
            
            if (ImGui.BeginCombo(label, displayValue))
            {
                foreach (T val in Enum.GetValues(typeof(T)))
                {
                    bool isSelected = val.Equals(currentValue);
                    string optionText = formatDisplay != null ? formatDisplay(val) : val.ToString();
                    
                    if (ImGui.Selectable(optionText, isSelected))
                    {
                        result = val;
                    }
                    if (isSelected) ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            return result;
        }

        private void DrawPortraitPathInput(string label, string currentPath, Action<string> updateAction, string supportedExtensions)
        {
            ImGui.Text($"{label} Portrait");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), FindImageMessage(currentPath));

            // Assign to a local variable to satisfy ImGui.InputText's ref requirement safely
            string tempPath = currentPath ?? string.Empty;
            ImGui.InputText($"##{label}Input", ref tempPath, 512, ImGuiInputTextFlags.ReadOnly);
            ImGui.SameLine();

            if (ImGui.Button($"Browse...##{label}Browse"))
            {
                var startDir = string.IsNullOrEmpty(currentPath) ? string.Empty : Path.GetDirectoryName(currentPath);
                void UpdatePath(bool success, List<string> paths)
                {
                    if (success && paths.Count > 0)
                    {
                        updateAction(paths[0]);
                    }
                }
                _dialogManager.OpenFileDialog($"Choose an image file for {label} Portrait", supportedExtensions, UpdatePath, 1, startDir);
            }
        }

        private string FindImageMessage(string path)
        {
            if (path.IsNullOrEmpty()) return "";
            if (!File.Exists(path)) return "File not found.";

            string[] supportedImages = { ".png", ".jpg", ".jpeg", ".bmp" };
            return supportedImages.Any(ext => Path.GetExtension(path).Equals(ext, StringComparison.OrdinalIgnoreCase)) 
                ? "" 
                : "Unsupported format (use png, jpg, jpeg, bmp).";
        }

        private FileDialogManager SetupDialogManager()
        {
            var fileManager = new FileDialogManager { AddedWindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking };
            fileManager.CustomSideBarItems.Add(("Videos", string.Empty, 0, -1));
            return fileManager;
        }
    }
}