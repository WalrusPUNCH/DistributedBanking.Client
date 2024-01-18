namespace DistributedBanking.Client.Domain.Options;

public record JwtOptions(string Issuer, string Audience, string Key);