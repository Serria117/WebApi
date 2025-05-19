using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Payloads;
using WebApp.Services.RegionService;
using WebApp.Services.RegionService.Dto;

namespace WebApp.Controllers;

[ApiController] [Route("/api/region")] [Authorize]
public class RegionController(IRegionAppService regionService) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated and sorted list of provinces based on the specified request parameters.
    /// </summary>
    /// <param name="req">The request parameters containing pagination, sorting, and filtering options.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> containing the list of provinces.</returns>
    [HttpGet("provinces")]
    public async Task<IActionResult> GetProvince([FromQuery] RequestParam req)
    {
        var page = PageRequest.BuildRequest(req.Valid());
        var result = await regionService.GetAllProvincesAsync(page);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the details of a specific province by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the province to retrieve.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> that contains the province details if found, or an error response if not.</returns>
    [HttpGet("provinces/{id:int}")]
    public async Task<IActionResult> GetProvinceById(int id)
    {
        var result = await regionService.GetProvinceAsync(id);
        return result.Code switch
        {
            "200" => Ok(result),
            "404" => NotFound(result),
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Creates a new province using the provided details.
    /// </summary>
    /// <param name="input">The data transfer object containing the information about the province to be created.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpPost("provinces/create")]
    public async Task<IActionResult> CreateProvince(ProvinceCreateDto input)
    {
        var result = await regionService.CreateProvinceAsync(input);
        return result.Code switch
        {
            "200" => CreatedAtAction(nameof(CreateProvince), result),
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Creates multiple provinces based on the provided list of province data.
    /// </summary>
    /// <param name="input">The list of province data objects to be created.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> indicating the success or failure of the creation process.</returns>
    [HttpPost("provinces/create-many")]
    public async Task<IActionResult> CreateManyProvinces(List<ProvinceCreateDto> input)
    {
        var result = await regionService.CreateManyProvincesAsync(input);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Updates an existing province based on the specified ID and input data.
    /// </summary>
    /// <param name="id">The ID of the province to be updated.</param>
    /// <param name="input">The input data containing updated province information.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    [HttpPut("provinces/update/{id:int}")]
    public async Task<IActionResult> UpdateProvince(int id, [FromBody] ProvinceCreateDto input)
    {
        return Ok(await regionService.UpdateProvinceAsync(id, input));
    }

    /// <summary>
    /// Creates a new district based on the provided data.
    /// </summary>
    /// <param name="input">The data used to create the district, including name, alternate name, code, and associated province ID.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> containing the result of the district creation.</returns>
    [HttpPost("district")]
    public async Task<IActionResult> CreateDistrict(DistrictCreateDto input)
    {
        return Ok(await regionService.CreateDistrictAsync(input));
    }

    /// <summary>
    /// Creates multiple districts for a specified province based on the provided district details.
    /// </summary>
    /// <param name="pId">The ID of the province to associate the districts with.</param>
    /// <param name="input">A list of district creation details.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> containing the result of the creation operation.</returns>
    [HttpPost("districts/{pId:int}")]
    public async Task<IActionResult> CreateManyDistricts(int pId, List<DistrictCreateDto> input)
    {
        var result = await regionService.CreateManyDistrictsAsync(pId, input);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Creates a new tax office based on the specified input data.
    /// </summary>
    /// <param name="input">The data used to create the tax office, including details such as full name, short name, code, and province or parent ID.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> indicating the result of the creation process.</returns>
    [HttpPost("taxOffices/create")]
    public async Task<IActionResult> CreateTaxOffice(TaxOfficeCreateDto input)
    {
        var result = await regionService.CreateTaxOfficeAsync(input);
        return result.Code switch
        {
            "200" => Ok(result),
            "400" => BadRequest(result),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result),
        };
    }

    /// <summary>
    /// Creates multiple tax offices under the specified province.
    /// </summary>
    /// <param name="pId">The ID of the province under which the tax offices will be created.</param>
    /// <param name="input">The list of <see cref="TaxOfficeCreateDto"/> containing details of the tax offices to be created.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> indicating the success or failure of the operation.</returns>
    [HttpPost("taxOffices/create-many/{pId:int}")]
    public async Task<IActionResult> CreateManyTaxOffices(int pId, List<TaxOfficeCreateDto> input)
    {
        var result = await regionService.CreateManyTaxOfficeAsync(pId, input);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Retrieves a paginated list of all tax offices based on the provided request parameters.
    /// </summary>
    /// <param name="req">The request parameters including pagination, sorting, and filtering options.</param>
    /// <param name="parentOnly">A boolean flag indicating whether to include only parent tax offices (true) or both parent and child tax offices (false).</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> containing the paginated list of tax offices.</returns>
    [HttpGet("taxOffices")]
    public async Task<IActionResult> GetAllTaxOffice([FromQuery] RequestParam req,
                                                     [FromQuery] bool parentOnly = false)
    {
        var page = PageRequest.BuildRequest(req.Valid());
        var result = parentOnly
            ? await regionService.FindTopLevelTaxOfficesAsync()
            : await regionService.GetAllTaxOfficesAsync(page);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a tax office by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tax office to retrieve.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> containing the tax office details if found.</returns>
    [HttpGet("taxOffices/{id:int}")]
    public async Task<IActionResult> GetTaxOfficeById(int id)
    {
        var result = await regionService.FindTaxOfficeByIdAsync(id);
        return result.Code switch
        {
            "200" => Ok(result),
            "404" => NotFound(result),
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Updates a tax office with the specified details.
    /// </summary>
    /// <param name="id">The unique identifier of the tax office to update.</param>
    /// <param name="input">The details of the tax office to update, provided as a <see cref="TaxOfficeCreateDto"/> object.</param>
    /// <returns>A task representing the asynchronous operation, with an <see cref="IActionResult"/> containing the operation result.</returns>
    [HttpPut("taxOffices/update/{id:int}")]
    public async Task<IActionResult> UpdateTaxOffice([FromRoute] int id,
                                                     [FromBody] TaxOfficeCreateDto input)
    {
        var result = await regionService.UpdateTaxOfficeAsync(id, input);
        return result.Code switch
        {
            "200" => Ok(result),
            "404" => NotFound(result),
            "500" => StatusCode(StatusCodes.Status500InternalServerError, result),
            _ => BadRequest(result)
        };
    }

    [HttpGet("taxOffices/code-exists")]
    public async Task<IActionResult> TaxOfficeCodeExists([FromQuery(Name = "code")] string code)
    {
        var result = await regionService.TaxOfficeCodeExists(code);
        return Ok(result);
    }
}