using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Utility
{
	public class EmailSender : IEmailSender
	{
		private readonly IOptions<EmailSenderSettings> _emailSenderSettings;

		public EmailSender(IOptions<EmailSenderSettings> emailSenderSettings)
		{
			_emailSenderSettings = emailSenderSettings;
		}

		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			var emailToSend = new MimeMessage();
			emailToSend.From.Add(MailboxAddress.Parse(_emailSenderSettings.Value.EmailAddress));
			emailToSend.To.Add(MailboxAddress.Parse(email));
			emailToSend.Subject = subject;
			emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

			using (var emailClient = new SmtpClient())
			{
				emailClient.Connect(_emailSenderSettings.Value.SMTPServer, _emailSenderSettings.Value.SMTPPort, MailKit.Security.SecureSocketOptions.StartTls);
				emailClient.Authenticate(_emailSenderSettings.Value.EmailAddress, _emailSenderSettings.Value.EmailPassword);
				emailClient.Send(emailToSend);
				emailClient.Disconnect(true);
			}

			return Task.CompletedTask;
		}
	}
}
