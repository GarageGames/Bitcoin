using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace CentralMine.NET
{
    public class Email
    {

        public Email()
        {
        }

        public void SendEmail(string body)
        {
            // Create thread to send email
            Thread t = new Thread(new ParameterizedThreadStart(DoSendEmail));

            t.Start(body);
        }

        void DoSendEmail(object data)
        {
            var fromAddress = new MailAddress("rono@torquepowered.com", "From Name");
            var toAddress = new MailAddress("rono@torquepowered.com", "To Name");
            const string fromPassword = "mandolin442";
            const string subject = "BitcoinTest";
            string body = (string)data;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            MailMessage message = new MailMessage(fromAddress, toAddress);
            message.Subject = subject;
            message.Body = body;
            message.To.Add(new MailAddress("ericp@torquepowered.com"));
            message.To.Add(new MailAddress("justinh@torquepowered.com"));
            smtp.Send(message);
        }

        public static void SendErrorEmail(string body)
        {
            var fromAddress = new MailAddress("rono@torquepowered.com", "From Name");
            var toAddress = new MailAddress("rono@torquepowered.com", "To Name");
            const string fromPassword = "mandolin442";
            const string subject = "Mining Error";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            MailMessage message = new MailMessage(fromAddress, toAddress);
            message.Subject = subject;
            message.Body = body;
            //smtp.Send(message);
        }
    }
}
