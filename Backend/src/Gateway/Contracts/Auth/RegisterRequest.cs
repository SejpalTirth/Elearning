namespace Gateway.Contracts.Auth
{
    public record RegisterRequest
    {
        public string Email { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}
