using Avalonia.Data.Converters;
using OptimizeRK.Models;
using System;
using System.Globalization;

namespace OptimizeRK.Converters;

public class StatusToIconConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		return value switch {
			ProcessStatus.Pending => "⏳",
			ProcessStatus.Processing => "🔄",
			ProcessStatus.Optimized => "✔️",
			ProcessStatus.Failed => "❌",
			_ => "?"
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}