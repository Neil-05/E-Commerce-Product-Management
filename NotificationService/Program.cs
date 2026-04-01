using NotificationService.Dto;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using MailKit.Net.Smtp;
using MimeKit;

var factory = new ConnectionFactory() { HostName = "localhost" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(
    queue: "product_published",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += async (model, ea) =>
{
    try
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        var data = JsonSerializer.Deserialize<ProductPublishedEvent>(message);

        if (data == null)
        {
            Console.WriteLine("❌ Failed to deserialize message");
            return;
        }

        Console.WriteLine("📩 Sending REAL Email...");
        Console.WriteLine($"Product: {data.ProductId}");
        Console.WriteLine($"To: {data.Email}");

        // ✅ CREATE EMAIL
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("E-Commerce App", "yourgmail@gmail.com"));
        mimeMessage.To.Add(new MailboxAddress("", data.Email));
        mimeMessage.Subject = "Product Published";

        mimeMessage.Body = new TextPart("html")
        {
            Text = $@"
    <div style='font-family: Arial, sans-serif; background-color:#f4f4f4; padding:20px;'>
        <div style='max-width:600px; margin:auto; background:white; border-radius:10px; overflow:hidden; box-shadow:0 0 10px rgba(0,0,0,0.1);'>
            
            <div style='background:#4CAF50; color:white; padding:15px; text-align:center;'>
                <h2>🎉 Product Published</h2>
            </div>

            <div style='padding:20px;'>
                <p>Hi,</p>
                
                <p>Your product has been <strong style='color:green;'>successfully published</strong>.</p>
                
                <table style='width:100%; margin-top:15px; border-collapse:collapse;'>
                    <tr>
                        <td style='padding:8px; font-weight:bold;'>Product ID:</td>
                        <td style='padding:8px;'>{data.ProductId}</td>
                    </tr>
                    <tr>
                        <td style='padding:8px; font-weight:bold;'>Status:</td>
                        <td style='padding:8px; color:green;'>Published</td>
                    </tr>
                    <tr>
                        <td style='padding:8px; font-weight:bold;'>Date:</td>
                        <td style='padding:8px;'>{DateTime.UtcNow:yyyy-MM-dd HH:mm}</td>
                    </tr>
                </table>

                <p style='margin-top:20px;'>You can now view your product live on the platform.</p>

                <div style='text-align:center; margin-top:25px;'>
                    <a href='#' 
                       style='background:#4CAF50; color:white; padding:10px 20px; text-decoration:none; border-radius:5px;'>
                        View Product
                    </a>
                </div>
            </div>

            <div style='background:#f1f1f1; text-align:center; padding:10px; font-size:12px; color:#777;'>
                © 2026 E-Commerce Platform
            </div>
        </div>
    </div>"
        };

        // ✅ SEND EMAIL
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync("smtp.gmail.com", 587, false);

        // ⚠️ USE APP PASSWORD (NOT NORMAL PASSWORD)
        await smtp.AuthenticateAsync("zorogoat05@gmail.com", "ubco msai cvde ayco");

        await smtp.SendAsync(mimeMessage);
        await smtp.DisconnectAsync(true);

        Console.WriteLine("✅ REAL Email sent successfully!\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
};

channel.BasicConsume(queue: "product_published", autoAck: true, consumer: consumer);

Console.WriteLine("🚀 Listening for events...");
Console.ReadLine();