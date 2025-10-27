using Microsoft.AspNetCore.Identity;
using Azure.Data.Tables;
using Azure;

namespace Po.LearnCert.Api.Features.Authentication.Infrastructure;

/// <summary>
/// Custom UserStore implementation for ASP.NET Core Identity using Azure Table Storage.
/// </summary>
public class TableUserStore : IUserStore<UserEntity>, IUserPasswordStore<UserEntity>, 
    IUserEmailStore<UserEntity>, IUserLockoutStore<UserEntity>, IUserSecurityStampStore<UserEntity>
{
    private readonly TableClient _tableClient;
    private readonly ILogger<TableUserStore> _logger;
    private const string PartitionKeyValue = "USER";

    public TableUserStore(TableServiceClient tableServiceClient, ILogger<TableUserStore> logger)
    {
        _logger = logger;
        _tableClient = tableServiceClient.GetTableClient("PoLearnCertUsers");
        _tableClient.CreateIfNotExists();
    }

    public async Task<IdentityResult> CreateAsync(UserEntity user, CancellationToken cancellationToken)
    {
        try
        {
            user.PartitionKey = PartitionKeyValue;
            user.RowKey = user.NormalizedUserName;
            user.Id = user.RowKey;
            user.ConcurrencyStamp = Guid.NewGuid().ToString();

            await _tableClient.AddEntityAsync(user, cancellationToken);
            _logger.LogInformation("Created user: {UserName}", user.UserName);
            return IdentityResult.Success;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            _logger.LogWarning("User already exists: {UserName}", user.UserName);
            return IdentityResult.Failed(new IdentityError { Description = "User already exists" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {UserName}", user.UserName);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
    }

    public async Task<IdentityResult> UpdateAsync(UserEntity user, CancellationToken cancellationToken)
    {
        try
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            await _tableClient.UpdateEntityAsync(user, user.ETag, TableUpdateMode.Replace, cancellationToken);
            _logger.LogInformation("Updated user: {UserName}", user.UserName);
            return IdentityResult.Success;
        }
        catch (RequestFailedException ex) when (ex.Status == 412)
        {
            _logger.LogWarning("Concurrency conflict updating user: {UserName}", user.UserName);
            return IdentityResult.Failed(new IdentityError { Description = "Concurrency conflict" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserName}", user.UserName);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
    }

    public async Task<IdentityResult> DeleteAsync(UserEntity user, CancellationToken cancellationToken)
    {
        try
        {
            await _tableClient.DeleteEntityAsync(user.PartitionKey, user.RowKey, cancellationToken: cancellationToken);
            _logger.LogInformation("Deleted user: {UserName}", user.UserName);
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserName}", user.UserName);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
    }

    public async Task<UserEntity?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<UserEntity>(PartitionKeyValue, userId, cancellationToken: cancellationToken);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<UserEntity?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await FindByIdAsync(normalizedUserName, cancellationToken);
    }

    public async Task<UserEntity?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        try
        {
            var filter = $"NormalizedEmail eq '{normalizedEmail}'";
            await foreach (var user in _tableClient.QueryAsync<UserEntity>(filter, cancellationToken: cancellationToken))
            {
                return user;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding user by email: {Email}", normalizedEmail);
            return null;
        }
    }

    public Task<string> GetUserIdAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.UserName);
    }

    public Task SetUserNameAsync(UserEntity user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(UserEntity user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName ?? string.Empty;
        return Task.CompletedTask;
    }

    // IUserPasswordStore implementation
    public Task SetPasswordHashAsync(UserEntity user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    // IUserEmailStore implementation
    public Task SetEmailAsync(UserEntity user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(UserEntity user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedEmailAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(UserEntity user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    // IUserLockoutStore implementation
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.LockoutEnd);
    }

    public Task SetLockoutEndDateAsync(UserEntity user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(UserEntity user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(UserEntity user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.LockoutEnabled);
    }

    public Task SetLockoutEnabledAsync(UserEntity user, bool enabled, CancellationToken cancellationToken)
    {
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    // IUserSecurityStampStore implementation
    public Task SetSecurityStampAsync(UserEntity user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(UserEntity user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}
