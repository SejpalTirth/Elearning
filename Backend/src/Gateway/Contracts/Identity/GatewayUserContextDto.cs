namespace Gateway.Contracts.Identity
{
    public sealed class GatewayUserContextDto
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
        public Dictionary<string, string> Claims { get; set; } = new();
    }

}
