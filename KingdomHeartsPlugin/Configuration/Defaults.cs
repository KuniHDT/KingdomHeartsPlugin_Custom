using KingdomHeartsPlugin.Enums;
using System.Numerics;

namespace KingdomHeartsPlugin.Configuration
{
    public static partial class Defaults
    {
        #region General
        public const bool Locked = false;
        public const bool Enabled  = true;
        public const bool HideWhenNpcTalking  = false;
        public const float Scale  = 1f;
        public const string TextFormatCulture = "en-US";
        #endregion

        #region HP
        public const bool HpBarEnabled  = true;
        public const float HpValueTextPositionX  = 26;
        public const float HpValueTextPositionY  = 130;
        public const float HpValueTextSize  = 21;
        public const int HpValueTextAlignment  = 0;
        public const NumberFormatStyle HpValueTextStyle = NumberFormatStyle.NoFormatting;
        public const float HpDamageWobbleIntensity  = 100f;
        public const int HpForFullRing  = 60000;
        public const int MaximumHpForMaximumLength  = 120000;
        public const int MinimumHpForLength  = 1000;
        public const float HpPerPixelLongBar  = 100;
        public const int PvpHpForFullRing  = 20000;
        public const int PvpMaximumHpForMaximumLength  = 40000;
        public const int PvpMinimumHpForLength  = 6666;
        public const float PvpHpPerPixelLongBar  = 33.33f;
        public const float LowHpPercent  = 25f;
        public const bool ShowHpRecovery  = true;
        public const bool ShowHpVal  = true;
        public const bool LengthByLevel = false;
        public const bool IgnoreRingForLevelScaling = false;
        public const int HpPerLevel = 500;
        public const bool PvpLengthByLevel = false;
        public const bool PvpIgnoreRingForLevelScaling = false;
        public const int PvpHpPerLevel = 250;
        public const float HpAnimationSpeed = 50f;
        public const float HpAnimationDelay = 1.0f;

        public const bool ShowShield = true;
        public const bool ShieldScalesWithLevel = true;
        public static readonly Vector4 ShieldColor = new(1f, 0.84f, 0f, 0.75f);
        public const float ShieldAnimationSpeed = 50f;
        public const float ShieldAnimationDelay = 1.0f;
        public const int ShieldFillDirection = 0;

        public const bool EnableRoleHpMultipliers = false;

        public const float TankHpMultiplier = 1.0f;
        public const bool EnableTankJobHpMultipliers = false;
        public const float PldHpMultiplier = 1.0f;
        public const float WarHpMultiplier = 1.0f;
        public const float DrkHpMultiplier = 1.0f;
        public const float GnbHpMultiplier = 1.0f;

        public const float HealerHpMultiplier = 1.0f;
        public const bool EnableHealerJobHpMultipliers = false;
        public const float WhmHpMultiplier = 1.0f;
        public const float SchHpMultiplier = 1.0f;
        public const float AstHpMultiplier = 1.0f;
        public const float SgeHpMultiplier = 1.0f;
        
        public const float MeleeHpMultiplier = 1.0f;
        public const bool EnableMeleeJobHpMultipliers = false;
        public const float MnkHpMultiplier = 1.0f;
        public const float DrgHpMultiplier = 1.0f;
        public const float NinHpMultiplier = 1.0f;
        public const float SamHpMultiplier = 1.0f;
        public const float RprHpMultiplier = 1.0f;
        public const float VprHpMultiplier = 1.0f;
        public const float BstHpMultiplier = 1.0f;

        public const float RangedHpMultiplier = 1.0f;
        public const bool EnableRangedJobHpMultipliers = false;
        public const float BrdHpMultiplier = 1.0f;
        public const float MchHpMultiplier = 1.0f;
        public const float DncHpMultiplier = 1.0f;
        public const float BlmHpMultiplier = 1.0f;
        public const float SmnHpMultiplier = 1.0f;
        public const float RdmHpMultiplier = 1.0f;
        public const float PctHpMultiplier = 1.0f;
        public const float BluHpMultiplier = 1.0f;

        public const float OtherHpMultiplier = 1.0f;
        #endregion

        #region Resource
        public const bool ResourceBarEnabled  = true;
        public const float ResourceBarPositionX  = 0;
        public const float ResourceBarPositionY  = 200;
        public const float ResourceTextPositionX  = -10;
        public const float ResourceTextPositionY  = -7;
        public const float ResourceTextSize  = 24;
        public const NumberFormatStyle ResourceTextStyle = NumberFormatStyle.NoFormatting;
        public const int ResourceTextAlignment  = 2;
        public const int MaximumMpLength  = 11500;
        public const int MinimumMpLength  = 500;
        public const float MpPerPixelLength  = 24.45f;
        public const float GpPerPixelLength  = 1.913f;
        public const int MaximumGpLength  = 900;
        public const int MinimumGpLength  = 1;
        public const float CpPerPixelLength  = 1.382f;
        public const int MaximumCpLength  = 650;
        public const int MinimumCpLength  = 1;
        public const bool TruncateMp  = false;
        public const bool ShowResourceVal  = true;
        public const bool ResourceLengthByLevel = false;
        public const int MpPerLevel = 50;
        public const int GpPerLevel = 5;
        public const int CpPerLevel = 5;
        public const bool ShowResourceRecovery = true;
        public const float ResourceAnimationSpeed = 50f;
        public const float ResourceAnimationDelay = 1.0f;
        
        public const bool EnableRoleResourceMultipliers = false;
        public const float TankResourceMultiplier = 1.0f;
        public const bool EnableTankJobResourceMultipliers = false;
        public const float PldResourceMultiplier = 1.0f;
        public const float WarResourceMultiplier = 1.0f;
        public const float DrkResourceMultiplier = 1.0f;
        public const float GnbResourceMultiplier = 1.0f;

        public const float HealerResourceMultiplier = 1.0f;
        public const bool EnableHealerJobResourceMultipliers = false;
        public const float WhmResourceMultiplier = 1.0f;
        public const float SchResourceMultiplier = 1.0f;
        public const float AstResourceMultiplier = 1.0f;
        public const float SgeResourceMultiplier = 1.0f;

        public const float MeleeResourceMultiplier = 1.0f;
        public const bool EnableMeleeJobResourceMultipliers = false;
        public const float MnkResourceMultiplier = 1.0f;
        public const float DrgResourceMultiplier = 1.0f;
        public const float NinResourceMultiplier = 1.0f;
        public const float SamResourceMultiplier = 1.0f;
        public const float RprResourceMultiplier = 1.0f;
        public const float VprResourceMultiplier = 1.0f;
        public const float BstResourceMultiplier = 1.0f;

        public const float RangedResourceMultiplier = 1.0f;
        public const bool EnableRangedJobResourceMultipliers = false;
        public const float BrdResourceMultiplier = 1.0f;
        public const float MchResourceMultiplier = 1.0f;
        public const float DncResourceMultiplier = 1.0f;
        public const float BlmResourceMultiplier = 1.0f;
        public const float SmnResourceMultiplier = 1.0f;
        public const float RdmResourceMultiplier = 1.0f;
        public const float PctResourceMultiplier = 1.0f;
        public const float BluResourceMultiplier = 1.0f;

        public const float OtherResourceMultiplier = 1.0f;
        
        public static readonly Vector4 ResourceSpentColor = new(1f, 0f, 0f, 1f);
        public static readonly Vector4 ResourceRecoveredColor = new(0.4f, 0.8f, 1f, 0.8f);
        #endregion

        #region Limit Break

        public const bool LimitBarEnabled  = true;
        public const bool LimitGaugeAlwaysShow  = false;
        public const bool LimitGaugeDiadem  = true;
        public const float LimitGaugePositionX  = -180;
        public const float LimitGaugePositionY  = 149;

        #endregion

        #region Job Ring

        public const bool JobRingEnabled = true;
        public const float JobRingScaleMultiplier = 1.15f;
        public const float JobRingOffsetX = 0f;
        public const float JobRingOffsetY = 0f;
        public const bool JobRingShowSegments = true;
        public const bool JobRingWrapFull = true;
        public const float JobRingMaxAngle = 270f;
        public const bool JobRingAnimationEnabled = true;
        public const float JobRingAnimationSpeed = 50f;
        public const float JobRingAnimationDelay = 1.0f;
        public const bool JobRingShowDamageTrail = true;

        public const float JobRingOutlineThickness = 3.0f;
        public const float JobRingRadius = 98.0f;
        public const float JobRingWidth = 28.0f;
        public const float JobRingStartAngle = 180f;

        // Secondary Outer Ring Defaults
        public const bool JobRingSecondaryWrapFull = true;
        public const float JobRingSecondaryMaxAngle = 270f;
        public const float JobRingSecondaryRadius = 120.0f;
        public const float JobRingSecondaryWidth = 16.0f;
        public const float JobRingSecondaryStartAngle = 180f;
        public const float JobRingSecondaryOutlineThickness = 3.0f;
        public const bool JobRingSecondaryShowSegments = true;

        #endregion

        #region Experience

        public const bool ExpBarEnabled  = true;
        public const bool ExpValueTextEnabled = false;
        public const float ExpValueTextSize = 24;
        public const int ExpValueTextAlignment = 0;
        public const NumberFormatStyle ExpValueTextFormatStyle = NumberFormatStyle.NoFormatting;
        public const float ExpValueTextPositionX = 0;
        public const float ExpValueTextPositionY = 0;

        #endregion

        #region ClassInfo

        public const bool LevelEnabled = true;
        public const float LevelTextX = 132f;
        public const float LevelTextY = 81f;
        public const float LevelTextSize = 32f;
        public const TextAlignment LevelTextAlignment = TextAlignment.Center;
        public const float LevelTextScale = 1f;

        public const float ClassIconX = 128f;
        public const float ClassIconY = 150f;
        public const float ClassIconScale = 1.0f;
        public const bool ClassIconEnabled  = true;

        #endregion
    }
}