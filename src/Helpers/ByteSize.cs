namespace OptimizeRK.Helpers;

public static class ByteSize {
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB", "PB"];

    public static string Format(long bytes, int decimals = 2, bool useBinary = true) {
        double size = bytes;
        int unit = 0;
        double step = useBinary ? 1024d : 1000d;

        while (size >= step && unit < Units.Length - 1) {
            size /= step;
            unit++;
        }

        string fmt = decimals <= 0 ? "0" : "0." + new string('#', decimals);
        return $"{size.ToString(fmt)} {Units[unit]}";
    }
}