using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Bindings.ImGui;

namespace KingdomHeartsPlugin.UIElements.JobRingNS
{
    public class JobRing : IDisposable
    {
        // Animation state (mirrors HealthFrame behavior)
        private float _primaryTemp;          // Analogous to HpTemp (filling current value)
        private float _primaryBeforeSpent;   // Analogous to HpBeforeDamaged (draining damage trail)
        private float _primaryDamageTimer;  // Delay timer for gauge spend
        private float _primaryHealTimer;    // Delay timer for gauge gain
        private float _primaryDamagedAlpha;  // Alpha channel for damage trail

        private float _secondaryTemp;
        private float _secondaryBeforeSpent;
        private float _secondaryDamageTimer;
        private float _secondaryHealTimer;
        private float _secondaryDamagedAlpha;

        private bool _initialized;

        // State trackers to correctly detect gauge and job changes
        private float _lastTargetPrimary;
        private float _lastTargetSecondary;
        private uint _lastJobId;

        public JobRing() { }

        public void Draw(IPlayerCharacter player, float scale, float wobbledY)
        {
            var cfg = KingdomHeartsPlugin.Ui.Configuration;
            if (!cfg.JobRingEnabled) return;

            var jobId = player.ClassJob.ValueNullable?.RowId ?? 0;
            var drawList = ImGui.GetWindowDrawList();

            // Transform
            float newScale = scale * cfg.JobRingScaleMultiplier;
            float offset = 128f * (newScale - scale);
            var drawPos = ImGui.GetItemRectMin() + new Vector2(-offset + cfg.JobRingOffsetX * scale, wobbledY - offset + cfg.JobRingOffsetY * scale);

            // Raw target percentages (pre‑animation)
            float targetPrimary = 0f;
            float targetSecondary = 0f;
            Vector3 primaryColor = new Vector3(1, 1, 1);
            Vector3 secondaryColor = new Vector3(1, 1, 1);
            bool showSecondary = false;
            int maxSegments = 0;

            var jg = KingdomHeartsPlugin.Jg;
            switch (jobId)
            {
                case 19: // PLD
                    var pld = jg.Get<PLDGauge>();
                    targetPrimary = pld.OathGauge / 100f;
                    primaryColor = new Vector3(1.0f, 0.9f, 0.5f);
                    break;
                case 21: // WAR
                    var war = jg.Get<WARGauge>();
                    targetPrimary = war.BeastGauge / 100f;
                    primaryColor = new Vector3(0.9f, 0.2f, 0.1f);
                    break;
                case 32: // DRK
                    var drk = jg.Get<DRKGauge>();
                    targetPrimary = drk.Blood / 100f;
                    primaryColor = new Vector3(0.8f, 0.1f, 0.1f);
                    break;
                case 37: // GNB (segmented)
                    var gnb = jg.Get<GNBGauge>();
                    maxSegments = 3;
                    targetPrimary = gnb.Ammo / (float)maxSegments;
                    primaryColor = new Vector3(0.7f, 0.8f, 0.9f);
                    break;
                case 24: // WHM (dual)
                    var whm = jg.Get<WHMGauge>();
                    maxSegments = 3;
                    targetPrimary = whm.Lily / (float)maxSegments;
                    primaryColor = new Vector3(0.6f, 0.8f, 1.0f);
                    if (whm.BloodLily > 0)
                    {
                        showSecondary = true;
                        targetSecondary = whm.BloodLily / (float)maxSegments;
                        secondaryColor = new Vector3(0.9f, 0.2f, 0.2f);
                    }
                    break;
                case 28: // SCH
                    var sch = jg.Get<SCHGauge>();
                    targetPrimary = sch.FairyGauge / 100f;
                    primaryColor = new Vector3(0.4f, 0.9f, 0.6f);
                    break;
                case 40: // SGE (segmented)
                    var sge = jg.Get<SGEGauge>();
                    maxSegments = 3;
                    targetPrimary = sge.Addersgall / (float)maxSegments;
                    primaryColor = new Vector3(0.4f, 0.8f, 0.9f);
                    break;
                case 20: // MNK (segmented)
                    var mnk = jg.Get<MNKGauge>();
                    maxSegments = 5;
                    targetPrimary = mnk.Chakra / (float)maxSegments;
                    primaryColor = new Vector3(0.9f, 0.8f, 0.3f);
                    break;
                case 22: // DRG (segmented)
                    var drg = jg.Get<DRGGauge>();
                    maxSegments = 2;
                    targetPrimary = drg.FirstmindsFocusCount / (float)maxSegments;
                    primaryColor = new Vector3(0.2f, 0.6f, 0.9f);
                    break;
                case 30: // NIN
                    var nin = jg.Get<NINGauge>();
                    targetPrimary = nin.Ninki / 100f;
                    primaryColor = new Vector3(0.8f, 0.2f, 0.3f);
                    break;
                case 34: // SAM
                    var sam = jg.Get<SAMGauge>();
                    targetPrimary = sam.Kenki / 100f;
                    primaryColor = new Vector3(0.9f, 0.4f, 0.1f);
                    break;
                case 39: // RPR
                    var rpr = jg.Get<RPRGauge>();
                    targetPrimary = rpr.Soul / 100f;
                    primaryColor = new Vector3(0.8f, 0.1f, 0.3f);
                    break;
                case 41: // VPR
                    var vpr = jg.Get<VPRGauge>();
                    targetPrimary = vpr.SerpentOffering / 100f;
                    primaryColor = new Vector3(0.3f, 0.8f, 0.3f);
                    break;
                case 23: // BRD
                    var brd = jg.Get<BRDGauge>();
                    targetPrimary = brd.SoulVoice / 100f;
                    primaryColor = new Vector3(0.5f, 0.8f, 0.7f);
                    break;
                case 31: // MCH
                    var mch = jg.Get<MCHGauge>();
                    targetPrimary = mch.Heat / 100f;
                    primaryColor = new Vector3(0.9f, 0.3f, 0.1f);
                    break;
                case 38: // DNC
                    var dnc = jg.Get<DNCGauge>();
                    targetPrimary = dnc.Esprit / 100f;
                    primaryColor = new Vector3(0.9f, 0.7f, 0.8f);
                    break;
                case 25: // BLM (segmented)
                    var blm = jg.Get<BLMGauge>();
                    maxSegments = 3;
                    targetPrimary = blm.PolyglotStacks / (float)maxSegments;
                    primaryColor = new Vector3(0.7f, 0.3f, 0.9f);
                    break;
                case 35: // RDM (dual)
                    var rdm = jg.Get<RDMGauge>();
                    targetPrimary = rdm.WhiteMana / 100f;
                    primaryColor = new Vector3(0.9f, 0.9f, 0.9f);
                    showSecondary = true;
                    targetSecondary = rdm.BlackMana / 100f;
                    secondaryColor = new Vector3(0.4f, 0.4f, 0.9f);
                    break;
                case 42: // PCT
                    var pct = jg.Get<PCTGauge>();
                    targetPrimary = pct.PalleteGauge / 100f;
                    primaryColor = new Vector3(0.9f, 0.5f, 0.8f);
                    break;
                default:
                    break;
            }

            // Clamp
            targetPrimary = Math.Clamp(targetPrimary, 0f, 1f);
            targetSecondary = Math.Clamp(targetSecondary, 0f, 1f);

            // Initialise animation state on first draw OR when job changes
            if (!_initialized || _lastJobId != jobId)
            {
                _primaryTemp = targetPrimary;
                _primaryBeforeSpent = targetPrimary;
                _primaryDamageTimer = 0f;
                _primaryHealTimer = 0f;
                _primaryDamagedAlpha = 0f;

                _secondaryTemp = targetSecondary;
                _secondaryBeforeSpent = targetSecondary;
                _secondaryDamageTimer = 0f;
                _secondaryHealTimer = 0f;
                _secondaryDamagedAlpha = 0f;

                _lastTargetPrimary = targetPrimary;
                _lastTargetSecondary = targetSecondary;
                _lastJobId = jobId;
                _initialized = true;
            }

            // Detect changes in primary target value (mirrors UpdateHealth)
            if (Math.Abs(targetPrimary - _lastTargetPrimary) > 0.0001f)
            {
                if (targetPrimary < _lastTargetPrimary)
                {
                    bool isDamageAnimating = _primaryBeforeSpent > _lastTargetPrimary && _primaryDamageTimer <= 0;
                    if (!isDamageAnimating)
                    {
                        _primaryDamageTimer = cfg.JobRingAnimationDelay;
                    }
                    _primaryBeforeSpent = Math.Max(_primaryBeforeSpent, _lastTargetPrimary);
                    _primaryDamagedAlpha = 1f;
                }
                else if (targetPrimary > _lastTargetPrimary)
                {
                    bool isHealAnimating = _primaryTemp < _lastTargetPrimary && _primaryHealTimer <= 0;
                    if (!isHealAnimating)
                    {
                        _primaryHealTimer = cfg.JobRingAnimationDelay;
                    }
                    _primaryTemp = Math.Min(_primaryTemp, _lastTargetPrimary);
                }
                _lastTargetPrimary = targetPrimary;
            }

            // Detect changes in secondary target value
            if (showSecondary && Math.Abs(targetSecondary - _lastTargetSecondary) > 0.0001f)
            {
                if (targetSecondary < _lastTargetSecondary)
                {
                    bool isDamageAnimating = _secondaryBeforeSpent > _lastTargetSecondary && _secondaryDamageTimer <= 0;
                    if (!isDamageAnimating)
                    {
                        _secondaryDamageTimer = cfg.JobRingAnimationDelay;
                    }
                    _secondaryBeforeSpent = Math.Max(_secondaryBeforeSpent, _lastTargetSecondary);
                    _secondaryDamagedAlpha = 1f;
                }
                else if (targetSecondary > _lastTargetSecondary)
                {
                    bool isHealAnimating = _secondaryTemp < _lastTargetSecondary && _secondaryHealTimer <= 0;
                    if (!isHealAnimating)
                    {
                        _secondaryHealTimer = cfg.JobRingAnimationDelay;
                    }
                    _secondaryTemp = Math.Min(_secondaryTemp, _lastTargetSecondary);
                }
                _lastTargetSecondary = targetSecondary;
            }

            // Update animation timers and interpolation steps (mirrors UpdateHpAnimations)
            float dt = KingdomHeartsPlugin.UiSpeed;
            if (cfg.JobRingAnimationEnabled)
            {
                float linearStep = (cfg.JobRingAnimationSpeed * 0.015f) * dt;
                linearStep = Math.Max(linearStep, 0.001f);

                // Sanity clamp bounds
                if (_primaryTemp > targetPrimary) _primaryTemp = targetPrimary;
                if (_primaryBeforeSpent < targetPrimary) _primaryBeforeSpent = targetPrimary;

                // Primary Damage Trail
                if (_primaryDamageTimer > 0)
                {
                    _primaryDamageTimer -= dt;
                }
                else if (_primaryBeforeSpent > targetPrimary)
                {
                    _primaryBeforeSpent -= linearStep;
                    if (_primaryBeforeSpent < targetPrimary) _primaryBeforeSpent = targetPrimary;
                }

                // Primary Restored / Fill Animation
                if (_primaryHealTimer > 0)
                {
                    _primaryHealTimer -= dt;
                }
                else if (_primaryTemp < targetPrimary)
                {
                    _primaryTemp += linearStep;
                    if (_primaryTemp > targetPrimary) _primaryTemp = targetPrimary;
                }

                // Primary Damaged Alpha decay
                if (_primaryBeforeSpent > targetPrimary)
                {
                    _primaryDamagedAlpha = 1f;
                }
                else if (_primaryDamagedAlpha > 0)
                {
                    _primaryDamagedAlpha -= 1.5f * dt;
                    if (_primaryDamagedAlpha < 0) _primaryDamagedAlpha = 0f;
                }

                // Secondary gauge animations (if present)
                if (showSecondary)
                {
                    if (_secondaryTemp > targetSecondary) _secondaryTemp = targetSecondary;
                    if (_secondaryBeforeSpent < targetSecondary) _secondaryBeforeSpent = targetSecondary;

                    if (_secondaryDamageTimer > 0)
                    {
                        _secondaryDamageTimer -= dt;
                    }
                    else if (_secondaryBeforeSpent > targetSecondary)
                    {
                        _secondaryBeforeSpent -= linearStep;
                        if (_secondaryBeforeSpent < targetSecondary) _secondaryBeforeSpent = targetSecondary;
                    }

                    if (_secondaryHealTimer > 0)
                    {
                        _secondaryHealTimer -= dt;
                    }
                    else if (_secondaryTemp < targetSecondary)
                    {
                        _secondaryTemp += linearStep;
                        if (_secondaryTemp > targetSecondary) _secondaryTemp = targetSecondary;
                    }

                    if (_secondaryBeforeSpent > targetSecondary)
                    {
                        _secondaryDamagedAlpha = 1f;
                    }
                    else if (_secondaryDamagedAlpha > 0)
                    {
                        _secondaryDamagedAlpha -= 1.5f * dt;
                        if (_secondaryDamagedAlpha < 0) _secondaryDamagedAlpha = 0f;
                    }
                }
            }
            else
            {
                _primaryTemp = targetPrimary;
                _primaryBeforeSpent = targetPrimary;
                _primaryDamagedAlpha = 0f;

                _secondaryTemp = targetSecondary;
                _secondaryBeforeSpent = targetSecondary;
                _secondaryDamagedAlpha = 0f;
            }

            // Dynamic Arc Geometry (270° when wrap fully enabled, or custom angle)
            float startAng = cfg.JobRingStartAngle * (float)Math.PI / 180f;
            float maxAngDeg = cfg.JobRingWrapFull ? 270f : cfg.JobRingMaxAngle;
            float maxAng = maxAngDeg * (float)Math.PI / 180f;

            float thickness = cfg.JobRingOutlineThickness * newScale;
            float radius = cfg.JobRingRadius * newScale;
            float width = cfg.JobRingWidth * newScale;
            float innerRadius = radius - width / 2f;
            float outerRadius = radius + width / 2f;
            float size = 256f * newScale;
            Vector2 centre = drawPos + new Vector2(size / 2f, size / 2f);

            uint outlineCol = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));
            uint bgCol = ImGui.GetColorU32(new Vector4(0.07843f, 0.07843f, 0.0745f, 0.5f));

            // Disable window clipping (fix top‑left cut‑off)
            drawList.PushClipRect(new Vector2(-8192.0f, -8192.0f), new Vector2(8192.0f, 8192.0f), false);

            // Background ring
            drawList.PathArcTo(centre, radius, startAng, startAng + maxAng, 64);
            drawList.PathStroke(bgCol, ImDrawFlags.None, width);

            // Damage trail – primary (red loss arc with alpha fade)
            if (cfg.JobRingShowDamageTrail && _primaryDamagedAlpha > 0 && _primaryBeforeSpent > targetPrimary)
            {
                float trailAng = startAng + (maxAng * _primaryBeforeSpent);
                drawList.PathArcTo(centre, radius, startAng, trailAng, 64);
                uint trailCol = ImGui.GetColorU32(new Vector4(1f, 0f, 0f, 0.6f * _primaryDamagedAlpha));
                drawList.PathStroke(trailCol, ImDrawFlags.None, width);
            }

            // Primary restored fill (brightened/cyan recovery arc behind current fill)
            if (_primaryTemp < targetPrimary)
            {
                float restoredAng = startAng + (maxAng * targetPrimary);
                drawList.PathArcTo(centre, radius, startAng, restoredAng, 64);
                uint restoredCol = ImGui.GetColorU32(new Vector4(0.4f, 0.8f, 1f, 0.8f));
                drawList.PathStroke(restoredCol, ImDrawFlags.None, width);
            }

            // Primary main fill (animates up to _primaryTemp)
            if (_primaryTemp > 0)
            {
                float primAng = startAng + (maxAng * _primaryTemp);
                drawList.PathArcTo(centre, radius, startAng, primAng, 64);
                drawList.PathStroke(ImGui.GetColorU32(new Vector4(primaryColor, 1f)), ImDrawFlags.None, width);
            }

            // Damage trail – secondary
            if (showSecondary && cfg.JobRingShowDamageTrail && _secondaryDamagedAlpha > 0 && _secondaryBeforeSpent > targetSecondary)
            {
                float trailAng = startAng + (maxAng * _secondaryBeforeSpent);
                drawList.PathArcTo(centre, radius, startAng, trailAng, 64);
                uint trailCol = ImGui.GetColorU32(new Vector4(0.8f, 0.2f, 0.8f, 0.6f * _secondaryDamagedAlpha));
                drawList.PathStroke(trailCol, ImDrawFlags.None, width);
            }

            // Secondary restored fill
            if (showSecondary && _secondaryTemp < targetSecondary)
            {
                float restoredAng = startAng + (maxAng * targetSecondary);
                float secWidth = width * 0.6f;
                drawList.PathArcTo(centre, radius, startAng, restoredAng, 64);
                uint restoredCol = ImGui.GetColorU32(new Vector4(0.4f, 0.8f, 1f, 0.8f));
                drawList.PathStroke(restoredCol, ImDrawFlags.None, secWidth);
            }

            // Secondary main fill (thinner overlay)
            if (showSecondary && _secondaryTemp > 0)
            {
                float secAng = startAng + (maxAng * _secondaryTemp);
                float secWidth = width * 0.6f;
                drawList.PathArcTo(centre, radius, startAng, secAng, 64);
                drawList.PathStroke(ImGui.GetColorU32(new Vector4(secondaryColor, 1f)), ImDrawFlags.None, secWidth);
            }

            // Outlines, caps, and optional segment lines
            if (thickness > 0)
            {
                drawList.PathArcTo(centre, innerRadius, startAng, startAng + maxAng, 64);
                drawList.PathStroke(outlineCol, ImDrawFlags.None, thickness);
                drawList.PathArcTo(centre, outerRadius, startAng, startAng + maxAng, 64);
                drawList.PathStroke(outlineCol, ImDrawFlags.None, thickness);

                // Caps
                Vector2 startInner = centre + new Vector2((float)Math.Cos(startAng), (float)Math.Sin(startAng)) * innerRadius;
                Vector2 startOuter = centre + new Vector2((float)Math.Cos(startAng), (float)Math.Sin(startAng)) * outerRadius;
                drawList.AddLine(startInner, startOuter, outlineCol, thickness);

                float endAng = startAng + maxAng;
                Vector2 endInner = centre + new Vector2((float)Math.Cos(endAng), (float)Math.Sin(endAng)) * innerRadius;
                Vector2 endOuter = centre + new Vector2((float)Math.Cos(endAng), (float)Math.Sin(endAng)) * outerRadius;
                drawList.AddLine(endInner, endOuter, outlineCol, thickness);

                if (cfg.JobRingShowSegments && maxSegments > 1)
                {
                    for (int i = 1; i < maxSegments; i++)
                    {
                        float segPerc = (float)i / maxSegments;
                        float segAng = startAng + segPerc * maxAng;
                        Vector2 segInner = centre + new Vector2((float)Math.Cos(segAng), (float)Math.Sin(segAng)) * innerRadius;
                        Vector2 segOuter = centre + new Vector2((float)Math.Cos(segAng), (float)Math.Sin(segAng)) * outerRadius;
                        drawList.AddLine(segInner, segOuter, outlineCol, thickness);
                    }
                }
            }

            drawList.PopClipRect();
        }

        public void Dispose() { }
    }
}