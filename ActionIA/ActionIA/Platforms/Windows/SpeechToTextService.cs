using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS
using System.Speech.Recognition;
using System.Globalization;
using ActionIA.Interfaces;

namespace ActionIA.Platforms.Windows
{
	public class SpeechToTextService : ISpeechToText
	{
		private SpeechRecognitionEngine _recognizer;

		public async Task<string> RecognizeSpeechAsync(string locale = "es-ES")
		{
			var tcs = new TaskCompletionSource<string>();

			try
			{
				// 1️⃣ Verificar si el idioma existe
				var culture = TryGetCulture(locale);
				if (culture == null)
				{
					tcs.TrySetResult($"Idioma '{locale}' no está instalado en el sistema.");
					return await tcs.Task;
				}

				// 2️⃣ Inicializar el motor
				_recognizer = new SpeechRecognitionEngine(culture);

				// 3️⃣ Cargar gramática
				_recognizer.LoadGrammar(new DictationGrammar());

				// 4️⃣ Configurar dispositivo de entrada
				_recognizer.SetInputToDefaultAudioDevice();

				// 5️⃣ Manejadores de eventos
				_recognizer.SpeechRecognized += (s, e) =>
				{
					if (e.Result != null && !string.IsNullOrWhiteSpace(e.Result.Text))
						tcs.TrySetResult(e.Result.Text);
				};

				_recognizer.RecognizeCompleted += (s, e) =>
				{
					if (e.Error != null)
						tcs.TrySetResult($"Error: {e.Error.Message}");
					else if (e.Cancelled)
						tcs.TrySetResult("Reconocimiento cancelado.");
				};

				// 6️⃣ Ejecutar reconocimiento en un hilo separado
				await Task.Run(() => _recognizer.RecognizeAsync(RecognizeMode.Single));

			}
			catch (InvalidOperationException ex)
			{
				tcs.TrySetResult($"Error de dispositivo de audio: {ex.Message}");
			}
			catch (Exception ex)
			{
				tcs.TrySetResult($"Error general: {ex.Message}");
			}

			return await tcs.Task;
		}

		// 🔧 Helper: Validar si el idioma está disponible
		private CultureInfo TryGetCulture(string locale)
		{
			try
			{
				var culture = new CultureInfo(locale);
				var installed = SpeechRecognitionEngine.InstalledRecognizers();

				if (installed.Any(r => r.Culture.Name == culture.Name))
					return culture;

				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}
#endif
