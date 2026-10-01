namespace MDRNJ.Shared.DTOs.Auth
{
    // Registration no longer logs the user in — they must verify their email first.
    public class RegisterResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
