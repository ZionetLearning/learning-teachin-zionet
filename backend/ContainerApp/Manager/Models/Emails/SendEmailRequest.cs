namespace Manager.Models.Emails;

public class SendEmailRequest
{
    public required string RecipientEmail { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}