/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 03/09/2011
 * Время: 13:42
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;

using NLog;

namespace SA1CService
{
	/// <summary>
	/// Description of Mail.
	/// </summary>
	public static class Mail
	{
		
		
		private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
		{
			// Get the unique identifier for this asynchronous operation.
			//String token = (string) e.UserState;

			if (e.Cancelled)
			{
				//Console.WriteLine("[{0}] Send canceled.", token);
			}
			if (e.Error != null)
			{
				Logger logger = LogManager.GetCurrentClassLogger();
				logger.Error(e.Error.Message);
				if(e.Error.InnerException!=null){
					logger.Error(e.Error.InnerException.Message);
				}
			} else
			{
				//Console.WriteLine("Message sent.");
			}
		}
		
		public static void SendMail(EmailSetting emailSetting, string Subject, string body){
			MailMessage message;
			SmtpClient client;

			try
			{
				message = new System.Net.Mail.MailMessage(
					emailSetting.User,
					emailSetting.MailTo,
					Subject,
					body);

				message.From = new MailAddress(emailSetting.User,"From");
				client = new SmtpClient(emailSetting.SMTPServer, emailSetting.Port);
				client.Credentials = new NetworkCredential(emailSetting.User, emailSetting.GetPassword());
				client.EnableSsl = emailSetting.EnableSsl;
				client.DeliveryMethod = SmtpDeliveryMethod.Network;
				message.BodyEncoding = System.Text.Encoding.UTF8;
				message.IsBodyHtml = true;
				client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
				client.SendAsync(message,client);				
			}
			catch (Exception ex)
			{
				Logger logger = LogManager.GetCurrentClassLogger();
				logger.Error(ex.Message +". "+ex.InnerException.Message);
			}
		}
	}
}
