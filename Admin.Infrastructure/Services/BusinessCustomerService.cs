using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessCustomerService : IBusinessCustomerService
{
    private readonly AdminDbContext _db;

    public BusinessCustomerService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CustomerSummaryDto>> ListAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.Customers
            .AsNoTracking()
            .Where(c => c.BusinessRegistrationId == businessId)
            .OrderBy(c => c.FullName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(c => new CustomerSummaryDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                CreatedAt = c.CreatedAt,
            })
            .ToList();
    }

    public async Task<CustomerDetailDto?> GetAsync(Guid businessId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var c = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == customerId && x.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return c is null ? null : MapDetail(c);
    }

    public async Task<CustomerDetailDto?> CreateAsync(Guid businessId, CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (!Validate(request.FullName, request.Email, request.Phone, request.Notes, out var name, out var email, out var phone, out var notes))
        {
            return null;
        }

        if (await EmailTakenAsync(businessId, email, excludeId: null, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            FullName = name,
            Email = email,
            Phone = phone,
            Notes = notes,
            CreatedAt = now,
        };

        _db.Customers.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetAsync(businessId, entity.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CustomerDetailDto?> UpdateAsync(
        Guid businessId,
        Guid customerId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Validate(request.FullName, request.Email, request.Phone, request.Notes, out var name, out var email, out var phone, out var notes))
        {
            return null;
        }

        var entity = await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && c.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        if (await EmailTakenAsync(businessId, email, excludeId: customerId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        entity.FullName = name;
        entity.Email = email;
        entity.Phone = phone;
        entity.Notes = notes;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await GetAsync(businessId, customerId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(Guid businessId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && c.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        _db.Customers.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async Task<bool> EmailTakenAsync(
        Guid businessId,
        string email,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var q = _db.Customers.Where(c => c.BusinessRegistrationId == businessId && c.Email == email);
        if (excludeId.HasValue)
        {
            q = q.Where(c => c.Id != excludeId.Value);
        }

        return await q.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool Validate(
        string? fullName,
        string? email,
        string? phone,
        string? notes,
        out string nameOut,
        out string emailOut,
        out string? phoneOut,
        out string? notesOut)
    {
        nameOut = string.Empty;
        emailOut = string.Empty;
        phoneOut = null;
        notesOut = null;

        var n = (fullName ?? string.Empty).Trim();
        if (n.Length < 2 || n.Length > 200)
        {
            return false;
        }

        var e = (email ?? string.Empty).Trim();
        if (e.Length < 3 || e.Length > 320 || e.IndexOf('@', StringComparison.Ordinal) < 0)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            var p = phone.Trim();
            if (p.Length > 40)
            {
                return false;
            }

            phoneOut = p;
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            var nt = notes.Trim();
            notesOut = nt.Length > 4000 ? nt[..4000] : nt;
        }

        nameOut = n;
        emailOut = e;
        return true;
    }

    private static CustomerDetailDto MapDetail(Customer c) =>
        new()
        {
            Id = c.Id,
            FullName = c.FullName,
            Email = c.Email,
            Phone = c.Phone,
            Notes = c.Notes,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
        };
}
