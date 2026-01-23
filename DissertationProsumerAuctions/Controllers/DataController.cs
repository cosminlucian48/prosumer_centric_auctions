using Microsoft.AspNetCore.Mvc;
using DissertationProsumerAuctions.DatabaseConnections;

namespace DissertationProsumerAuctions.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly DatabaseConnection _database;
        
        public DataController()
        {
            _database = DatabaseConnection.Instance;
        }
        
        /// <summary>
        /// Get prosumer load data by ID and timestamp
        /// </summary>
        [HttpGet("prosumer/{id}/load")]
        public async Task<IActionResult> GetProsumerLoad(int id, [FromQuery] string timestamp)
        {
            try
            {
                var data = await _database.GetProsumerLoadByIdAsync(id, timestamp);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Get prosumer generation data by ID and timestamp
        /// </summary>
        [HttpGet("prosumer/{id}/generation")]
        public async Task<IActionResult> GetProsumerGeneration(int id, [FromQuery] string timestamp)
        {
            try
            {
                var data = await _database.GetProsumerGenerationByIdAsync(id, timestamp);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Get energy market prices by timestamp
        /// </summary>
        [HttpGet("energy-market/prices")]
        public async Task<IActionResult> GetEnergyMarketPrices([FromQuery] string timestamp)
        {
            try
            {
                var data = await _database.GetEnergyMarketPricesbyTime(timestamp);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
