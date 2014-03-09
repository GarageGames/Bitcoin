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
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
