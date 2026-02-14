using ErkanTatilPlani.Core.Entities;

namespace ErkanTatilPlani.Core.Infrastructure;

public interface IJwtService
{
    string GenerateToken(Visitor visitor);
}
