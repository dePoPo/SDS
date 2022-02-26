//-----------------------------------------------------------------------
//  Path="C:\Users\bno.CORP\OneDrive\Git\SDS\sds\sds"
//  File="LogBuilder.cs" 
//  Modified="zaterdag 26 februari 2022" 
//  Author: H.P. Noordam
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace sds
{
    internal class LogBuilder
    {
        private readonly List<string> _messagesList;

        public LogBuilder() {
            _messagesList = new List<string>();
        }

        public void MailLog(string smtpserver, string rcpt, string sender) {
            MailMessage msg = new MailMessage {
                From = new MailAddress(sender)
            };
            msg.To.Add(new MailAddress(rcpt));
            msg.Subject = $"sds backup log {DateTime.Now}";
            foreach (string s in _messagesList) {
                msg.Body += $"{s}{Environment.NewLine}";
            }
            msg.IsBodyHtml = false;
            SmtpClient smtpClient = new SmtpClient(smtpserver);
            smtpClient.Send(msg);
        }

        public void Say(string msg) {
            msg = $"{DateTime.Now} : {msg}";
            _messagesList.Add(msg);
            Console.WriteLine(msg);
        }
    }
}
