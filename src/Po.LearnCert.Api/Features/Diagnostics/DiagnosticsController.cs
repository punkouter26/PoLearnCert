using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;

namespace Po.LearnCert.Api.Features.Diagnostics;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(TableServiceClient tableServiceClient, ILogger<DiagnosticsController> logger)
    {
        _tableServiceClient = tableServiceClient;
        _logger = logger;
    }

    [HttpGet("tables")]
    public async Task<ActionResult> ListTables()
    {
        try
        {
            var tables = new List<string>();
            await foreach (var table in _tableServiceClient.QueryAsync())
            {
                tables.Add(table.Name);
            }
            return Ok(new { Tables = tables, Count = tables.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list tables");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpGet("tables/{tableName}/count")]
    public async Task<ActionResult> CountEntities(string tableName)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            var count = 0;
            await foreach (var entity in tableClient.QueryAsync<TableEntity>())
            {
                count++;
            }
            return Ok(new { TableName = tableName, Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count entities in table {TableName}", tableName);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpGet("tables/{tableName}/entities")]
    public async Task<ActionResult> ListEntities(string tableName, [FromQuery] int limit = 100)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            var entities = new List<Dictionary<string, object?>>();
            var count = 0;
            
            await foreach (var entity in tableClient.QueryAsync<TableEntity>())
            {
                if (count >= limit) break;
                
                var dict = new Dictionary<string, object?>();
                foreach (var prop in entity)
                {
                    dict[prop.Key] = prop.Value;
                }
                entities.Add(dict);
                count++;
            }
            
            return Ok(new { TableName = tableName, Entities = entities, Count = entities.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list entities in table {TableName}", tableName);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpGet("sessions/user/{userId}")]
    public async Task<ActionResult> GetUserSessions(string userId)
    {
        try
        {
            var tableClient = _tableServiceClient.GetTableClient("quizsessions");
            var sessions = new List<Dictionary<string, object?>>();
            var filter = $"PartitionKey eq '{userId}'";
            
            await foreach (var entity in tableClient.QueryAsync<TableEntity>(filter))
            {
                var dict = new Dictionary<string, object?>();
                foreach (var prop in entity)
                {
                    dict[prop.Key] = prop.Value;
                }
                sessions.Add(dict);
            }
            
            return Ok(new { UserId = userId, Sessions = sessions, Count = sessions.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sessions for user {UserId}", userId);
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}
