using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Bindings.ImGui;
using KingdomHeartsPlugin.Enums;
using KingdomHeartsPlugin.UIElements.Experience;
using KingdomHeartsPlugin.UIElements.LimitBreak;
using KingdomHeartsPlugin.UIElements.ParameterResource;
using KingdomHeartsPlugin.Utilities;
using System;
using System.IO;
using System.Numerics;

namespace KingdomHeartsPlugin.UIElements.HealthBar
{
    public class HealthFrame : IDisposable
    {
        private float _verticalAnimationTicks;
        private readonly Vector3 _bgColor;
        private LimitGauge? _limitGauge;
        private ResourceBar? _resourceBar;
        private ClassBar? _expBar;

        public HealthFrame()
        {
            HealthY = 0;
            _verticalAnimationTicks = 0;
            HealthVerticalSpeed = 0f;
            LowHealthAlpha = 0;
            LowHealthAlphaDirection = 0;
            _bgColor = new Vector3(0.07843f, 0.07843f, 0.0745f);

            HealthRingBg = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"), _bgColor.X, _bgColor.Y, _bgColor.Z);
            HealthLostRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"), 1, 0, 0);
            RingOutline = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_outline_segment.png"));
            HealthRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_segment.png"));
            HealthRestoredRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_restored_segment.png"));
            _limitGauge = new LimitGauge();
            _resourceBar = new ResourceBar();
            _expBar = new ClassBar();
        }

        public unsafe void Draw()
        {
            var player = KingdomHeartsPlugin.Ot.LocalPlayer;
            var parameterWidget = (AtkUnitBase*)KingdomHeartsPlugin.Gui.GetAddonByName("_ParameterWidget", 1).Address;

            if (parameterWidget != null && !parameterWidget->IsVisible)
            {
                return;
            }

            if (player is null || KingdomHeartsPlugin.Gui.GameUiHidden)
            {
                return;
            }

            var drawList = ImGui.GetWindowDrawList();

            if (ImGui.GetDrawListSharedData().IsNull) return;

            ImGui.Dummy(new Vector2(220, 256));

            if (KingdomHeartsPlugin.Ui.Configuration.HpBarEnabled)
            {
                byte role = player.ClassJob.ValueNullable?.Role ?? 0;
                uint jobId = player.ClassJob.ValueNullable?.RowId ?? 0;
                UpdateHealth(player);
                DrawHealth(drawList, player.CurrentHp, player.MaxHp, player.Level, jobId, role);
            }

            if (KingdomHeartsPlugin.Ui.Configuration.ResourceBarEnabled) _resourceBar?.Draw(player);
            if (KingdomHeartsPlugin.Ui.Configuration.LimitBarEnabled) _limitGauge?.Draw();
            _expBar?.Draw(player, HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f);

            if (KingdomHeartsPlugin.Ui.Configuration.ShowHpVal && KingdomHeartsPlugin.Ui.Configuration.HpBarEnabled)
            {
                var hpText = StringFormatting.FormatDigits(player.CurrentHp, (NumberFormatStyle)KingdomHeartsPlugin.Ui.Configuration.HpValueTextStyle);
                var rawPosition = ImGui.GetItemRectMin() + new Vector2(KingdomHeartsPlugin.Ui.Configuration.HpValueTextPositionX, KingdomHeartsPlugin.Ui.Configuration.HpValueTextPositionY) * KingdomHeartsPlugin.Ui.Configuration.Scale;
                var basePosition = new Vector2((float)Math.Round(rawPosition.X), (float)Math.Round(rawPosition.Y));

                var fontHandle = KingdomHeartsPlugin.HpFont;

                // Check if the font handle is loaded and ready
                if (fontHandle != null && fontHandle.Available)
                {
                    // Push the custom font onto the ImGui stack
                    using (fontHandle.Push())
                    {
                        // Calculate scale down factor relative to the 40pt baked size
                        float targetScale = (KingdomHeartsPlugin.Ui.Configuration.HpValueTextSize * KingdomHeartsPlugin.Ui.Configuration.Scale) / 40.0f;
                        ImGui.SetWindowFontScale(targetScale);

                        uint textColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
                        uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.65f));

                        float offset = 2f * KingdomHeartsPlugin.Ui.Configuration.Scale;

                        // Draw shadow/outline
                        drawList.AddText(basePosition + new Vector2(offset, offset), shadowColor, hpText);

                        // Draw main text
                        drawList.AddText(basePosition, textColor, hpText);

                        // Reset font scale
                        ImGui.SetWindowFontScale(1.0f);
                    }
                }
                else
                {
                    // Fallback drawing routine while font loads
                    ImGuiAdditions.TextShadowedDrawList(drawList,
                        KingdomHeartsPlugin.Ui.Configuration.HpValueTextSize,
                        hpText,
                        basePosition,
                        new Vector4(1f, 1f, 1f, 1f),
                        new Vector4(0f, 0f, 0f, 0.25f), 3, (TextAlignment)KingdomHeartsPlugin.Ui.Configuration.HpValueTextAlignment);
                }
            }
        }


        private void UpdateHealth(IPlayerCharacter player)
        {
            // Initialization catch to prevent massive drops when loading in
            if (LastHp == 0 && player.CurrentHp > 0)
            {
                LastHp = player.CurrentHp;
                SmoothCurrentHp = player.CurrentHp;
                HpBeforeDamaged = player.CurrentHp;
                HpTemp = player.CurrentHp;
                return; 
            }

            if (player.CurrentHp != LastHp)
            {
                if (player.CurrentHp < LastHp)
                {
                    uint damageAmount = LastHp - player.CurrentHp;
                    DamagedHealth(damageAmount, player.MaxHp);

                    // Only apply delay if the damaged bar is not currently actively animating
                    bool isDamageAnimating = HpBeforeDamaged > LastHp && _damageAnimationTimer <= 0;
                    if (!isDamageAnimating)
                    {
                        _damageAnimationTimer = KingdomHeartsPlugin.Ui.Configuration.HpAnimationDelay;
                    }

                    // Ensure damaged visual continues from the highest recent point
                    HpBeforeDamaged = Math.Max(HpBeforeDamaged, LastHp);
                }
                else if (player.CurrentHp > LastHp)
                {
                    // Only apply delay if the restored bar is not currently actively animating
                    bool isHealAnimating = HpTemp < LastHp && _healAnimationTimer <= 0;
                    if (!isHealAnimating)
                    {
                        _healAnimationTimer = KingdomHeartsPlugin.Ui.Configuration.HpAnimationDelay;
                    }

                    // Ensure restore visual starts/continues from the lowest recent point (No Skipping)
                    HpTemp = Math.Min(HpTemp, LastHp);
                }
            }

            UpdateLowHealth(player.CurrentHp, player.MaxHp);
            UpdateDamagedHealth(); // Handles wobble physics
            UpdateHpAnimations(player.CurrentHp, player.MaxHp); // Handles smooth interpolation for current, damaged, and restored HP

            LastHp = player.CurrentHp;
        }

        private void DamagedHealth(uint damageAmount, uint maxHp)
        {
            float damagePercent = Math.Clamp((float)damageAmount / maxHp, 0.01f, 1f);

            // Wobble physics injection
            float impactForce = -4f - (damagePercent * 25f);
            HealthVerticalSpeed += impactForce;

            // Trigger the red flash effect
            _flashAlpha = 1.0f;
        }

        private void UpdateDamagedHealth()
        {
            _verticalAnimationTicks += 240 * KingdomHeartsPlugin.UiSpeed;

            const float springStiffness = 0.12f;
            const float springDamping = 0.90f;

            while (_verticalAnimationTicks > 1)
            {
                _verticalAnimationTicks--;

                float springForce = -springStiffness * HealthY;
                HealthVerticalSpeed += springForce;
                HealthVerticalSpeed *= springDamping;
                HealthY += HealthVerticalSpeed;

                if (Math.Abs(HealthY) < 0.1f && Math.Abs(HealthVerticalSpeed) < 0.1f)
                {
                    HealthY = 0;
                    HealthVerticalSpeed = 0;
                    _verticalAnimationTicks = 0; 
                    break;
                }
            }
        }

        private void UpdateHpAnimations(uint currentHp, uint maxHp)
        {
            // The restored health instantly snaps to the new value instead of interpolating
            SmoothCurrentHp = currentHp;

            // Prevent visuals from overlapping incorrectly by keeping Temp values clamped to SmoothCurrentHp
            // Snap instantly if damaged health exceeds max health bounds
            if (HpBeforeDamaged > maxHp)
            {
                HpBeforeDamaged = currentHp;
            }

            if (HpTemp > SmoothCurrentHp)
            {
                HpTemp = SmoothCurrentHp;
            }
            if (HpBeforeDamaged < SmoothCurrentHp)
            {
                HpBeforeDamaged = SmoothCurrentHp;
            }

            // Linear step based on max HP and user animation speed
            float linearStep = maxHp * (KingdomHeartsPlugin.Ui.Configuration.HpAnimationSpeed * 0.015f) * KingdomHeartsPlugin.UiSpeed;
            // Ensure a minimum movement speed
            linearStep = Math.Max(linearStep, 1f);

            // Independent damage timer and drain animation
            if (_damageAnimationTimer > 0)
            {
                _damageAnimationTimer -= KingdomHeartsPlugin.UiSpeed;
            }
            else if (HpBeforeDamaged > currentHp)
            {
                HpBeforeDamaged -= linearStep;
                if (HpBeforeDamaged < currentHp) HpBeforeDamaged = currentHp;
            }

            // Independent heal timer and fill animation (current health still interpolates)
            if (_healAnimationTimer > 0)
            {
                _healAnimationTimer -= KingdomHeartsPlugin.UiSpeed;
            }
            else if (HpTemp < currentHp)
            {
                HpTemp += linearStep;
                if (HpTemp > currentHp) HpTemp = currentHp;
            }

            // Alpha handling: keep solid while waiting/draining, fade out when done
            if (HpBeforeDamaged > currentHp)
            {
                DamagedHealthAlpha = 1f;
            }
            else if (DamagedHealthAlpha > 0)
            {
                DamagedHealthAlpha -= 1.5f * KingdomHeartsPlugin.UiSpeed;
                if (DamagedHealthAlpha < 0) DamagedHealthAlpha = 0;
            }

            // Fade out the flash effect
            if (_flashAlpha > 0)
            {
                _flashAlpha -= 3.0f * KingdomHeartsPlugin.UiSpeed;
                if (_flashAlpha < 0) _flashAlpha = 0;
            }
        }

        private void UpdateLowHealth(uint health, uint maxHealth)
        {
            if ((health > maxHealth * (KingdomHeartsPlugin.Ui.Configuration.LowHpPercent / 100f) || health <= 0) && LowHealthAlpha <= 0) return;

            if (LowHealthAlphaDirection == 0)
            {
                LowHealthAlpha += 1.6f * KingdomHeartsPlugin.UiSpeed;

                if (LowHealthAlpha >= .4)
                    LowHealthAlphaDirection = 1;
            }
            else
            {
                LowHealthAlpha -= 1.6f * KingdomHeartsPlugin.UiSpeed;

                if (LowHealthAlpha <= 0)
                    LowHealthAlphaDirection = 0;
            }

            if (HealthRingBg is not null)
                HealthRingBg.Color = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);

        }

        private float GetHpMultiplier(uint jobId, byte role)
        {
            var config = KingdomHeartsPlugin.Ui.Configuration;
            if (!config.EnableRoleHpMultipliers) return 1.0f;

            return role switch
            {
                1 => config.EnableTankJobHpMultipliers ? jobId switch
                {
                    19 => config.PldHpMultiplier,
                    21 => config.WarHpMultiplier,
                    32 => config.DrkHpMultiplier,
                    37 => config.GnbHpMultiplier,
                    _ => config.TankHpMultiplier
                } : config.TankHpMultiplier,

                2 => config.EnableMeleeJobHpMultipliers ? jobId switch
                {
                    20 => config.MnkHpMultiplier,
                    22 => config.DrgHpMultiplier,
                    30 => config.NinHpMultiplier,
                    34 => config.SamHpMultiplier,
                    39 => config.RprHpMultiplier,
                    41 => config.VprHpMultiplier,
                    43 => config.BstHpMultiplier,
                    _ => config.MeleeHpMultiplier
                } : config.MeleeHpMultiplier,

                3 => config.EnableRangedJobHpMultipliers ? jobId switch
                {
                    23 => config.BrdHpMultiplier,
                    31 => config.MchHpMultiplier,
                    38 => config.DncHpMultiplier,
                    25 => config.BlmHpMultiplier,
                    27 => config.SmnHpMultiplier,
                    35 => config.RdmHpMultiplier,
                    42 => config.PctHpMultiplier,
                    36 => config.BluHpMultiplier,
                    _ => config.RangedHpMultiplier
                } : config.RangedHpMultiplier,

                4 => config.EnableHealerJobHpMultipliers ? jobId switch
                {
                    24 => config.WhmHpMultiplier,
                    28 => config.SchHpMultiplier,
                    33 => config.AstHpMultiplier,
                    40 => config.SgeHpMultiplier,
                    _ => config.HealerHpMultiplier
                } : config.HealerHpMultiplier,

                _ => config.OtherHpMultiplier
            };
        }

        private void DrawHealth(ImDrawListPtr drawList, uint hp, uint maxHp, byte level, uint jobId, byte role)
        {
            var fullRing = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpHpForFullRing : KingdomHeartsPlugin.Ui.Configuration.HpForFullRing;
            var minimumMaxHpSize = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpMinimumHpForLength : KingdomHeartsPlugin.Ui.Configuration.MinimumHpForLength;
            var maximumMaxHpSize = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpMaximumHpForMaximumLength : KingdomHeartsPlugin.Ui.Configuration.MaximumHpForMaximumLength;

            var isLevelBased = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpLengthByLevel : KingdomHeartsPlugin.Ui.Configuration.LengthByLevel;
            var hpPerLevel = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpHpPerLevel : KingdomHeartsPlugin.Ui.Configuration.HpPerLevel;
            var ignoreRingForLevelScaling = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpIgnoreRingForLevelScaling : KingdomHeartsPlugin.Ui.Configuration.IgnoreRingForLevelScaling;

            float roleMultiplier = GetHpMultiplier(jobId, role);

            if (isLevelBased)
            {
                float simulatedHp = (minimumMaxHpSize + (level * hpPerLevel)) * roleMultiplier;
                HpLengthMultiplier = simulatedHp / (float)maxHp;
            }
            else
            {
                float baseMultiplier = maxHp < minimumMaxHpSize
                    ? minimumMaxHpSize / (float)maxHp
                    : maxHp > maximumMaxHpSize
                        ? (float)maximumMaxHpSize / maxHp
                        : 1f;

                HpLengthMultiplier = baseMultiplier * roleMultiplier;
            }

            bool scaleBeforeRing = !isLevelBased || ignoreRingForLevelScaling;
            float GetScaledHp(float hpValue) => scaleBeforeRing ? hpValue * HpLengthMultiplier : hpValue;

            var drawPosition = ImGui.GetItemRectMin();
            
            var maxHealthPercent = GetScaledHp(maxHp) / (float)fullRing; 

            try
            {
                DrawRingEdgesAndTrack(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)));
            }
            catch
            {
                return;
            }

            HealthRingBg?.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

            if (DamagedHealthAlpha > 0)
            {
                if (HealthLostRing is not null)
                {
                    HealthLostRing.Alpha = DamagedHealthAlpha;
                    HealthLostRing.Draw(drawList, GetScaledHp(HpBeforeDamaged) / (float)fullRing, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
                }
            }

            if (KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery)
            {
                if (HpTemp < SmoothCurrentHp)
                    HealthRestoredRing?.Draw(drawList, GetScaledHp(SmoothCurrentHp) / (float)fullRing, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

                HealthRing?.Draw(drawList, GetScaledHp(HpTemp) / (float)fullRing, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }
            else
            {
                HealthRing?.Draw(drawList, GetScaledHp(SmoothCurrentHp) / (float)fullRing, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }

            // Add Flash effect for the ring
            if (_flashAlpha > 0 && HealthLostRing is not null)
            {
                float currentHpLength = GetScaledHp(KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? HpTemp : SmoothCurrentHp) / (float)fullRing;
                HealthLostRing.Alpha = _flashAlpha;
                HealthLostRing.Draw(drawList, currentHpLength, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }

            RingOutline?.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            DrawLongHealthBar(drawList, hp, maxHp, scaleBeforeRing);
        }

        private void DrawLongHealthBar(ImDrawListPtr drawList, uint hp, uint maxHp, bool scaleBeforeRing)
        {
            var fullRing = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpHpForFullRing : KingdomHeartsPlugin.Ui.Configuration.HpForFullRing;
            var HpPerWidth = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpHpPerPixelLongBar : KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar;
            var basePosition = new Vector2(129, 212 + HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f);
            
            float GetScaledHp(float hpValue) => scaleBeforeRing ? hpValue * HpLengthMultiplier : hpValue;

            float actualCurrentHp = KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? HpTemp : SmoothCurrentHp;
            var healthLength = scaleBeforeRing
                ? Math.Max(0, GetScaledHp(actualCurrentHp) - fullRing) / HpPerWidth
                : Math.Max(0, actualCurrentHp - fullRing) * HpLengthMultiplier / HpPerWidth;
            
            var damagedHealthLength = scaleBeforeRing
                ? Math.Max(0, GetScaledHp(HpBeforeDamaged) - fullRing) / HpPerWidth
                : Math.Max(0, HpBeforeDamaged - fullRing) * HpLengthMultiplier / HpPerWidth;
            
            float actualRestoredHp = KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery && HpTemp < SmoothCurrentHp ? SmoothCurrentHp : 0;
            var restoredHealthLength = scaleBeforeRing
                ? Math.Max(0, GetScaledHp(actualRestoredHp) - fullRing) / HpPerWidth
                : Math.Max(0, actualRestoredHp - fullRing) * HpLengthMultiplier / HpPerWidth;
            
            var maxHealthLength = scaleBeforeRing
                ? Math.Max(0, GetScaledHp(maxHp) - fullRing) / HpPerWidth
                : Math.Max(0, maxHp - fullRing) * HpLengthMultiplier / HpPerWidth;
            
            if (maxHealthLength > 0)
            {
                Vector3 lowHealthColor = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);
                ImageDrawing.DrawImage(drawList, BarEdgeTexture, new Vector2(basePosition.X - 5.4f - maxHealthLength, basePosition.Y));
                ImageDrawing.DrawImageScaled(drawList, BarColorlessTexture, new Vector2(basePosition.X - maxHealthLength, basePosition.Y + 4), new Vector2(maxHealthLength, 1), ImGui.GetColorU32(new Vector4(lowHealthColor.X, lowHealthColor.Y, lowHealthColor.Z, 1)));
            }

            if (damagedHealthLength > 0)
            {
                ImageDrawing.DrawImageScaled(drawList, BarColorlessTexture, new Vector2(basePosition.X - damagedHealthLength, basePosition.Y + 4), new Vector2(damagedHealthLength, 1), ImGui.GetColorU32(new Vector4(1f, 0f, 0f, DamagedHealthAlpha)));
            }

            if (restoredHealthLength > 0)
            {
                ImageDrawing.DrawImageScaled(drawList, BarRecoveryTexture, new Vector2(basePosition.X - restoredHealthLength, basePosition.Y + 4), new Vector2(restoredHealthLength, 1));
            }

            if (healthLength > 0)
            {
                // Normal green/foreground bar
                ImageDrawing.DrawImageScaled(drawList, BarForegroundTexture, new Vector2(basePosition.X - healthLength, basePosition.Y + 4), new Vector2(healthLength, 1));

                // Flash overlay for the long bar
                if (_flashAlpha > 0)
                {
                    ImageDrawing.DrawImageScaled(drawList, BarColorlessTexture, new Vector2(basePosition.X - healthLength, basePosition.Y + 4), new Vector2(healthLength, 1), ImGui.GetColorU32(new Vector4(1f, 0f, 0f, _flashAlpha)));
                }
            }

            if (maxHealthLength > 0)
            {
                ImageDrawing.DrawImageScaled(drawList, BarOutlineTexture, new Vector2(basePosition.X - maxHealthLength, basePosition.Y), new Vector2(maxHealthLength, 1));
            }
        }

        private void DrawRingEdgesAndTrack(ImDrawListPtr drawList, float percent, Vector2 position)
        {
            var size = 256 * KingdomHeartsPlugin.Ui.Configuration.Scale;

            drawList.PushClipRect(position, position + new Vector2(size, size));
            drawList.AddImage(RingTrackTexture.GetWrapOrEmpty().Handle, position, position + new Vector2(size, size));
            drawList.AddImage(RingBaseTexture.GetWrapOrEmpty().Handle, position, position + new Vector2(size, size));
            ImageDrawing.ImageRotated(drawList, RingEndTexture.GetWrapOrEmpty().Handle, new Vector2(position.X + size / 2f, position.Y + size / 2f), new Vector2(RingEndTexture.GetWrapOrEmpty().Width * KingdomHeartsPlugin.Ui.Configuration.Scale, RingEndTexture.GetWrapOrEmpty().Height * KingdomHeartsPlugin.Ui.Configuration.Scale), Math.Min(percent, 1) * 0.75f * (float)Math.PI * 2);
            drawList.PopClipRect();
        }

        public void Dispose()
        {
            _limitGauge?.Dispose();
            _resourceBar?.Dispose();
            _expBar?.Dispose();

            _limitGauge = null;
            _resourceBar = null;
            _expBar = null;
            HealthRing = null;
            HealthRingBg = null;
            RingOutline = null;
            HealthRestoredRing = null;
            HealthLostRing = null;
        }

        // Temp Health Values
        private uint LastHp { get; set; }
        private float SmoothCurrentHp { get; set; }
        private float HpBeforeDamaged { get; set; }
        private float HpTemp { get; set; }
        private float HpLengthMultiplier { get; set; }

        // Alpha Channels
        public float DamagedHealthAlpha { get; private set; }
        public float LowHealthAlpha { get; private set; }
        private int LowHealthAlphaDirection { get; set; }
        private float _flashAlpha { get; set; }

        // Timers
        private float _damageAnimationTimer { get; set; }
        private float _healAnimationTimer { get; set; }

        // Positioning
        private float HealthY { get; set; }
        private float HealthVerticalSpeed { get; set; }

        // Textures
        private ISharedImmediateTexture HealthRingSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_segment.png"));
        }
        private ISharedImmediateTexture HealthRestoredRingSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_restored_segment.png"));
        }
        private ISharedImmediateTexture BarOutlineTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_outline.png"));
        }
        private ISharedImmediateTexture BarColorlessTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_colorless.png"));
        }
        private ISharedImmediateTexture BarForegroundTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_foreground.png"));
        }
        private ISharedImmediateTexture BarRecoveryTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_recovery.png"));
        }
        private ISharedImmediateTexture BarEdgeTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_edge.png"));
        }
        private ISharedImmediateTexture RingValueSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"));
        }
        private ISharedImmediateTexture RingOutlineTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_outline_segment.png"));
        }
        private ISharedImmediateTexture RingTrackTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_track.png"));
        }
        private ISharedImmediateTexture RingBaseTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_base_edge.png"));
        }
        private ISharedImmediateTexture RingEndTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_end_edge.png"));
        }

        // Rings
        private Ring? HealthRing { get; set; }
        private Ring? RingOutline { get; set; }
        private Ring? HealthRingBg { get; set; }
        private Ring? HealthRestoredRing { get; set; }
        private Ring? HealthLostRing { get; set; }
    }
}