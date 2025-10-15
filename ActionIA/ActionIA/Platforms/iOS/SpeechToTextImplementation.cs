using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if IOS
using AVFoundation;
using Foundation;
using Speech;
using ActionIA.Interfaces;

namespace ActionIA.Platforms.iOS
{
	public class SpeechToTextImplementation : ISpeechToText
	{
		public async Task<string> RecognizeSpeechAsync(string locale = "es-ES")
		{
			var tcs = new TaskCompletionSource<string>();

			// 1️⃣ Solicitar permisos (forma moderna)
			var authStatus = await RequestSpeechAuthorizationAsync();
			if (authStatus != SFSpeechRecognizerAuthorizationStatus.Authorized)
			{
				tcs.TrySetResult("Permiso para reconocimiento de voz denegado.");
				return await tcs.Task;
			}

			// 2️⃣ Configurar sesión de audio correctamente
			var audioSession = AVAudioSession.SharedInstance();
			NSError error = null;

			audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker, out error);
			audioSession.SetMode(AVAudioSessionMode.Default, out error);
			audioSession.SetActive(true, out error);

			// 3️⃣ Crear reconocedor
			var recognizer = new SFSpeechRecognizer(new NSLocale(locale));
			if (recognizer == null || !recognizer.Available)
			{
				tcs.TrySetResult($"Reconocedor no disponible para el idioma {locale}.");
				return await tcs.Task;
			}

			var request = new SFSpeechAudioBufferRecognitionRequest();
			var audioEngine = new AVAudioEngine();

			// 4️⃣ Configurar captura del micrófono
			var inputNode = audioEngine.InputNode;
			var recordingFormat = inputNode.GetBusOutputFormat(0);
			inputNode.InstallTapOnBus(0, 1024, recordingFormat, (buffer, when) =>
			{
				request.Append(buffer);
			});

			// 5️⃣ Configurar reconocimiento
			recognizer.GetRecognitionTask(request, (result, err) =>
			{
				if (result != null && result.Final)
				{
					tcs.TrySetResult(result.BestTranscription.FormattedString);
					inputNode.RemoveTapOnBus(0);
					audioEngine.Stop();
					request.EndAudio();
					audioSession.SetActive(false, out _);
				}
				else if (err != null)
				{
					tcs.TrySetResult($"Error: {err.LocalizedDescription}");
					inputNode.RemoveTapOnBus(0);
					audioEngine.Stop();
					request.EndAudio();
					audioSession.SetActive(false, out _);
				}
			});

			// 6️⃣ Iniciar el motor de audio
			audioEngine.Prepare();
			audioEngine.StartAndReturnError(out error);
			if (error != null)
				tcs.TrySetResult($"Error al iniciar audio: {error.LocalizedDescription}");

			return await tcs.Task;
		}

		// 🔧 Método auxiliar porque no existe RequestAuthorizationAsync en .NET MAUI
		private Task<SFSpeechRecognizerAuthorizationStatus> RequestSpeechAuthorizationAsync()
		{
			var tcs = new TaskCompletionSource<SFSpeechRecognizerAuthorizationStatus>();
			SFSpeechRecognizer.RequestAuthorization(status => tcs.TrySetResult(status));
			return tcs.Task;
		}
	}
}
#endif