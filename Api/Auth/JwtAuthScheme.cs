﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Api.Auth;

public class JwtAuthScheme
{
    public const string Header = "Authorization";
    public const string SchemeType = "Bearer";

    public const string GuidClaim = "Guid";

    public class Options : AuthenticationSchemeOptions { }

    public class Handler : AuthenticationHandler<Options>
    {
        private JwtService jwtService;

        public Handler(JwtService jwtService,
                       IOptionsMonitor<Options> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
            this.jwtService = jwtService;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => Task.FromResult(HandleAuthenticate());

        private AuthenticateResult HandleAuthenticate()
        {
            string? jwt = Request.Headers[Header];
            if (string.IsNullOrEmpty(jwt))
                return AuthenticateResult.Fail("Missing 'Authenticate' header");
            
            jwt = jwtService.StripAuthSchemeName(jwt, SchemeType);
            Guid? guid = jwtService.Decode(jwt);
            if (!guid.HasValue)
                return AuthenticateResult.Fail("Invalid JWT");

            Claim[] claims = [
                new Claim(GuidClaim, guid.Value.ToString()),
            ];
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            var result = AuthenticateResult.Success(ticket);
            return result;
        }
    }
}
