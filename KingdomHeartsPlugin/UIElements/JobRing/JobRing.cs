using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Bindings.ImGui;

namespace KingdomHeartsPlugin.UIElements.JobRingNS
{
    public class JobRing : IDisposable
    {
        public JobRing()
        {
        }

        public void Draw(IPlayerCharacter player, float scale, float wobbledY)
        {
            var config = KingdomHeartsPlugin.Ui.Configuration;
            if (!config.JobRingEnabled) return;

            var jobId = player.ClassJob.ValueNullable?.RowId ?? 0;
            var drawList = ImGui.GetWindowDrawList();
            
            float newScale = scale * config.JobRingScaleMultiplier;
            float offset = 128f * (newScale - scale);
            var drawPosition = ImGui.GetItemRectMin() + new Vector2(-offset + config.JobRingOffsetX * scale, wobbledY - offset + config.JobRingOffsetY * scale);

            float primaryPercent = 0f;
            float secondaryPercent = 0f;
            Vector3 primaryColor = new Vector3(1, 1, 1);
            Vector3 secondaryColor = new Vector3(1, 1, 1);
            bool showSecondary = false;
            int maxSegments = 0;

            var jg = KingdomHeartsPlugin.Jg;

            switch (jobId)
            {
                case 19: // PLD
                    var pld = jg.Get<PLDGauge>();
                    primaryPercent = pld.OathGauge / 100f;
                    primaryColor = new Vector3(1.0f, 0.9f, 0.5f);
                    break;
                case 21: // WAR
                    var war = jg.Get<WARGauge>();
                    primaryPercent = war.BeastGauge / 100f;
                    primaryColor = new Vector3(0.9f, 0.2f, 0.1f);
                    break;
                case 32: // DRK
                    var drk = jg.Get<DRKGauge>();
                    primaryPercent = drk.Blood / 100f;
                    primaryColor = new Vector3(0.8f, 0.1f, 0.1f);
                    break;
                case 37: // GNB
                    var gnb = jg.Get<GNBGauge>();
                    maxSegments = 3; 
                    primaryPercent = gnb.Ammo / (float)maxSegments;
                    primaryColor = new Vector3(0.7f, 0.8f, 0.9f);
                    break;
                case 24: // WHM
                    var whm = jg.Get<WHMGauge>();
                    maxSegments = 3;
                    primaryPercent = whm.Lily / (float)maxSegments;
                    primaryColor = new Vector3(0.6f, 0.8f, 1.0f);
                    if (whm.BloodLily > 0)
                    {
                        showSecondary = true;
                        secondaryPercent = whm.BloodLily / (float)maxSegments;
                        secondaryColor = new Vector3(0.9f, 0.2f, 0.2f);
                    }
                    break;
                case 28: // SCH
                    var sch = jg.Get<SCHGauge>();
                    primaryPercent = sch.FairyGauge / 100f;
                    primaryColor = new Vector3(0.4f, 0.9f, 0.6f);
                    break;
                case 33: // AST
                    break;
                case 40: // SGE
                    var sge = jg.Get<SGEGauge>();
                    maxSegments = 3;
                    primaryPercent = sge.Addersgall / (float)maxSegments;
                    primaryColor = new Vector3(0.4f, 0.8f, 0.9f);
                    break;
                case 20: // MNK
                    var mnk = jg.Get<MNKGauge>();
                    maxSegments = 5;
                    primaryPercent = mnk.Chakra / (float)maxSegments;
                    primaryColor = new Vector3(0.9f, 0.8f, 0.3f);
                    break;
                case 22: // DRG
                    var drg = jg.Get<DRGGauge>();
                    maxSegments = 2;
                    primaryPercent = drg.FirstmindsFocusCount / (float)maxSegments;
                    primaryColor = new Vector3(0.2f, 0.6f, 0.9f);
                    break;
                case 30: // NIN
                    var nin = jg.Get<NINGauge>();
                    primaryPercent = nin.Ninki / 100f;
                    primaryColor = new Vector3(0.8f, 0.2f, 0.3f);
                    break;
                case 34: // SAM
                    var sam = jg.Get<SAMGauge>();
                    primaryPercent = sam.Kenki / 100f;
                    primaryColor = new Vector3(0.9f, 0.4f, 0.1f);
                    break;
                case 39: // RPR
                    var rpr = jg.Get<RPRGauge>();
                    primaryPercent = rpr.Soul / 100f;
                    primaryColor = new Vector3(0.8f, 0.1f, 0.3f);
                    break;
                case 41: // VPR
                    var vpr = jg.Get<VPRGauge>();
                    primaryPercent = vpr.SerpentOffering / 100f;
                    primaryColor = new Vector3(0.3f, 0.8f, 0.3f);
                    break;
                case 23: // BRD
                    var brd = jg.Get<BRDGauge>();
                    primaryPercent = brd.SoulVoice / 100f;
                    primaryColor = new Vector3(0.5f, 0.8f, 0.7f);
                    break;
                case 31: // Mch
                    var mch = jg.Get<MCHGauge>();
                    primaryPercent = mch.Heat / 100f;
                    primaryColor = new Vector3(0.9f, 0.3f, 0.1f);
                    break;
                case 38: // DNC
                    var dnc = jg.Get<DNCGauge>();
                    primaryPercent = dnc.Esprit / 100f;
                    primaryColor = new Vector3(0.9f, 0.7f, 0.8f);
                    break;
                case 25: // BLM
                    var blm = jg.Get<BLMGauge>();
                    maxSegments = 3;
                    primaryPercent = blm.PolyglotStacks / (float)maxSegments;
                    primaryColor = new Vector3(0.7f, 0.3f, 0.9f);
                    break;
                case 35: // RDM
                    var rdm = jg.Get<RDMGauge>();
                    primaryPercent = rdm.WhiteMana / 100f; 
                    primaryColor = new Vector3(0.9f, 0.9f, 0.9f);
                    showSecondary = true;
                    secondaryPercent = rdm.BlackMana / 100f; 
                    secondaryColor = new Vector3(0.4f, 0.4f, 0.9f); 
                    break;
                case 42: // PCT
                    var pct = jg.Get<PCTGauge>();
                    primaryPercent = pct.PalleteGauge / 100f;
                    primaryColor = new Vector3(0.9f, 0.5f, 0.8f);
                    break;
            }

            primaryPercent = Math.Clamp(primaryPercent, 0f, 1f);
            secondaryPercent = Math.Clamp(secondaryPercent, 0f, 1f);

            float startAngle = config.JobRingStartAngle * (float)Math.PI / 180f; 
            float maxAngle = 270f * (float)Math.PI / 180f; 
            
            float thickness = config.JobRingOutlineThickness * newScale;
            float radius = config.JobRingRadius * newScale;
            float width = config.JobRingWidth * newScale;
            
            float innerRadius = radius - width / 2f;
            float outerRadius = radius + width / 2f;
            
            float size = 256f * newScale;
            Vector2 center = drawPosition + new Vector2(size / 2f, size / 2f);
            
            uint outlineColor = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));
            uint bgColor = ImGui.GetColorU32(new Vector4(0.07843f, 0.07843f, 0.0745f, 0.5f));

            // Bypass window clipping to prevent cutoff at top and left
            drawList.PushClipRect(new Vector2(-8192.0f, -8192.0f), new Vector2(8192.0f, 8192.0f), false);

            // Background Ring Fill
            drawList.PathArcTo(center, radius, startAngle, startAngle + maxAngle, 64);
            drawList.PathStroke(bgColor, ImDrawFlags.None, width);

            // Primary Ring Fill
            if (primaryPercent > 0)
            {
                float primaryAngle = startAngle + (maxAngle * primaryPercent);
                drawList.PathArcTo(center, radius, startAngle, primaryAngle, 64);
                drawList.PathStroke(ImGui.GetColorU32(new Vector4(primaryColor, 1f)), ImDrawFlags.None, width);
            }

            // Secondary Ring Fill (drawn slightly thinner on the interior logic overlay)
            if (secondaryPercent > 0 && showSecondary)
            {
                float secondaryAngle = startAngle + (maxAngle * secondaryPercent);
                float secWidth = width * 0.6f;
                drawList.PathArcTo(center, radius, startAngle, secondaryAngle, 64);
                drawList.PathStroke(ImGui.GetColorU32(new Vector4(secondaryColor, 1f)), ImDrawFlags.None, secWidth);
            }

            if (thickness > 0)
            {
                // Inner and Outer Arcs
                drawList.PathArcTo(center, innerRadius, startAngle, startAngle + maxAngle, 64);
                drawList.PathStroke(outlineColor, ImDrawFlags.None, thickness);
                
                drawList.PathArcTo(center, outerRadius, startAngle, startAngle + maxAngle, 64);
                drawList.PathStroke(outlineColor, ImDrawFlags.None, thickness);
                
                // Start Cap
                Vector2 startP1 = center + new Vector2((float)Math.Cos(startAngle), (float)Math.Sin(startAngle)) * innerRadius;
                Vector2 startP2 = center + new Vector2((float)Math.Cos(startAngle), (float)Math.Sin(startAngle)) * outerRadius;
                drawList.AddLine(startP1, startP2, outlineColor, thickness);
                
                // End Cap
                float endAngle = startAngle + maxAngle;
                Vector2 endP1 = center + new Vector2((float)Math.Cos(endAngle), (float)Math.Sin(endAngle)) * innerRadius;
                Vector2 endP2 = center + new Vector2((float)Math.Cos(endAngle), (float)Math.Sin(endAngle)) * outerRadius;
                drawList.AddLine(endP1, endP2, outlineColor, thickness);

                // Internal stock visual segments
                if (config.JobRingShowSegments && maxSegments > 1)
                {
                    for (int i = 1; i < maxSegments; i++)
                    {
                        float segPercent = (float)i / maxSegments;
                        float segAngle = startAngle + segPercent * maxAngle;
                        Vector2 segP1 = center + new Vector2((float)Math.Cos(segAngle), (float)Math.Sin(segAngle)) * innerRadius;
                        Vector2 segP2 = center + new Vector2((float)Math.Cos(segAngle), (float)Math.Sin(segAngle)) * outerRadius;
                        drawList.AddLine(segP1, segP2, outlineColor, thickness);
                    }
                }
            }

            drawList.PopClipRect();
        }

        public void Dispose()
        {
        }
    }
}