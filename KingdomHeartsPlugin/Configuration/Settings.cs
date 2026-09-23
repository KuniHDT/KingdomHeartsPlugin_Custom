using System;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Plugin;
using KingdomHeartsPlugin.Enums;

namespace KingdomHeartsPlugin.Configuration
{
    [Serializable]
    public partial class Settings : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        #region General
        public bool Locked { get; set; } = Defaults.Locked;
        public bool Enabled { get; set; } = Defaults.Enabled;
        public bool HideWhenNpcTalking { get; set; } = Defaults.HideWhenNpcTalking;
        public float Scale { get; set; } = Defaults.Scale;
        public string TextFormatCulture = Defaults.TextFormatCulture;
        #endregion

        #region HP
        public bool HpBarEnabled { get; set; } = Defaults.HpBarEnabled;
        public float HpValueTextPositionX { get; set; } = Defaults.HpValueTextPositionX;
        public float HpValueTextPositionY { get; set; } = Defaults.HpValueTextPositionY;
        public float HpValueTextSize { get; set; } = Defaults.HpValueTextSize;
        public int HpValueTextAlignment { get; set; } = Defaults.HpValueTextAlignment;
        public NumberFormatStyle HpValueTextStyle { get; set; } = Defaults.HpValueTextStyle;
        public float HpDamageWobbleIntensity { get; set; } = Defaults.HpDamageWobbleIntensity;
        public int HpForFullRing { get; set; } = Defaults.HpForFullRing;
        public int MaximumHpForMaximumLength { get; set; } = Defaults.MaximumHpForMaximumLength;
        public int MinimumHpForLength { get; set; } = Defaults.MinimumHpForLength;
        public float HpPerPixelLongBar { get; set; } = Defaults.HpPerPixelLongBar;
        public int PvpHpForFullRing { get; set; } = Defaults.PvpHpForFullRing;
        public int PvpMaximumHpForMaximumLength { get; set; } = Defaults.PvpMaximumHpForMaximumLength;
        public int PvpMinimumHpForLength { get; set; } = Defaults.PvpMinimumHpForLength;
        public float PvpHpPerPixelLongBar { get; set; } = Defaults.PvpHpPerPixelLongBar;
        public float LowHpPercent { get; set; } = Defaults.LowHpPercent;
        public bool ShowHpRecovery { get; set; } = Defaults.ShowHpRecovery;
        public bool ShowHpVal { get; set; } = Defaults.ShowHpVal;
        public bool LengthByLevel { get; set; } = Defaults.LengthByLevel;
        public bool IgnoreRingForLevelScaling { get; set; } = Defaults.IgnoreRingForLevelScaling;
        public int HpPerLevel { get; set; } = Defaults.HpPerLevel;
        public bool PvpLengthByLevel { get; set; } = Defaults.PvpLengthByLevel;
        public bool PvpIgnoreRingForLevelScaling { get; set; } = Defaults.PvpIgnoreRingForLevelScaling;
        public int PvpHpPerLevel { get; set; } = Defaults.PvpHpPerLevel;
        public float HpAnimationSpeed { get; set; } = Defaults.HpAnimationSpeed;
        public float HpAnimationDelay { get; set; } = Defaults.HpAnimationDelay;

        public bool ShowShield { get; set; } = Defaults.ShowShield;
        public bool ShieldScalesWithLevel { get; set; } = Defaults.ShieldScalesWithLevel;
        public Vector4 ShieldColor { get; set; } = Defaults.ShieldColor;
        public float ShieldAnimationSpeed { get; set; } = Defaults.ShieldAnimationSpeed;
        public float ShieldAnimationDelay { get; set; } = Defaults.ShieldAnimationDelay;
        public int ShieldFillDirection { get; set; } = Defaults.ShieldFillDirection;

        public bool EnableRoleHpMultipliers { get; set; } = Defaults.EnableRoleHpMultipliers;

        public float TankHpMultiplier { get; set; } = Defaults.TankHpMultiplier;
        public bool EnableTankJobHpMultipliers { get; set; } = Defaults.EnableTankJobHpMultipliers;
        public float PldHpMultiplier { get; set; } = Defaults.PldHpMultiplier;
        public float WarHpMultiplier { get; set; } = Defaults.WarHpMultiplier;
        public float DrkHpMultiplier { get; set; } = Defaults.DrkHpMultiplier;
        public float GnbHpMultiplier { get; set; } = Defaults.GnbHpMultiplier;

        public float HealerHpMultiplier { get; set; } = Defaults.HealerHpMultiplier;
        public bool EnableHealerJobHpMultipliers { get; set; } = Defaults.EnableHealerJobHpMultipliers;
        public float WhmHpMultiplier { get; set; } = Defaults.WhmHpMultiplier;
        public float SchHpMultiplier { get; set; } = Defaults.SchHpMultiplier;
        public float AstHpMultiplier { get; set; } = Defaults.AstHpMultiplier;
        public float SgeHpMultiplier { get; set; } = Defaults.SgeHpMultiplier;

        public float MeleeHpMultiplier { get; set; } = Defaults.MeleeHpMultiplier;
        public bool EnableMeleeJobHpMultipliers { get; set; } = Defaults.EnableMeleeJobHpMultipliers;
        public float MnkHpMultiplier { get; set; } = Defaults.MnkHpMultiplier;
        public float DrgHpMultiplier { get; set; } = Defaults.DrgHpMultiplier;
        public float NinHpMultiplier { get; set; } = Defaults.NinHpMultiplier;
        public float SamHpMultiplier { get; set; } = Defaults.SamHpMultiplier;
        public float RprHpMultiplier { get; set; } = Defaults.RprHpMultiplier;
        public float VprHpMultiplier { get; set; } = Defaults.VprHpMultiplier;
        public float BstHpMultiplier { get; set; } = Defaults.BstHpMultiplier;

        public float RangedHpMultiplier { get; set; } = Defaults.RangedHpMultiplier;
        public bool EnableRangedJobHpMultipliers { get; set; } = Defaults.EnableRangedJobHpMultipliers;
        public float BrdHpMultiplier { get; set; } = Defaults.BrdHpMultiplier;
        public float MchHpMultiplier { get; set; } = Defaults.MchHpMultiplier;
        public float DncHpMultiplier { get; set; } = Defaults.DncHpMultiplier;
        public float BlmHpMultiplier { get; set; } = Defaults.BlmHpMultiplier;
        public float SmnHpMultiplier { get; set; } = Defaults.SmnHpMultiplier;
        public float RdmHpMultiplier { get; set; } = Defaults.RdmHpMultiplier;
        public float PctHpMultiplier { get; set; } = Defaults.PctHpMultiplier;
        public float BluHpMultiplier { get; set; } = Defaults.BluHpMultiplier;

        public float OtherHpMultiplier { get; set; } = Defaults.OtherHpMultiplier;
        #endregion

        #region Resource
        public bool ResourceBarEnabled { get; set; } = Defaults.ResourceBarEnabled;
        public float ResourceBarPositionX { get; set; } = Defaults.ResourceBarPositionX;
        public float ResourceBarPositionY { get; set; } = Defaults.ResourceBarPositionY;
        public float ResourceTextPositionX { get; set; } = Defaults.ResourceTextPositionX;
        public float ResourceTextPositionY { get; set; } = Defaults.ResourceTextPositionY;
        public float ResourceTextSize { get; set; } = Defaults.ResourceTextSize;
        public NumberFormatStyle ResourceTextStyle { get; set; } = Defaults.ResourceTextStyle;
        public int ResourceTextAlignment { get; set; } = Defaults.ResourceTextAlignment;
        public int MaximumMpLength { get; set; } = Defaults.MaximumMpLength;
        public int MinimumMpLength { get; set; } = Defaults.MinimumMpLength;
        public float MpPerPixelLength { get; set; } = Defaults.MpPerPixelLength;
        public float GpPerPixelLength { get; set; } = Defaults.GpPerPixelLength;
        public int MaximumGpLength { get; set; } = Defaults.MaximumGpLength;
        public int MinimumGpLength { get; set; } = Defaults.MinimumGpLength;
        public float CpPerPixelLength { get; set; } = Defaults.CpPerPixelLength;
        public int MaximumCpLength { get; set; } = Defaults.MaximumCpLength;
        public int MinimumCpLength { get; set; } = Defaults.MinimumCpLength;
        public bool TruncateMp { get; set; } = Defaults.TruncateMp;
        public bool ShowResourceVal { get; set; } = Defaults.ShowResourceVal;
        public bool ResourceLengthByLevel { get; set; } = Defaults.ResourceLengthByLevel;
        public int MpPerLevel { get; set; } = Defaults.MpPerLevel;
        public int GpPerLevel { get; set; } = Defaults.GpPerLevel;
        public int CpPerLevel { get; set; } = Defaults.CpPerLevel;
        public bool ShowResourceRecovery { get; set; } = Defaults.ShowResourceRecovery;
        public float ResourceAnimationSpeed { get; set; } = Defaults.ResourceAnimationSpeed;
        public float ResourceAnimationDelay { get; set; } = Defaults.ResourceAnimationDelay;

        public bool EnableRoleResourceMultipliers { get; set; } = Defaults.EnableRoleResourceMultipliers;
        public float TankResourceMultiplier { get; set; } = Defaults.TankResourceMultiplier;
        public bool EnableTankJobResourceMultipliers { get; set; } = Defaults.EnableTankJobResourceMultipliers;
        public float PldResourceMultiplier { get; set; } = Defaults.PldResourceMultiplier;
        public float WarResourceMultiplier { get; set; } = Defaults.WarResourceMultiplier;
        public float DrkResourceMultiplier { get; set; } = Defaults.DrkResourceMultiplier;
        public float GnbResourceMultiplier { get; set; } = Defaults.GnbResourceMultiplier;

        public float HealerResourceMultiplier { get; set; } = Defaults.HealerResourceMultiplier;
        public bool EnableHealerJobResourceMultipliers { get; set; } = Defaults.EnableHealerJobResourceMultipliers;
        public float WhmResourceMultiplier { get; set; } = Defaults.WhmResourceMultiplier;
        public float SchResourceMultiplier { get; set; } = Defaults.SchResourceMultiplier;
        public float AstResourceMultiplier { get; set; } = Defaults.AstResourceMultiplier;
        public float SgeResourceMultiplier { get; set; } = Defaults.SgeResourceMultiplier;

        public float MeleeResourceMultiplier { get; set; } = Defaults.MeleeResourceMultiplier;
        public bool EnableMeleeJobResourceMultipliers { get; set; } = Defaults.EnableMeleeJobResourceMultipliers;
        public float MnkResourceMultiplier { get; set; } = Defaults.MnkResourceMultiplier;
        public float DrgResourceMultiplier { get; set; } = Defaults.DrgResourceMultiplier;
        public float NinResourceMultiplier { get; set; } = Defaults.NinResourceMultiplier;
        public float SamResourceMultiplier { get; set; } = Defaults.SamResourceMultiplier;
        public float RprResourceMultiplier { get; set; } = Defaults.RprResourceMultiplier;
        public float VprResourceMultiplier { get; set; } = Defaults.VprResourceMultiplier;
        public float BstResourceMultiplier { get; set; } = Defaults.BstResourceMultiplier;

        public float RangedResourceMultiplier { get; set; } = Defaults.RangedResourceMultiplier;
        public bool EnableRangedJobResourceMultipliers { get; set; } = Defaults.EnableRangedJobResourceMultipliers;
        public float BrdResourceMultiplier { get; set; } = Defaults.BrdResourceMultiplier;
        public float MchResourceMultiplier { get; set; } = Defaults.MchResourceMultiplier;
        public float DncResourceMultiplier { get; set; } = Defaults.DncResourceMultiplier;
        public float BlmResourceMultiplier { get; set; } = Defaults.BlmResourceMultiplier;
        public float SmnResourceMultiplier { get; set; } = Defaults.SmnResourceMultiplier;
        public float RdmResourceMultiplier { get; set; } = Defaults.RdmResourceMultiplier;
        public float PctResourceMultiplier { get; set; } = Defaults.PctResourceMultiplier;
        public float BluResourceMultiplier { get; set; } = Defaults.BluResourceMultiplier;

        public float OtherResourceMultiplier { get; set; } = Defaults.OtherResourceMultiplier;

        public Vector4 ResourceSpentColor { get; set; } = Defaults.ResourceSpentColor;
        public Vector4 ResourceRecoveredColor { get; set; } = Defaults.ResourceRecoveredColor;
        #endregion

        #region Limit Break

        public bool LimitBarEnabled { get; set; } = Defaults.LimitBarEnabled;
        public bool LimitGaugeAlwaysShow { get; set; } = Defaults.LimitGaugeAlwaysShow;
        public bool LimitGaugeDiadem { get; set; } = Defaults.LimitGaugeDiadem;
        public float LimitGaugePositionX { get; set; } = Defaults.LimitGaugePositionX;
        public float LimitGaugePositionY { get; set; } = Defaults.LimitGaugePositionY;

        #endregion

        #region Job Ring

        public bool JobRingEnabled { get; set; } = Defaults.JobRingEnabled;
        public float JobRingScaleMultiplier { get; set; } = Defaults.JobRingScaleMultiplier;
        public float JobRingOffsetX { get; set; } = Defaults.JobRingOffsetX;
        public float JobRingOffsetY { get; set; } = Defaults.JobRingOffsetY;
        public bool JobRingShowSegments { get; set; } = Defaults.JobRingShowSegments;
        public bool JobRingWrapFull { get; set; } = Defaults.JobRingWrapFull;
        public float JobRingMaxAngle { get; set; } = Defaults.JobRingMaxAngle;

        // Animation settings
        public bool JobRingAnimationEnabled { get; set; } = Defaults.JobRingAnimationEnabled;
        public float JobRingAnimationSpeed { get; set; } = Defaults.JobRingAnimationSpeed;
        public float JobRingAnimationDelay { get; set; } = Defaults.JobRingAnimationDelay;
        public bool JobRingShowDamageTrail { get; set; } = Defaults.JobRingShowDamageTrail;

        public float JobRingOutlineThickness { get; set; } = Defaults.JobRingOutlineThickness;
        public float JobRingRadius { get; set; } = Defaults.JobRingRadius;
        public float JobRingWidth { get; set; } = Defaults.JobRingWidth;
        public float JobRingStartAngle { get; set; } = Defaults.JobRingStartAngle;

        #endregion

        #region Experience

        public bool ExpBarEnabled { get; set; } = Defaults.ExpBarEnabled;
        public bool ExpValueTextEnabled { get; set; } = Defaults.ExpValueTextEnabled;
        public float ExpValueTextSize { get; set; } = Defaults.ExpValueTextSize;
        public int ExpValueTextAlignment { get; set; } = Defaults.ExpValueTextAlignment;
        public NumberFormatStyle ExpValueTextFormatStyle { get; set; } = Defaults.ExpValueTextFormatStyle;
        public float ExpValueTextPositionX { get; set; } = Defaults.ExpValueTextPositionX;
        public float ExpValueTextPositionY { get; set; } = Defaults.ExpValueTextPositionY;

        #endregion

        #region ClassInfo

        public bool LevelEnabled { get; set; } = Defaults.LevelEnabled;
        public bool ClassIconEnabled { get; set; } = Defaults.ClassIconEnabled;

        public float LevelTextX { get; set; } = Defaults.LevelTextX;
        public float LevelTextY { get; set; } = Defaults.LevelTextY;
        public float LevelTextSize { get; set; } = Defaults.LevelTextSize;
        public TextAlignment LevelTextAlignment { get; set; } = Defaults.LevelTextAlignment;
        public float LevelTextScale { get; set; } = Defaults.LevelTextScale;

        public float ClassIconX { get; set; } = Defaults.ClassIconX;
        public float ClassIconY { get; set; } = Defaults.ClassIconY;
        public float ClassIconScale { get; set; } = Defaults.ClassIconScale;

        #endregion

        [NonSerialized]
        private IDalamudPluginInterface _pluginInterface = null!;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this._pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this._pluginInterface.SavePluginConfig(this);
        }
    }
}