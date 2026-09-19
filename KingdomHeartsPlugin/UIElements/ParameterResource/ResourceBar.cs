using System;
using System.IO;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Textures;
using Dalamud.Bindings.ImGui;
using KingdomHeartsPlugin.Enums;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.UIElements.ParameterResource
{
    public class ResourceBar
    {
        private ISharedImmediateTexture _barBackgroundTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\ResourceBar\background.png"));
        }
        private ISharedImmediateTexture _barForegroundTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\ResourceBar\foreground.png"));
        }
        private ISharedImmediateTexture _mpBaseTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\ResourceBar\MP_base.png"));
        }
        private ISharedImmediateTexture _barEdgeTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\ResourceBar\edge.png"));
        }

        private enum Resource
        {
            Mp,
            Cp,
            Gp
        }

        public ResourceBar()
        {
        }

        public void Update(IPlayerCharacter player)
        {
            var minLength = 1;
            var maxLength = 1;
            var lengthRate = 1f;

            if (player.MaxMp > 0)
            {
                ResourceValue = player.CurrentMp;
                ResourceMax = player.MaxMp;
                ResourceType = Resource.Mp;

                minLength = KingdomHeartsPlugin.Ui.Configuration.MinimumMpLength;
                maxLength = KingdomHeartsPlugin.Ui.Configuration.MaximumMpLength;
                lengthRate = KingdomHeartsPlugin.Ui.Configuration.MpPerPixelLength;
            }
            else if (player.MaxCp > 0)
            {
                ResourceValue = player.CurrentCp;
                ResourceMax = player.MaxCp;
                ResourceType = Resource.Cp;

                minLength = KingdomHeartsPlugin.Ui.Configuration.MinimumCpLength;
                maxLength = KingdomHeartsPlugin.Ui.Configuration.MaximumCpLength;
                lengthRate = KingdomHeartsPlugin.Ui.Configuration.CpPerPixelLength;
            }
            else if (player.MaxGp > 0)
            {
                ResourceValue = player.CurrentGp;
                ResourceMax = player.MaxGp;
                ResourceType = Resource.Gp;

                minLength = KingdomHeartsPlugin.Ui.Configuration.MinimumGpLength;
                maxLength = KingdomHeartsPlugin.Ui.Configuration.MaximumGpLength;
                lengthRate = KingdomHeartsPlugin.Ui.Configuration.GpPerPixelLength;
            }

            // Initialization catch for animations
            if (LastResource == 0 && ResourceValue > 0)
            {
                LastResource = ResourceValue;
                SmoothCurrentResource = ResourceValue;
                ResourceBeforeSpent = ResourceValue;
                ResourceTemp = ResourceValue;
            }

            if (ResourceValue != LastResource)
            {
                _resourceAnimationTimer = KingdomHeartsPlugin.Ui.Configuration.ResourceAnimationDelay;

                if (ResourceValue < LastResource)
                {
                    ResourceBeforeSpent = Math.Max(ResourceBeforeSpent, LastResource);
                }
                else if (ResourceValue > LastResource)
                {
                    ResourceTemp = Math.Min(ResourceTemp, LastResource);
                }
            }

            UpdateResourceAnimations(ResourceValue, ResourceMax);
            LastResource = ResourceValue;

            float roleMultiplier = 1f;
            if (KingdomHeartsPlugin.Ui.Configuration.EnableRoleResourceMultipliers)
            {
                var role = player.ClassJob.Value.Role;
                var jobAbbr = player.ClassJob.Value.Abbreviation.ToString();

                if (KingdomHeartsPlugin.Ui.Configuration.EnableTankJobResourceMultipliers && jobAbbr is "PLD" or "WAR" or "DRK" or "GNB")
                {
                    roleMultiplier = jobAbbr switch
                    {
                        "PLD" => KingdomHeartsPlugin.Ui.Configuration.PldResourceMultiplier,
                        "WAR" => KingdomHeartsPlugin.Ui.Configuration.WarResourceMultiplier,
                        "DRK" => KingdomHeartsPlugin.Ui.Configuration.DrkResourceMultiplier,
                        "GNB" => KingdomHeartsPlugin.Ui.Configuration.GnbResourceMultiplier,
                        _ => 1f
                    };
                }
                else if (KingdomHeartsPlugin.Ui.Configuration.EnableHealerJobResourceMultipliers && jobAbbr is "WHM" or "SCH" or "AST" or "SGE")
                {
                    roleMultiplier = jobAbbr switch
                    {
                        "WHM" => KingdomHeartsPlugin.Ui.Configuration.WhmResourceMultiplier,
                        "SCH" => KingdomHeartsPlugin.Ui.Configuration.SchResourceMultiplier,
                        "AST" => KingdomHeartsPlugin.Ui.Configuration.AstResourceMultiplier,
                        "SGE" => KingdomHeartsPlugin.Ui.Configuration.SgeResourceMultiplier,
                        _ => 1f
                    };
                }
                else if (KingdomHeartsPlugin.Ui.Configuration.EnableMeleeJobResourceMultipliers && jobAbbr is "MNK" or "DRG" or "NIN" or "SAM" or "RPR" or "VPR" or "BST")
                {
                    roleMultiplier = jobAbbr switch
                    {
                        "MNK" => KingdomHeartsPlugin.Ui.Configuration.MnkResourceMultiplier,
                        "DRG" => KingdomHeartsPlugin.Ui.Configuration.DrgResourceMultiplier,
                        "NIN" => KingdomHeartsPlugin.Ui.Configuration.NinResourceMultiplier,
                        "SAM" => KingdomHeartsPlugin.Ui.Configuration.SamResourceMultiplier,
                        "RPR" => KingdomHeartsPlugin.Ui.Configuration.RprResourceMultiplier,
                        "VPR" => KingdomHeartsPlugin.Ui.Configuration.VprResourceMultiplier,
                        "BST" => KingdomHeartsPlugin.Ui.Configuration.BstResourceMultiplier,
                        _ => 1f
                    };
                }
                else if (KingdomHeartsPlugin.Ui.Configuration.EnableRangedJobResourceMultipliers && jobAbbr is "BRD" or "MCH" or "DNC" or "BLM" or "SMN" or "RDM" or "PCT" or "BLU")
                {
                    roleMultiplier = jobAbbr switch
                    {
                        "BRD" => KingdomHeartsPlugin.Ui.Configuration.BrdResourceMultiplier,
                        "MCH" => KingdomHeartsPlugin.Ui.Configuration.MchResourceMultiplier,
                        "DNC" => KingdomHeartsPlugin.Ui.Configuration.DncResourceMultiplier,
                        "BLM" => KingdomHeartsPlugin.Ui.Configuration.BlmResourceMultiplier,
                        "SMN" => KingdomHeartsPlugin.Ui.Configuration.SmnResourceMultiplier,
                        "RDM" => KingdomHeartsPlugin.Ui.Configuration.RdmResourceMultiplier,
                        "PCT" => KingdomHeartsPlugin.Ui.Configuration.PctResourceMultiplier,
                        "BLU" => KingdomHeartsPlugin.Ui.Configuration.BluResourceMultiplier,
                        _ => 1f
                    };
                }
                else
                {
                    roleMultiplier = role switch
                    {
                        1 => KingdomHeartsPlugin.Ui.Configuration.TankResourceMultiplier,
                        2 => KingdomHeartsPlugin.Ui.Configuration.MeleeResourceMultiplier,
                        3 => KingdomHeartsPlugin.Ui.Configuration.RangedResourceMultiplier,
                        4 => KingdomHeartsPlugin.Ui.Configuration.HealerResourceMultiplier,
                        _ => KingdomHeartsPlugin.Ui.Configuration.OtherResourceMultiplier
                    };
                }
            }

            float lengthMultiplier;
            if (KingdomHeartsPlugin.Ui.Configuration.ResourceLengthByLevel)
            {
                int resourcePerLevel = ResourceType switch
                {
                    Resource.Mp => KingdomHeartsPlugin.Ui.Configuration.MpPerLevel,
                    Resource.Cp => KingdomHeartsPlugin.Ui.Configuration.CpPerLevel,
                    Resource.Gp => KingdomHeartsPlugin.Ui.Configuration.GpPerLevel,
                    _ => 0
                };

                float simulatedMax = minLength + (player.Level * resourcePerLevel);
                lengthMultiplier = simulatedMax / (float)ResourceMax;
            }
            else
            {
                lengthMultiplier = ResourceMax < minLength
                    ? minLength / (float)ResourceMax
                    : ResourceMax > maxLength
                        ? (float)maxLength / ResourceMax
                        : 1f;
            }
            
            lengthMultiplier *= roleMultiplier;

            MaxResourceLength = (float)Math.Ceiling(ResourceMax / lengthRate * lengthMultiplier);
            ResourceLength = (float)Math.Ceiling(SmoothCurrentResource / lengthRate * lengthMultiplier);
            SpentResourceLength = (float)Math.Ceiling(ResourceBeforeSpent / lengthRate * lengthMultiplier);
            TempResourceLength = (float)Math.Ceiling(ResourceTemp / lengthRate * lengthMultiplier);
        }

        private void UpdateResourceAnimations(uint currentResource, uint maxResource)
        {
            if (SmoothCurrentResource < currentResource)
                SmoothCurrentResource = currentResource; // Instantly snaps on restored
            else if (SmoothCurrentResource > currentResource)
                SmoothCurrentResource = currentResource;

            if (ResourceBeforeSpent > maxResource) ResourceBeforeSpent = currentResource;
            if (ResourceTemp > SmoothCurrentResource) ResourceTemp = SmoothCurrentResource;
            if (ResourceBeforeSpent < SmoothCurrentResource) ResourceBeforeSpent = SmoothCurrentResource;

            if (_resourceAnimationTimer > 0)
            {
                _resourceAnimationTimer -= KingdomHeartsPlugin.UiSpeed;
            }
            else
            {
                float linearStep = maxResource * (KingdomHeartsPlugin.Ui.Configuration.ResourceAnimationSpeed * 0.015f) * KingdomHeartsPlugin.UiSpeed;
                linearStep = Math.Max(linearStep, 1f);

                if (ResourceBeforeSpent > currentResource)
                {
                    ResourceBeforeSpent -= linearStep;
                    if (ResourceBeforeSpent < currentResource) ResourceBeforeSpent = currentResource;
                }
                if (ResourceTemp < currentResource) // Slowly catch up
                {
                    ResourceTemp += linearStep;
                    if (ResourceTemp > currentResource) ResourceTemp = currentResource;
                }
            }

            if (ResourceBeforeSpent > currentResource)
            {
                SpentResourceAlpha = 1f;
            }
            else if (SpentResourceAlpha > 0)
            {
                SpentResourceAlpha -= 1.5f * KingdomHeartsPlugin.UiSpeed;
                if (SpentResourceAlpha < 0) SpentResourceAlpha = 0;
            }
        }

        public void Draw(IPlayerCharacter player)
        {
            Update(player);
            var drawList = ImGui.GetWindowDrawList();
            var basePosition =  new Vector2(KingdomHeartsPlugin.Ui.Configuration.ResourceBarPositionX, KingdomHeartsPlugin.Ui.Configuration.ResourceBarPositionY);
            var textPosition = new Vector2(KingdomHeartsPlugin.Ui.Configuration.ResourceTextPositionX, KingdomHeartsPlugin.Ui.Configuration.ResourceTextPositionY) * KingdomHeartsPlugin.Ui.Configuration.Scale;

            // Base
            ImageDrawing.DrawImage(drawList, _mpBaseTexture, new Vector2(basePosition.X - 1, basePosition.Y), new Vector4(0, 0, 74 / 80f, 1));

            // BG
            ImageDrawing.DrawImageScaled(drawList, _barBackgroundTexture, new Vector2(basePosition.X + 0.33f - MaxResourceLength, basePosition.Y), new Vector2(MaxResourceLength, 1f));

            // Spent Resource Trail (Red Tint)
            if (SpentResourceLength > 0 && SpentResourceAlpha > 0)
            {
                var spentColor = KingdomHeartsPlugin.Ui.Configuration.ResourceSpentColor;
                ImageDrawing.DrawImageScaled(drawList, _barForegroundTexture, new Vector2(basePosition.X + 0.33f - SpentResourceLength, basePosition.Y + 5), new Vector2(SpentResourceLength, 1f), ImGui.GetColorU32(new Vector4(spentColor.X, spentColor.Y, spentColor.Z, spentColor.W * SpentResourceAlpha)));
            }

            // Recovery Trail (Cyan Tint)
            if (KingdomHeartsPlugin.Ui.Configuration.ShowResourceRecovery && ResourceTemp < SmoothCurrentResource)
            {
                ImageDrawing.DrawImageScaled(drawList, _barForegroundTexture, new Vector2(basePosition.X + 0.33f - ResourceLength, basePosition.Y + 5), new Vector2(ResourceLength, 1f), ImGui.GetColorU32(KingdomHeartsPlugin.Ui.Configuration.ResourceRecoveredColor)); 
            }

            // FG (Active Resource Length)
            float fgLength = KingdomHeartsPlugin.Ui.Configuration.ShowResourceRecovery ? TempResourceLength : ResourceLength;
            if (fgLength > 0)
            {
                ImageDrawing.DrawImageScaled(drawList, _barForegroundTexture, new Vector2(basePosition.X + 0.33f - fgLength, basePosition.Y + 5), new Vector2(fgLength, 1f));
            }

            // Edge
            ImageDrawing.DrawImage(drawList, _barEdgeTexture, new Vector2(basePosition.X + 0.65f - MaxResourceLength - 6, basePosition.Y));
            
            // Base Edge
            ImageDrawing.DrawImageRotated(drawList, _barEdgeTexture, new Vector2(basePosition.X + 74, basePosition.Y + 16), new Vector2(_barEdgeTexture.GetWrapOrEmpty().Width, _barEdgeTexture.GetWrapOrEmpty().Height), (float)Math.PI);

            if (KingdomHeartsPlugin.Ui.Configuration.ShowResourceVal)
                ImGuiAdditions.TextShadowedDrawList(drawList, KingdomHeartsPlugin.Ui.Configuration.ResourceTextSize, $"{StringFormatting.FormatDigits(KingdomHeartsPlugin.Ui.Configuration.TruncateMp && ResourceType == Resource.Mp ? ResourceValue / 100 : ResourceValue, KingdomHeartsPlugin.Ui.Configuration.ResourceTextStyle)}", ImGui.GetItemRectMin() + basePosition * KingdomHeartsPlugin.Ui.Configuration.Scale + textPosition, new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f), new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3, (TextAlignment)KingdomHeartsPlugin.Ui.Configuration.ResourceTextAlignment);
         }

        public void Dispose()
        {
        }

        private uint ResourceValue { get; set; }
        private Resource ResourceType { get; set; }
        private uint ResourceMax { get; set; }
        private float ResourceLength { get; set; }
        private float MaxResourceLength { get; set; }

        // Animation Properties
        private uint LastResource { get; set; }
        private float SmoothCurrentResource { get; set; }
        private float ResourceBeforeSpent { get; set; }
        private float ResourceTemp { get; set; }
        private float SpentResourceLength { get; set; }
        private float TempResourceLength { get; set; }
        private float _resourceAnimationTimer { get; set; }
        public float SpentResourceAlpha { get; private set; }
    }
}