

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Contracts;

namespace Shared.Contracts.Common
{
    public class EnviarEmail : IEnviarEmailAsync
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnviarEmail> _logger;
        public EnviarEmail(IConfiguration configuration, ILogger<EnviarEmail> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task EnviarEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
           try
            {
                var fromAddressValue = _configuration["Smtp:From"] ?? "";
                var fromDisplayName = _configuration["Smtp:FromDisplayName"] ?? "Gestão Hospitalar";
                var fromAddress = new MailAddress(fromAddressValue, fromDisplayName);
                var toAddress = new MailAddress(toEmail, "Destinatário");

                var host = _configuration["Smtp:Host"] ?? "mail.sosoft.it.ao";
                var portString = _configuration["Smtp:Port"] ?? "587";
                int.TryParse(portString, out var port);
                if (port == 0) port = 587;

                var enableSsl = true;
                if (bool.TryParse(_configuration["Smtp:EnableSsl"], out var parsedSsl)) enableSsl = parsedSsl;

                var username = _configuration["Smtp:Username"] ?? fromAddress.Address;
                var password = _configuration["Smtp:Password"] ?? string.Empty;

                _logger.LogDebug("SMTP config (sem senha): Host={Host}, Port={Port}, EnableSsl={EnableSsl}, Username={Username}", host, port, enableSsl, username);

                using var smtp = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password),
                    Timeout = 60000
                };

                smtp.TargetName = "STARTTLS/" + host;

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                var timeoutSeconds = 15;
                if (int.TryParse(_configuration["Smtp:SendTimeoutSeconds"], out var cfgVal) && cfgVal > 0)
                    timeoutSeconds = cfgVal;

                var sendTask = smtp.SendMailAsync(message);
                var completed = await Task.WhenAny(sendTask, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));
                if (completed != sendTask)
                {
                    _logger.LogError("Timeout ao enviar e-mail para {Email} após {Seconds} segundos", toEmail, timeoutSeconds);
                    throw new TimeoutException($"Timeout ao enviar e-mail após {timeoutSeconds} segundos.");
                }

                await sendTask;
                _logger.LogInformation("E-mail enviado com sucesso para {Email}", toEmail);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "Erro SMTP ao enviar e-mail para {Email}. Status={Status}", toEmail, smtpEx.StatusCode);
                if (smtpEx.InnerException != null)
                    _logger.LogError(smtpEx.InnerException, "Inner exception: {Msg}", smtpEx.InnerException.Message);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar e-mail para {Email}", toEmail);
                throw;
            }
        }   
             public void EnviarEmaill(string toEmail, string subject, string body)
        {
            EnviarEmailAsync(toEmail, subject, body, false).GetAwaiter().GetResult();
        }
    }
}