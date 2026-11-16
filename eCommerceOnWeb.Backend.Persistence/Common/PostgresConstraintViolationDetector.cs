// Persistence/Common/PostgresConstraintViolationDetector.cs
using eCommerceOnWeb.Backend.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace eCommerceOnWeb.Backend.Persistence.Common;

internal sealed class PostgresConstraintViolationDetector
    : IDbConstraintViolationDetector
{
    public bool TryDetect(DbUpdateException ex, out ConstraintViolation violation)
    {
        violation = default;

        PostgresException? pg = ex.GetBaseException() as PostgresException
            ?? ex.InnerException as PostgresException;

        if (pg is null || pg.ConstraintName is null)
        {
            return false;
        }

        ConstraintKind kind = pg.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation => ConstraintKind.Unique,
            PostgresErrorCodes.ForeignKeyViolation => ConstraintKind.ForeignKey,
            _ => default
        };

        if (pg.SqlState is not PostgresErrorCodes.UniqueViolation
            and not PostgresErrorCodes.ForeignKeyViolation)
        {
            return false;
        }

        violation = new ConstraintViolation(kind, pg.ConstraintName);
        return true;
    }
}