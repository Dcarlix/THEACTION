using Microsoft.Extensions.Logging;
using ActionIA.Interfaces;

#if ANDROID
using ActionIA.Platforms.Android;
#elif IOS
using ActionIA.Platforms.iOS;
#elif WINDOWS
using ActionIA.Platforms.Windows;
#endif

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
				fonts.AddFont("vcr.ttf", "Balatro");
			});

		// 📣 Registro de servicios multiplataforma
		RegisterPlatformServices(builder);

		// 🧩 Páginas y clases principales
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<App>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	/// <summary>
	/// Registra los servicios específicos según la plataforma.
	/// </summary>
	private static void RegisterPlatformServices(MauiAppBuilder builder)
	{
#if ANDROID
        builder.Services.AddSingleton<ISpeechToText, SpeechToTextImplementation>();
#elif IOS
        builder.Services.AddSingleton<ISpeechToText, SpeechToTextImplementation>();
#elif WINDOWS
		builder.Services.AddSingleton<ISpeechToText, SpeechToTextService>();
#else
        // ⚠️ Si alguna plataforma no está implementada
        builder.Services.AddSingleton<ISpeechToText, NotImplementedSpeechService>();
#endif
	}
}

/// <summary>
/// Implementación de respaldo para plataformas no soportadas.
/// </summary>
public class NotImplementedSpeechService : ISpeechToText
{
	public Task<string> RecognizeSpeechAsync(string locale = "es-ES")
	{
		return Task.FromResult("Reconocimiento de voz no soportado en esta plataforma.");
	}
}
