using ElevenNote.Models.Token;
using ElevenNote.Models.Tokens;

namespace ElevenNote.Services.Token;



    public interface ITokenService
    {
        Task<TokenResponse?> GetTokenAsync(TokenRequest model);
    }