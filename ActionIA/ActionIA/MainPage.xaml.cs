
using ActionIA.Interfaces;
using Microsoft.Maui.ApplicationModel;
using System.Net.Http.Json;

namespace ActionIA
{
    public partial class MainPage : ContentPage
    {
		private readonly ISpeechToText _speech;
		private Dictionary<string, string> _languages = new()
{
	{ "Español", "es" },
	{ "Inglés", "en" },
	{ "Chino", "zh" },
	{ "Japonés", "ja" },
	{ "Portugués", "pt" },
	{ "Francés", "fr" },
	{ "Italiano", "it" },
	{ "Alemán", "de" }
};

		public MainPage(ISpeechToText speech)
        {
            InitializeComponent();
			_speech = speech;
			FromLangPicker.ItemsSource = _languages.Keys.ToList();
			ToLangPicker.ItemsSource = _languages.Keys.ToList();

			FromLangPicker.SelectedIndex = 0; // Español por defecto
			ToLangPicker.SelectedIndex = 1;   // Inglés por defecto

		}
		private async void OnListenClicked(object sender, EventArgs e)
		{
			await Permissions.RequestAsync<Permissions.Microphone>();
			var text = await _speech.RecognizeSpeechAsync(_languages[FromLangPicker.SelectedItem.ToString()]);
			InputEditor.Text = text;
		}

		private void OnSwapClicked(object sender, EventArgs e)
		{
			var temp = FromLangPicker.SelectedIndex;
			FromLangPicker.SelectedIndex = ToLangPicker.SelectedIndex;
			ToLangPicker.SelectedIndex = temp;
		}
		private async void OnTranslateClicked(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(InputEditor.Text)) return;

			var fromLang = _languages[FromLangPicker.SelectedItem.ToString()];
			var toLang = _languages[ToLangPicker.SelectedItem.ToString()];

			using var client = new HttpClient();
			var values = new Dictionary<string, string>
	{
		{ "q", InputEditor.Text },
		{ "source", fromLang },
		{ "target", toLang },
		{ "format", "text" }
	};

			var content = new FormUrlEncodedContent(values);
			var response = await client.PostAsync("https://libretranslate.de/translate", content);
			var result = await response.Content.ReadFromJsonAsync<TranslationResponse>();

			OutputEditor.Text = result?.TranslatedText ?? "Error en traducción";
		}

		public class TranslationResponse
		{
			public string TranslatedText { get; set; }
		}

		private async void OnSpeakClicked(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(OutputEditor.Text)) return;

			var toLang = _languages[ToLangPicker.SelectedItem.ToString()];
			await TextToSpeech.Default.SpeakAsync(OutputEditor.Text, new SpeechOptions
			{
				Volume = 1.0f,
				Pitch = 1.0f
			});
		}




	}

}
