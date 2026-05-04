using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Companion;

namespace QMS
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:5062/")
        };
        public Form1()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            InitializeComponent();
        }

        private async Task<byte[]> GenerateValue()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("CustomerQueue");
                if (!byte.TryParse(response, out byte value))
                {
                    return null;
                }

                var image = new Bitmap(200, 100);

                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.Clear(System.Drawing.Color.White);
                    using (var font = new Font("Arial", 48, FontStyle.Bold))
                    {
                        var textSize = graphics.MeasureString(value.ToString(), font);
                        var position = new PointF((image.Width - textSize.Width) / 2, (image.Height - textSize.Height) / 2);
                        graphics.DrawString(value.ToString(), font, Brushes.Black, position);
                    }
                }

                using (var ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }

            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error fetching value: {ex.Message}");
                return null;
            }
        }


        private async void PrintButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null) { button.Enabled = false; }

            var image = await GenerateValue();

            if (image != null) {
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
    }
}
