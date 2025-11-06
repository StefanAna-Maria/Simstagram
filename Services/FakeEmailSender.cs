using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System;

namespace proiect.Services
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"Email simulativ către: {email} — Subiect: {subject}");
            return Task.CompletedTask;
        }
    }
}