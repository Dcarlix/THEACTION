using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using Android.Speech;
using ActionIA.Interfaces;
using Application = Android.App.Application;
using global::Android.Runtime;

namespace ActionIA.Platforms.Android
{
	public class SpeechToTextImplementation : Java.Lang.Object, ISpeechToText, IRecognitionListener
	{
		private TaskCompletionSource<string> _tcs;
		private SpeechRecognizer _speechRecognizer;
		private string _result;

		public Task<string> RecognizeSpeechAsync(string locale = "es-ES")
		{
			_tcs = new TaskCompletionSource<string>();

			try
			{
				if (!SpeechRecognizer.IsRecognitionAvailable(Application.Context))
				{
					_tcs.TrySetResult("Reconocimiento de voz no disponible en este dispositivo.");
					return _tcs.Task;
				}

				// Crear intent de reconocimiento
				var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
				intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

				// Se fuerza el idioma y se da preferencia al indicado
				var javaLocale = new Java.Util.Locale(locale);
				intent.PutExtra(RecognizerIntent.ExtraLanguage, javaLocale.ToString());
				intent.PutExtra(RecognizerIntent.ExtraLanguagePreference, javaLocale.ToString());
				intent.PutExtra(RecognizerIntent.ExtraCallingPackage, Application.Context.PackageName);

				// Mensaje que se mostrará al iniciar la voz
				intent.PutExtra(RecognizerIntent.ExtraPrompt, "🎤 Habla ahora...");

				// Crear e iniciar el reconocedor
				_speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Application.Context);
				_speechRecognizer.SetRecognitionListener(this);
				_speechRecognizer.StartListening(intent);
			}
			catch (Exception ex)
			{
				_tcs.TrySetException(ex);
			}

			return _tcs.Task;
		}

		// ✅ Devuelve el texto reconocido
		public void OnResults(Bundle results)
		{
			var matches = results?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
			_result = matches != null && matches.Count > 0 ? matches[0] : "";
			_tcs.TrySetResult(_result);
		}

		// ✅ Manejo de errores más claro
		public void OnError([GeneratedEnum] SpeechRecognizerError error)
		{
			string message = error switch
			{
				SpeechRecognizerError.NetworkTimeout => "Error de red (tiempo agotado).",
				SpeechRecognizerError.Network => "Error de conexión.",
				SpeechRecognizerError.Audio => "Error de audio.",
				SpeechRecognizerError.NoMatch => "No se reconoció ninguna voz.",
				SpeechRecognizerError.RecognizerBusy => "El reconocedor ya está en uso.",
				SpeechRecognizerError.InsufficientPermissions => "Permisos insuficientes para usar el micrófono.",
				_ => $"Error desconocido: {error}"
			};

			_tcs.TrySetResult(message);
		}

		// Métodos requeridos (no usados)
		public void OnReadyForSpeech(Bundle @params) { }
		public void OnBeginningOfSpeech() { }
		public void OnRmsChanged(float rmsdB) { }
		public void OnBufferReceived(byte[] buffer) { }
		public void OnEndOfSpeech() { }
		public void OnPartialResults(Bundle partialResults) { }
		public void OnEvent(int eventType, Bundle @params) { }
	}
}
#endif

