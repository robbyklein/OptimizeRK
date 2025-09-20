namespace OptimizeRK.Models;

/// <summary>
/// Supported output scales for video optimization.
/// </summary>
public enum VideoScale {
    /// <summary>Keep the original resolution.</summary>
    Original = 0,

    /// <summary>Scale video height to 3840 pixels.</summary>
    Scale3840 = 3840,

    /// <summary>Scale video height to 2560 pixels.</summary>
    Scale2560 = 2560,

    /// <summary>Scale video height to 1920 pixels.</summary>
    Scale1920 = 1920,

    /// <summary>Scale video height to 1280 pixels.</summary>
    Scale1280 = 1280,

    /// <summary>Scale video height to 1024 pixels.</summary>
    Scale1024 = 1024,

    /// <summary>Scale video height to 720 pixels.</summary>
    Scale720 = 720,

    /// <summary>Scale video height to 480 pixels.</summary>
    Scale480 = 480,
}
