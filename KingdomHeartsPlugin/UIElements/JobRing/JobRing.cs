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
                    showSecondary = true;
                    targetSecondary = whm.BloodLily / (float)maxSegments;
                    secondaryColor = new Vector3(0.9f, 0.2f, 0.2f);
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
                    maxSegments = 3;
                    if (sam.Kenki > 0)
                    {
                        targetPrimary = sam.Kenki / 100f;
                        primaryColor = new Vector3(0.9f, 0.4f, 0.1f);
                    }
                    if (sam.MeditationStacks > 0)
                    {
                        showSecondary = true;
                        targetSecondary = sam.MeditationStacks / (float)maxSegments;
                        secondaryColor = new Vector3(0.9f, 0.2f, 0.2f);
                    }
                    break;
                case 39: // RPR
                    var rpr = jg.Get<RPRGauge>();
                    targetPrimary = rpr.Soul / 100f;
                    primaryColor = new Vector3(0.8f, 0.1f, 0.3f);
                    if (rpr.Shroud > 0)
                    {
                        showSecondary = true;
                        targetSecondary = rpr.Shroud / 100f;
                        secondaryColor = new Vector3(0.9f, 0.2f, 0.2f);
                    }
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
                    if (dnc.Feathers > 0)
                    {
                        showSecondary = true;
                        maxSegments = 4;
                        targetSecondary = dnc.Feathers / (float)maxSegments;
                        secondaryColor = new Vector3(0.3f, 0.8f, 0.3f);
                    }
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

            // Job settings overrides
            bool useOverride = cfg.PerJobRingSettings.TryGetValue((uint)jobId, out var jobCfg) && jobCfg.UseCustomSettings;

            // Apply custom per-job colors if configured
            if (useOverride && jobCfg.UseCustomColors)
            {
                primaryColor = new Vector3(jobCfg.PrimaryColor.X, jobCfg.PrimaryColor.Y, jobCfg.PrimaryColor.Z);
                secondaryColor = new Vector3(jobCfg.SecondaryColor.X, jobCfg.SecondaryColor.Y, jobCfg.SecondaryColor.Z);
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

            // Colors & Common Geometry Constants
            uint outlineCol = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));
            // Completely dark and not transparent
            uint bgCol = ImGui.GetColorU32(new Vector4(0.2f, 0.2f, 0.2f, 1f));

            float size = 256f * newScale;
            Vector2 centre = drawPos + new Vector2(size / 2f, size / 2f);

            // Disable window clipping (fix top‑left cut‑off)
            drawList.PushClipRect(new Vector2(-8192.0f, -8192.0f), new Vector2(8192.0f, 8192.0f), false);

            // -------------------------------------------------------------------------
            // 1. SECONDARY OUTER RING DRAWING (Dual-Gauge Jobs: WHM & RDM)
            // (Drawn first so it renders beneath the primary ring on the Z-axis)
            // -------------------------------------------------------------------------
            if (showSecondary)
            {
                float secBaseStartAng = useOverride ? jobCfg.JobRingSecondaryStartAngle : cfg.JobRingSecondaryStartAngle;
                float secStartAng = secBaseStartAng * (float)Math.PI / 180f;

                bool secBaseWrapFull = useOverride ? jobCfg.JobRingSecondaryWrapFull : cfg.JobRingSecondaryWrapFull;
                float secBaseMaxAng = useOverride ? jobCfg.JobRingSecondaryMaxAngle : cfg.JobRingSecondaryMaxAngle;
                float secMaxAngDeg = secBaseWrapFull ? 270f : secBaseMaxAng;
                float secMaxAng = secMaxAngDeg * (float)Math.PI / 180f;

                float secThickness = (useOverride ? jobCfg.JobRingSecondaryOutlineThickness : cfg.JobRingSecondaryOutlineThickness) * newScale;
                float secRadius = (useOverride ? jobCfg.JobRingSecondaryRadius : cfg.JobRingSecondaryRadius) * newScale;
                float secWidth = (useOverride ? jobCfg.JobRingSecondaryWidth : cfg.JobRingSecondaryWidth) * newScale;
                float secInnerRadius = secRadius - secWidth / 2f;
                float secOuterRadius = secRadius + secWidth / 2f;

                // Secondary background ring (Kept flat for contrast)
                drawList.PathArcTo(centre, secRadius, secStartAng, secStartAng + secMaxAng, 64);
                drawList.PathStroke(bgCol, ImDrawFlags.None, secWidth);

                // Secondary damage trail
                if (cfg.JobRingShowDamageTrail && _secondaryDamagedAlpha > 0 && _secondaryBeforeSpent > targetSecondary)
                {
                    float trailAng = secStartAng + (secMaxAng * _secondaryBeforeSpent);
                    DrawRingGradient(drawList, centre, secRadius, secWidth, secStartAng, trailAng, new Vector3(0.8f, 0.2f, 0.8f), 0.6f * _secondaryDamagedAlpha);
                }

                // Secondary restored fill
                if (_secondaryTemp < targetSecondary)
                {
                    float restoredAng = secStartAng + (secMaxAng * targetSecondary);
                    DrawRingGradient(drawList, centre, secRadius, secWidth, secStartAng, restoredAng, new Vector3(0.4f, 0.8f, 1f), 0.8f);
                }

                // Secondary main fill
                if (_secondaryTemp > 0)
                {
                    float secAng = secStartAng + (secMaxAng * _secondaryTemp);
                    DrawRingGradient(drawList, centre, secRadius, secWidth, secStartAng, secAng, secondaryColor, 1f);
                }

                // Secondary outlines, caps, and segment lines
                if (secThickness > 0)
                {
                    drawList.PathArcTo(centre, secInnerRadius, secStartAng, secStartAng + secMaxAng, 64);
                    drawList.PathStroke(outlineCol, ImDrawFlags.None, secThickness);
                    drawList.PathArcTo(centre, secOuterRadius, secStartAng, secStartAng + secMaxAng, 64);
                    drawList.PathStroke(outlineCol, ImDrawFlags.None, secThickness);

                    // Caps
                    Vector2 secStartInner = centre + new Vector2((float)Math.Cos(secStartAng), (float)Math.Sin(secStartAng)) * secInnerRadius;
                    Vector2 secStartOuter = centre + new Vector2((float)Math.Cos(secStartAng), (float)Math.Sin(secStartAng)) * secOuterRadius;
                    drawList.AddLine(secStartInner, secStartOuter, outlineCol, secThickness);

                    float secEndAng = secStartAng + secMaxAng;
                    Vector2 secEndInner = centre + new Vector2((float)Math.Cos(secEndAng), (float)Math.Sin(secEndAng)) * secInnerRadius;
                    Vector2 secEndOuter = centre + new Vector2((float)Math.Cos(secEndAng), (float)Math.Sin(secEndAng)) * secOuterRadius;
                    drawList.AddLine(secEndInner, secEndOuter, outlineCol, secThickness);

                    if (cfg.JobRingSecondaryShowSegments && maxSegments > 1)
                    {
                        for (int i = 1; i < maxSegments; i++)
                        {
                            float segPerc = (float)i / maxSegments;
                            float segAng = secStartAng + segPerc * secMaxAng;
                            Vector2 segInner = centre + new Vector2((float)Math.Cos(segAng), (float)Math.Sin(segAng)) * secInnerRadius;
                            Vector2 segOuter = centre + new Vector2((float)Math.Cos(segAng), (float)Math.Sin(segAng)) * secOuterRadius;
                            drawList.AddLine(segInner, segOuter, outlineCol, secThickness);
                        }
                    }
                }
            }

            // -------------------------------------------------------------------------
            // 2. PRIMARY INNER RING DRAWING
            // (Drawn last so it renders on top)
            // -------------------------------------------------------------------------
            float baseStartAng = useOverride ? jobCfg.JobRingStartAngle : cfg.JobRingStartAngle;
            float startAng = baseStartAng * (float)Math.PI / 180f;

            bool baseWrapFull = useOverride ? jobCfg.JobRingWrapFull : cfg.JobRingWrapFull;
            float baseMaxAng = useOverride ? jobCfg.JobRingMaxAngle : cfg.JobRingMaxAngle;
            float maxAngDeg = baseWrapFull ? 270f : baseMaxAng;
            float maxAng = maxAngDeg * (float)Math.PI / 180f;

            float thickness = (useOverride ? jobCfg.JobRingOutlineThickness : cfg.JobRingOutlineThickness) * newScale;
            float radius = (useOverride ? jobCfg.JobRingRadius : cfg.JobRingRadius) * newScale;
            float width = (useOverride ? jobCfg.JobRingWidth : cfg.JobRingWidth) * newScale;
            float innerRadius = radius - width / 2f;
            float outerRadius = radius + width / 2f;

            // Background ring (Kept flat for contrast)
            drawList.PathArcTo(centre, radius, startAng, startAng + maxAng, 64);
            drawList.PathStroke(bgCol, ImDrawFlags.None, width);

            // Damage trail – primary
            if (cfg.JobRingShowDamageTrail && _primaryDamagedAlpha > 0 && _primaryBeforeSpent > targetPrimary)
            {
                float trailAng = startAng + (maxAng * _primaryBeforeSpent);
                DrawRingGradient(drawList, centre, radius, width, startAng, trailAng, new Vector3(1f, 0f, 0f), 0.6f * _primaryDamagedAlpha);
            }

            // Primary restored fill
            if (_primaryTemp < targetPrimary)
            {
                float restoredAng = startAng + (maxAng * targetPrimary);
                DrawRingGradient(drawList, centre, radius, width, startAng, restoredAng, new Vector3(0.4f, 0.8f, 1f), 0.8f);
            }

            // Primary main fill (animates up to _primaryTemp)
            if (_primaryTemp > 0)
            {
                float primAng = startAng + (maxAng * _primaryTemp);
                DrawRingGradient(drawList, centre, radius, width, startAng, primAng, primaryColor, 1f);
            }

            // Outlines, caps, and segment lines for primary ring
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
        
        /// <summary>
        /// Draws a thick arc as a series of connected quads, dynamically applying a radial gradient from inner to outer radius.
        /// </summary>
        private void DrawRingGradient(ImDrawListPtr drawList, Vector2 centre, float radius, float width, float startAng, float endAng, Vector3 baseColor, float alpha)
        {
            if (endAng <= startAng) return;
            
            float innerRadius = radius - width / 2f;

            // Generate an appropriate number of smoothing segments dynamically
            int segments = (int)Math.Max(4, Math.Ceiling(64 * ((endAng - startAng) / (Math.PI * 2.0))));
            int radialSteps = 4; // Subdivide radially to create the gradient

            for (int i = 0; i < segments; i++)
            {
                float a0 = startAng + (endAng - startAng) * (i / (float)segments);
                float a1 = startAng + (endAng - startAng) * ((i + 1) / (float)segments);
                
                // Add a very tiny overlap to the end angle to bridge hairline anti-aliasing gaps between quads
                if (i < segments - 1) a1 += 0.015f;

                for (int r = 0; r < radialSteps; r++)
                {
                    float r0 = innerRadius + width * (r / (float)radialSteps);
                    float r1 = innerRadius + width * ((r + 1) / (float)radialSteps);
                    
                    // Add a tiny overlap to bridge radial gaps between steps
                    if (r < radialSteps - 1) r1 += 0.5f;

                    Vector2 p0_inner = centre + new Vector2((float)Math.Cos(a0) * r0, (float)Math.Sin(a0) * r0);
                    Vector2 p0_outer = centre + new Vector2((float)Math.Cos(a0) * r1, (float)Math.Sin(a0) * r1);
                    Vector2 p1_inner = centre + new Vector2((float)Math.Cos(a1) * r0, (float)Math.Sin(a1) * r0);
                    Vector2 p1_outer = centre + new Vector2((float)Math.Cos(a1) * r1, (float)Math.Sin(a1) * r1);

                    // Calculate the interpolation factor (0.0 = Inner, 1.0 = Outer)
                    float t = (r + 0.5f) / radialSteps;
                    
                    // Dark (0.3x) at Inner, Light (1.0x) at Outer
                    float intensity = 0.3f + 0.7f * t;
                    Vector3 gradColor = baseColor * intensity;
                    
                    // Clamp just in case floating point scaling pushes it above valid ranges
                    gradColor.X = Math.Min(1f, gradColor.X);
                    gradColor.Y = Math.Min(1f, gradColor.Y);
                    gradColor.Z = Math.Min(1f, gradColor.Z);

                    uint col = ImGui.GetColorU32(new Vector4(gradColor, alpha));
                    
                    drawList.AddQuadFilled(p0_inner, p0_outer, p1_outer, p1_inner, col);
                }
            }
        }

        public void Dispose() { }
    }
}