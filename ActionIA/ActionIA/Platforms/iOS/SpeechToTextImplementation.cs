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

			var recognizer = new SFSpeechRecognizer(new NSLocale(locale));
			var request = new SFSpeechAudioBufferRecognitionRequest();
			var audioEngine = new AVAudioEngine();

			recognizer.GetRecognitionTask(request, (result, error) =>
			{
				if (result != null && result.Final)
				{
					tcs.TrySetResult(result.BestTranscription.FormattedString);
					audioEngine.Stop();
				}
				else if (error != null)
				{
					tcs.TrySetResult($"Error: {error.LocalizedDescription}");
					audioEngine.Stop();
				}
			});

			var inputNode = audioEngine.InputNode;
			var recordingFormat = inputNode.GetBusOutputFormat(0);
			inputNode.InstallTapOnBus(0, 1024, recordingFormat, (buffer, when) =>
			{
				request.Append(buffer);
			});

			audioEngine.Prepare();
			audioEngine.StartAndReturnError(out _);

			return await tcs.Task;
		}

	}
}
#endif