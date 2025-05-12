using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using zad_LAB09.Models.DTOs;
using zad_LAB09.Services;

namespace zad_LAB09.Controllers;

[Route("api/[controller]")] 
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;  
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    
    public WarehouseController(IWarehouseService warehouseService, IProductService productService, IOrderService orderService)
    {
        _warehouseService = warehouseService;
        _productService = productService;
        _orderService = orderService;
    }
    
    
    // GET api/warehouse/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> DoesWarehouseExist(int id)
    {
        if (!await _warehouseService.DoesWarehouseExist(id))
        {
            return NotFound($"Warehouse with ID {id} does not exist.");
        }
            
        return Ok();
    }

    // POST api/warehouse
    [HttpPost]
    public async Task<IActionResult> AddProduct(AddProductToWarehouseRequest request)
    {
        try
        {
            var resultId = await _warehouseService.AddProductToWarehouse(request);
            return CreatedAtAction(nameof(AddProduct), new { resultId }, new { IdProductWarehouse = resultId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new { error = "Database error", details = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An internal error occurred: {ex.Message}");
        }
    }
    
    // POST api/warehouse/proc
    [HttpPost("proc")]
    public async Task<IActionResult> AddProductProcedure([FromBody] AddProductToWarehouseRequest request)
    {
        try
        {
            var resultId = await _warehouseService.AddProductToWarehouseProcedure(request);
            // return Ok(new { IdProductWarehouse = resultId });
            return CreatedAtAction(nameof(AddProductProcedure), new { resultId }, new { IdProductWarehouse = resultId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

}