namespace ActionIA
{
    public partial class App : Application
    {
        public App(MainPage mainPage)
        {

			try
			{
				InitializeComponent();
				MainPage = mainPage;

			}
			catch (Exception ex)
			{
				System.IO.File.WriteAllText("error_log.txt", ex.ToString());
				throw;
			}
		}
    }
}
