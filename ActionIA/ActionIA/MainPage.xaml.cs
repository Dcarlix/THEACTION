
using ActionIA.Interfaces;
using Microsoft.Maui.ApplicationModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ActionIA
{
    public partial class MainPage : ContentPage
    {
		private readonly ISpeechToText _speech;
		private Dictionary<string, string> _languages = new()
		{
			{ "Español", "es-ES" },
			{ "Inglés", "en-US" },
			{ "Chino", "zh-CN" },
			{ "Japonés", "ja-JP" },
			{ "Portugués", "pt-PT" },
			{ "Francés", "fr-FR" },
			{ "Italiano", "it-IT" },
			{ "Alemán", "de-DE" }
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
			try
			{
				await Permissions.RequestAsync<Permissions.Microphone>();
				var text = await _speech.RecognizeSpeechAsync(_languages[FromLangPicker.SelectedItem.ToString()]);
				InputEditor.Text = text;
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Error al grabar audio: {ex.Message}", "OK");
			}
		}

		private void OnSwapClicked(object sender, EventArgs e)
		{
			var temp = FromLangPicker.SelectedIndex;
			FromLangPicker.SelectedIndex = ToLangPicker.SelectedIndex;
			ToLangPicker.SelectedIndex = temp;
		}
		private async void OnTranslateClicked(object sender, EventArgs e)
		{

			try
			{
				// Obtener idioma origen y destino desde los pickers
				var fromLangFull = _languages[FromLangPicker.SelectedItem.ToString()];
				var toLangFull = _languages[ToLangPicker.SelectedItem.ToString()];

				// Convertir a códigos cortos (es, en, fr, etc.)
				var fromLang = fromLangFull.Split('-')[0];
				var toLang = toLangFull.Split('-')[0];

				// Obtener texto a traducir
				var textToTranslate = InputEditor.Text?.Trim();
				if (string.IsNullOrWhiteSpace(textToTranslate))
				{
					await DisplayAlert("Aviso", "Por favor ingrese texto para traducir.", "OK");
					return;
				}

				// Construir la URL de MyMemory API
				var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(textToTranslate)}" +
						  $"&langpair={fromLang}|{toLang}" +
						  $"&de=demo@mymemory.ai&use_auto_detect=0";

				using var client = new HttpClient();
				var response = await client.GetAsync(url);
				var raw = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					await DisplayAlert("Error", $"Error en la API: {response.StatusCode}\n{raw}", "OK");
					return;
				}

				// Analizar la respuesta JSON
				var json = System.Text.Json.JsonDocument.Parse(raw);
				var translatedText = json.RootElement
					.GetProperty("responseData")
					.GetProperty("translatedText")
					.GetString();

				// Mostrar traducción en el editor de salida
				OutputEditor.Text = translatedText ?? "Error al traducir texto.";
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"No se pudo traducir el texto:\n{ex.Message}", "OK");
			}
		}

		public class TranslationResponse
		{
			[JsonPropertyName("translatedText")]
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
