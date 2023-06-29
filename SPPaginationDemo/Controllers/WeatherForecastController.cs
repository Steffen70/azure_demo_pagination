using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SPPaginationDemo.Dto;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.Filtration.Custom;

namespace SPPaginationDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public WeatherForecastController(DataContext dataContext, IMapper mapper)
    {
        _dataContext = dataContext;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeatherForecastDto>>> Get([FromBody] FiltrationParams filtrationParams)
    {
        var userAdminDtos = _dataContext.WeatherForecasts
            .OrderBy(w => w.Date)
            .ProjectTo<WeatherForecastDto>(_mapper.ConfigurationProvider);

        var weatherList = await FilteredList<WeatherForecastDto>.CreateAsync(userAdminDtos, filtrationParams, _mapper);

        Response.AddFiltrationHeader(weatherList);

        return weatherList.Result;
    }

    [HttpGet("test-filter")]
    public async Task<ActionResult<IEnumerable<WeatherForecastDto>>> GetTestFilter([FromBody] DayFilterParams filtrationParams)
    {
        // Raw SQL Query with parameterized day filter
        var rawQuery = _dataContext.WeatherForecasts
            .FromSqlRaw($"SELECT * FROM WeatherForecasts WHERE strftime('%Y-%m-%d', Date) = '{filtrationParams.Date:yyyy-MM-dd}'")
            .AsQueryable();

        var orderedQuery = rawQuery.OrderBy(w => w.Date);

        // Project to DTO
        var projectedQuery = orderedQuery.ProjectTo<WeatherForecastDto>(_mapper.ConfigurationProvider);

        var weatherList = await FilteredList<WeatherForecastDto, DayFilterHeader>.CreateAsync(projectedQuery, filtrationParams, _mapper);

        // TODO: Add additional fields that are not contained in the ef model

        Response.AddFiltrationHeader(weatherList);

        return weatherList.Result;
    }

    [HttpGet("sqlite")]
    public async Task<ActionResult<string>> GetFromSqlite([FromBody] DayFilterParams filtrationParams)
    {
        var weatherList = await FilteredListSqlite<DayFilterHeader>.CreateAsync($"select * from WeatherForecasts WHERE strftime('%Y-%m-%d', Date) = '{filtrationParams.Date:yyyy-MM-dd}'", filtrationParams, _mapper);

        Response.AddFiltrationHeader(weatherList.Header);

        // Serialize the object to JSON using camelCase
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var jsonResult = JsonConvert.SerializeObject(weatherList.Result, jsonSerializerSettings);

        return Content(jsonResult, "application/json");
    }
}