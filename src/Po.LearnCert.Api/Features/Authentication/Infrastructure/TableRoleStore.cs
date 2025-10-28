using Microsoft.AspNetCore.Identity;
using Azure.Data.Tables;
using Azure;
using Po.LearnCert.Api.Infrastructure.Entities;

namespace Po.LearnCert.Api.Features.Authentication.Infrastructure;

/// <summary>
/// Role entity for Azure Table Storage.
/// </summary>
public class RoleEntity : TableEntityBase
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? NormalizedName { get; set; }
    public string? ConcurrencyStamp { get; set; }
}

/// <summary>
/// Custom RoleStore implementation for ASP.NET Core Identity using Azure Table Storage.
/// </summary>
public class TableRoleStore : IRoleStore<RoleEntity>
{
    private readonly TableClient _tableClient;
    private readonly ILogger<TableRoleStore> _logger;
    private const string PartitionKeyValue = "ROLE";

    public TableRoleStore(TableServiceClient tableServiceClient, ILogger<TableRoleStore> logger)
    {
        _logger = logger;
        _tableClient = tableServiceClient.GetTableClient("PoLearnCertRoles");
        _tableClient.CreateIfNotExists();
    }

    public async Task<IdentityResult> CreateAsync(RoleEntity role, CancellationToken cancellationToken)
    {
        try
        {
            role.PartitionKey = PartitionKeyValue;
            role.RowKey = role.NormalizedName ?? role.Name.ToUpperInvariant();
            role.Id = role.RowKey;
            role.ConcurrencyStamp = Guid.NewGuid().ToString();

            await _tableClient.AddEntityAsync(role, cancellationToken);
            _logger.LogInformation("Created role: {RoleName}", role.Name);
            return IdentityResult.Success;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            _logger.LogWarning("Role already exists: {RoleName}", role.Name);
            return IdentityResult.Failed(new IdentityError { Description = "Role already exists" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", role.Name);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
    }

    public async Task<IdentityResult> UpdateAsync(RoleEntity role, CancellationToken cancellationToken)
    {
        try
        {
            role.ConcurrencyStamp = Guid.NewGuid().ToString();
            await _tableClient.UpdateEntityAsync(role, role.ETag, TableUpdateMode.Replace, cancellationToken);
            _logger.LogInformation("Updated role: {RoleName}", role.Name);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleName}", role.Name);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
    }

    public async Task<IdentityResult> DeleteAsync(RoleEntity role, CancellationToken cancellationToken)
    {
        try
        {
            await _tableClient.DeleteEntityAsync(role.PartitionKey, role.RowKey, cancellationToken: cancellationToken);
            _logger.LogInformation("Deleted role: {RoleName}", role.Name);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleName}", role.Name);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
    }

    public async Task<RoleEntity?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<RoleEntity>(PartitionKeyValue, roleId, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<RoleEntity?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return await FindByIdAsync(normalizedRoleName, cancellationToken);
    }

    public Task<string> GetRoleIdAsync(RoleEntity role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id);
    }

    public Task<string?> GetRoleNameAsync(RoleEntity role, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(role.Name);
    }

    public Task SetRoleNameAsync(RoleEntity role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(RoleEntity role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(RoleEntity role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}
