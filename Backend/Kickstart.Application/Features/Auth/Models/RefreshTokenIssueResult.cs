using Kickstart.Domain.Entities;

namespace Kickstart.Application.Features.Auth.Models
{
    /// <summary>
    /// Persisted refresh token row (stores hash only) plus the plaintext secret shown once to the client.
    /// </summary>
    public sealed class RefreshTokenIssueResult
    {
        public required RefreshToken Stored { get; init; }
        public required string PlainToken { get; init; }
    }
}
