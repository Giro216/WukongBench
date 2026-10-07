using System.Text.RegularExpressions;
using WukongBench.model;

namespace WukongBench.service;

public class IniService
{
    private static readonly Dictionary<string, string> ScalabilityMappings = new()
    {
        ["ViewDistance"] = "sg.ViewDistanceQuality",
        ["AntiAliasing"] = "sg.AntiAliasingQuality",
        ["PostProcessing"] = "sg.PostProcessQuality",
        ["ShadowQuality"] = "sg.ShadowQuality",
        ["TextureQuality"] = "sg.TextureQuality",
        ["FxQuality"] = "sg.EffectsQuality",
        ["MaterialQuality"] = "sg.ShadingQuality",
        ["VegetationQuality"] = "sg.FoliageQuality",
        ["GlobalIllumination"] = "sg.GlobalIlluminationQuality",
        ["ReflectionQuality"] = "sg.ReflectionQuality"
    };

    public string ApplyProfile(string ini, BenchmarkSettings settings)
    {
        ini = SetUiSetting(ini, "QualityLevel", settings.QualityLevel);
        ini = SetUiSetting(ini, "ImageQuality", settings.ImageQuality);
        ini = SetUiSetting(ini, "Rtx", settings.Rtx);
        ini = SetUiSetting(ini, "Dlss", settings.Dlss);
        ini = SetUiSetting(ini, "InsertFrame", settings.InsertFrame);
        ini = SetUiSetting(ini, "MotionBlur", settings.MotionBlur);

        ini = SetUiSetting(ini, "ViewDistance", settings.ViewDistance);
        ini = SetUiSetting(ini, "AntiAliasing", settings.AntiAliasing);
        ini = SetUiSetting(ini, "PostProcessing", settings.PostProcessing);
        ini = SetUiSetting(ini, "ShadowQuality", settings.ShadowQuality);
        ini = SetUiSetting(ini, "TextureQuality", settings.TextureQuality);
        ini = SetUiSetting(ini, "MaterialQuality", settings.MaterialQuality);
        ini = SetUiSetting(ini, "VegetationQuality", settings.VegetationQuality);
        ini = SetUiSetting(ini, "FxQuality", settings.FxQuality);

        foreach (var (setting, iniKey) in ScalabilityMappings)
        {
            int value = GetSettingValue(settings, setting);
            ini = SetLine(ini, iniKey, (value - 1).ToString());
        }

        ini = SetLine(
            ini,
            "sg.ResolutionQuality",
            settings.ImageQuality.ToString());

        ini = SetResolution(
            ini,
            settings.ResolutionWidth,
            settings.ResolutionHeight);

        ini = SetLine(ini, "bNeverShowStartupUI", "True");
        ini = SetLine(ini, "bUseVSync", "False");
        ini = SetLine(ini, "FrameRateLimit", "0.000000");

        return ini;
    }

    private static string SetUiSetting(
        string ini,
        string name,
        int value)
    {
        return Regex.Replace(
            ini,
            $"\\(\"{Regex.Escape(name)}\",\\s*\"[^\"]*\"\\)",
            $"(\"{name}\", \"{value}\")");
    }

    private static string SetLine(
        string ini,
        string key,
        string value)
    {
        return Regex.Replace(
            ini,
            $"(?m)^{Regex.Escape(key)}=[^\\r\\n]*",
            $"{key}={value}");
    }

    private static string SetResolution(
        string ini,
        int width,
        int height)
    {
        ini = SetLine(ini, "ResolutionSizeX", width.ToString());
        ini = SetLine(ini, "ResolutionSizeY", height.ToString());

        ini = SetLine(
            ini,
            "LastUserConfirmedResolutionSizeX",
            width.ToString());

        ini = SetLine(
            ini,
            "LastUserConfirmedResolutionSizeY",
            height.ToString());

        ini = SetLine(
            ini,
            "DesiredScreenWidth",
            width.ToString());

        ini = SetLine(
            ini,
            "DesiredScreenHeight",
            height.ToString());

        ini = SetLine(
            ini,
            "LastUserConfirmedDesiredScreenWidth",
            width.ToString());

        ini = SetLine(
            ini,
            "LastUserConfirmedDesiredScreenHeight",
            height.ToString());

        return ini;
    }

    private static int GetSettingValue(
        BenchmarkSettings settings,
        string name)
    {
        return name switch
        {
            "ViewDistance" => settings.ViewDistance,
            "AntiAliasing" => settings.AntiAliasing,
            "PostProcessing" => settings.PostProcessing,
            "ShadowQuality" => settings.ShadowQuality,
            "TextureQuality" => settings.TextureQuality,
            "FxQuality" => settings.FxQuality,
            "MaterialQuality" => settings.MaterialQuality,
            "VegetationQuality" => settings.VegetationQuality,
            "GlobalIllumination" => settings.GlobalIllumination,
            "ReflectionQuality" => settings.ReflectionQuality,

            _ => throw new ArgumentException(
                $"Unknown setting: {name}")
        };
    }
}