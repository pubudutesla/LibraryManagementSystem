using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Api.Controllers
{
    [Route("api/logs")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly string _logDirectory = "Logs"; // Logs directory path
        [AllowAnonymous]
        [HttpGet]
        //[Authorize(Roles = "Admin")] // Restrict access to Admins only
        public async Task<IActionResult> GetLogs()
        {
            try
            {
                string latestLogFile = GetLatestLogFile();
                if (string.IsNullOrEmpty(latestLogFile))
                {
                    return NotFound(new { message = "No log files found" });
                }

                // Read all lines from log file (each line is a separate JSON object)
                var logLines = await System.IO.File.ReadAllLinesAsync(latestLogFile);

                if (logLines.Length == 0)
                {
                    return NotFound(new { message = "Log file is empty" });
                }

                // Convert multiple JSON lines into a valid JSON array
                var jsonArray = "[" + string.Join(",", logLines) + "]";

                // Parse to ensure it's valid JSON
                var parsedLogs = JsonSerializer.Deserialize<List<object>>(jsonArray, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                });

                return Ok(parsedLogs);
            }
            catch (JsonException)
            {
                return BadRequest(new { message = "Invalid JSON format in log file" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while reading logs", error = ex.Message });
            }
        }

        private string GetLatestLogFile()
        {
            if (!Directory.Exists(_logDirectory))
            {
                return null;
            }

            var logFiles = Directory.GetFiles(_logDirectory, "log-*.json")
                                    .OrderByDescending(f => f)
                                    .ToList();

            return logFiles.FirstOrDefault();
        }
    }
}