namespace Engine.Models.Emails;

public class EmailSettings
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}
