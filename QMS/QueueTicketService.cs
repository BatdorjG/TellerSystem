using System.Drawing;
using System.Drawing.Imaging;

namespace QMS;

public class QueueTicketService
{
    private readonly HttpClient _httpClient;

    public QueueTicketService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]?> GenerateTicketImageAsync()
    {
        var response = await _httpClient.GetStringAsync("CustomerQueue");

        if (!byte.TryParse(response, out byte value))
            return null;

        using var image = new Bitmap(200, 100);

        using (var graphics = Graphics.FromImage(image))
        {
            graphics.Clear(Color.White);

            using var font = new Font("Arial", 48, FontStyle.Bold);

            var textSize = graphics.MeasureString(value.ToString(), font);

            var position = new PointF(
                (image.Width - textSize.Width) / 2,
                (image.Height - textSize.Height) / 2
            );

            graphics.DrawString(value.ToString(), font, Brushes.Black, position);
        }

        using var ms = new MemoryStream();

        image.Save(ms, ImageFormat.Png);

        return ms.ToArray();
    }
}