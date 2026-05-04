using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Companion;

namespace QMS
{
    public partial class Form1 : Form
    {
        private TempQueue tempQueue = new TempQueue();

        public Form1()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            InitializeComponent();
        }

        private byte[] GenerateValue()
        {
            var value = tempQueue.Give();

            var image = new Bitmap (200, 100);

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


        private void PrintButton_Click(object sender, EventArgs e)
        {
            var image = GenerateValue();

            if (image != null) {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A8);
                        page.Content().AlignCenter().AlignMiddle().Image(image);
                    });
                });

                document.GeneratePdf("output.pdf");
            }
        }
    }
}
