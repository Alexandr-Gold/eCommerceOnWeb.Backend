// Application/Common/Interfaces/IDbConstraintViolationDetector.cs
using Microsoft.EntityFrameworkCore;

namespace eCommerceOnWeb.Backend.Application.Common.Interfaces;

public enum ConstraintKind
{
    Unique,
    ForeignKey
}

public readonly record struct ConstraintViolation(
    ConstraintKind Kind,
    string ConstraintName);

/// <summary>
/// Абстракция над провайдер-специфичным разбором DbUpdateException.
/// Application не зависит от Npgsql / SqlClient напрямую.
/// </summary>
public interface IDbConstraintViolationDetector
{
    bool TryDetect(DbUpdateException ex, out ConstraintViolation violation);
}