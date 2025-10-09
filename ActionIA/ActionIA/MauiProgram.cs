using Microsoft.Extensions.Logging;
#if ANDROID
using ActionIA.Platforms.Android;
#elif IOS
using ActionIA.Platforms.iOS;
#endif
using ActionIA.Interfaces;

namespace ActionIA;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
#if ANDROID
        builder.Services.AddSingleton<ISpeechToText, SpeechToTextImplementation>();
#elif IOS
        builder.Services.AddSingleton<ISpeechToText, SpeechToTextImplementation>();
#endif


#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
