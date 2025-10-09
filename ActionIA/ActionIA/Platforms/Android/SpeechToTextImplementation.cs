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
using global::Android.OS;
using global::Android.Runtime;
using global::Android.Speech;

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

			var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
			intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
			intent.PutExtra(RecognizerIntent.ExtraLanguage, locale);
			intent.PutExtra(RecognizerIntent.ExtraPrompt, "Habla ahora...");

			_speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Application.Context);
			_speechRecognizer.SetRecognitionListener(this);
			_speechRecognizer.StartListening(intent);

			return _tcs.Task;
		}

		public void OnResults(Bundle results)
		{
			var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
			_result = matches?[0] ?? "";
			_tcs.TrySetResult(_result);
		}

		public void OnError([GeneratedEnum] SpeechRecognizerError error)
		{
			_tcs.TrySetResult($"Error: {error}");
		}

		// Métodos requeridos vacíos
		public void OnReadyForSpeech(Bundle @params) { }
		public void OnBeginningOfSpeech() { }
		public void OnRmsChanged(float rmsdB) { }
		public void OnBufferReceived(byte[] buffer) { }
		public void OnEndOfSpeech() { }
		public void OnPartialResults(Bundle partialResults) { }
		public void OnEvent(int eventType, Bundle @params) { }
	}
#endif

}
