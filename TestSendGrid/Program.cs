using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;

namespace TestSendGrid
{
    class Program
    {
        // https://docs.microsoft.com/en-us/azure/sendgrid-dotnet-how-to-send-email
        static void Main(string[] args)
        {
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress("dx@example.com", "SendGrid DX Team"));

            var recipients = new List<EmailAddress>
            {
                new EmailAddress("jeff@example.com", "Jeff Smith"),
                new EmailAddress("anna@example.com", "Anna Lidman"),
                new EmailAddress("peter@example.com", "Peter Saddow")
            };
            msg.AddTos(recipients);

            msg.SetSubject("Testing the SendGrid C# Library");

            msg.AddContent(MimeType.Text, "Hello World plain text!");
            msg.AddContent(MimeType.Html, "<p>Hello World!</p>");

            var apiKey = System.Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
            var client = new SendGridClient(apiKey);
            // var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", htmlContent, false);
            var response = client.SendEmailAsync(msg).GetAwaiter().GetResult();
        }
    }
}
