using System;
using System.IO;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Bindings.ImGui;
using KingdomHeartsPlugin.Utilities;
using Dalamud.Interface.Textures;

namespace KingdomHeartsPlugin.UIElements.JobRing
{
    public class JobRing : IDisposable
    {
        private Ring _gaugeBgRing;
        private Ring _gaugePrimaryRing;
        private Ring _gaugeSecondaryRing;
        private Ring _gaugeOutlineRing;

        private ISharedImmediateTexture BaseEdge => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_base_edge.png"));
        private ISharedImmediateTexture EndEdge => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_end_edge.png"));

        public JobRing()
        {
            var bgColor = new Vector3(0.07843f, 0.07843f, 0.0745f);
            string tex = Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png");
            string outlineTex = Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_outline_segment.png");
            
            _gaugeBgRing = new Ring(tex, bgColor.X, bgColor.Y, bgColor.Z, 0.5f);
            _gaugePrimaryRing = new Ring(tex, 1f, 1f, 1f, 1f);
            _gaugeSecondaryRing = new Ring(tex, 1f, 1f, 1f, 1f);
            _gaugeOutlineRing = new Ring(outlineTex, 1f, 1f, 1f, 1f);
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

            // Always draw BG & Outline borders
            _gaugeBgRing.Draw(drawList, 1f, drawPosition, 3, newScale);
            _gaugeOutlineRing.Draw(drawList, 1f, drawPosition, 3, newScale);

            if (primaryPercent > 0)
            {
                _gaugePrimaryRing.Color = primaryColor;
                _gaugePrimaryRing.Draw(drawList, primaryPercent, drawPosition, 3, newScale);
            }
            if (secondaryPercent > 0 && showSecondary)
            {
                _gaugeSecondaryRing.Color = secondaryColor;
                _gaugeSecondaryRing.Draw(drawList, secondaryPercent, drawPosition, 3, newScale * 0.98f);
            }

            // Draw Caps & Segments
            var baseTex = BaseEdge?.GetWrapOrEmpty();
            var endTex = EndEdge?.GetWrapOrEmpty();
            
            if (baseTex != null && baseTex.Handle != IntPtr.Zero)
            {
                float size = 256f * newScale;
                Vector2 center = drawPosition + new Vector2(size / 2f, size / 2f);
                
                // Base Cap at 0%
                drawList.AddImage(baseTex.Handle, drawPosition, drawPosition + new Vector2(size, size));
                
                // Current Progress Cap
                if (endTex != null && endTex.Handle != IntPtr.Zero)
                {
                    float angle = primaryPercent * 0.75f * (float)Math.PI * 2;
                    ImageDrawing.ImageRotated(drawList, endTex.Handle, center, new Vector2(size, size), angle);
                }

                // Internal stock visual segments
                if (config.JobRingShowSegments && maxSegments > 1)
                {
                    for (int i = 1; i < maxSegments; i++)
                    {
                        float segPercent = (float)i / maxSegments;
                        float segAngle = segPercent * 0.75f * (float)Math.PI * 2;
                        
                        if (endTex != null && endTex.Handle != IntPtr.Zero)
                            ImageDrawing.ImageRotated(drawList, endTex.Handle, center, new Vector2(size, size), segAngle);
                        ImageDrawing.ImageRotated(drawList, baseTex.Handle, center, new Vector2(size, size), segAngle);
                    }
                }
            }
        }

        public void Dispose()
        {
            _gaugeBgRing?.Dispose();
            _gaugePrimaryRing?.Dispose();
            _gaugeSecondaryRing?.Dispose();
            _gaugeOutlineRing?.Dispose();
        }
    }
}