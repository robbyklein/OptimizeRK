namespace OptimizeRK.Models;

/// <summary>
/// Supported output scales for video optimization.
/// </summary>
public enum VideoScale {
    /// <summary>Keep the original resolution.</summary>
    Original = 0,

    /// <summary>Limit video width to 3840 pixels (smaller videos are left as is).</summary>
    Scale3840 = 3840,

    /// <summary>Limit video width to 2560 pixels (smaller videos are left as is).</summary>
    Scale2560 = 2560,

    /// <summary>Limit video width to 1920 pixels (smaller videos are left as is).</summary>
    Scale1920 = 1920,

    /// <summary>Limit video width to 1280 pixels (smaller videos are left as is).</summary>
    Scale1280 = 1280,

    /// <summary>Limit video width to 1024 pixels (smaller videos are left as is).</summary>
    Scale1024 = 1024,

    /// <summary>Limit video width to 720 pixels (smaller videos are left as is).</summary>
    Scale720 = 720,

    /// <summary>Limit video width to 480 pixels (smaller videos are left as is).</summary>
    Scale480 = 480,
}
