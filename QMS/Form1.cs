using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace QMS
{

    public record Config
    {
        public string ServerUri { get; init; } = "http://127.0.0.1:5062/";
    }

    public partial class Form1 : Form
    {
        private readonly QueueTicketService _ticketService;
        private Config _config;

        private readonly HttpClient _httpClient;
        public Form1()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            InitializeComponent();
            _config = LoadConfig();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_config.ServerUri)
            };

            _ticketService = new QueueTicketService(_httpClient);
        }

        
        private async void PrintButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null) { button.Enabled = false; }

            var image = await _ticketService.GenerateTicketImageAsync();

            if (image != null)
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A9);
                        page.Content().AlignCenter().AlignMiddle().Image(image);
                    });
                });

                document.GeneratePdf("output.pdf");
            }

            if (button != null) { button.Enabled = true; }

        }

        private Config LoadConfig()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "config.json");

            if (!File.Exists(path))
            {
                var defaultConfig = new Config();

                string json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(path, json);

                return defaultConfig;
            }

            string fileJson = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(fileJson) ?? new Config();
        }
    }
}

