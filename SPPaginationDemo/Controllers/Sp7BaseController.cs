using AutoMapper;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPPaginationDemo.Filtration;
using System.Reflection;
using SPPaginationDemo.DtoGenerator;
using SPPaginationDemo.SqlQueries;
using SPPaginationDemo.Services;

#pragma warning disable CS1998

namespace SPPaginationDemo.Controllers;

[ApiController]
[Route("[controller]")]
public abstract class Sp7ControllerBase : Controller
{
    private readonly IMapper _mapper;
    private readonly Appsettings _appsettings;
    private readonly ILogger _logger;

    protected Sp7ControllerBase(IMapper mapper, Appsettings appsettings, ILogger logger)
    {
        _mapper = mapper;
        _appsettings = appsettings;
        _logger = logger;
    }

    [HttpGet("sql-query-identifier/{*actionName}")]
    public async Task<ActionResult<string>> GetSqlQueryIdentifierAsync(string actionName)
    {
        var generatorWrapper = new SqlGeneratorFactory(_logger, _appsettings, actionName, typeof(IDemoSelect));

        var dtoWrapper = new RedisCacheFactory(_logger, _appsettings, generatorWrapper.SqlIdentifier);

        if (!dtoWrapper.IsCached)
            dtoWrapper = new RedisCacheFactory(_logger, _appsettings, generatorWrapper);

        return Ok(dtoWrapper.SqlIdentifier);
    }

    [HttpGet("assembly-bytes/{*sqlIdentifier}")]
    public ActionResult<string> GetAssemblyBytes(string sqlIdentifier)
    {
        var dtoCache = new RedisCacheFactory(_logger,_appsettings, sqlIdentifier);

        if (!dtoCache.IsCached)
            throw new InvalidOperationException("No type with the specified identifier was found in the cache.");

        return Ok(dtoCache.AssemblyString);
    }

    [Route("{*actionName}")]
    public async Task<IActionResult> HandleRequestAsync(string actionName)
    {
        var enpointType = GetType().GetNestedType(actionName, BindingFlags.Public);
        if (enpointType == null)
            return BadRequest();

        var endpoint = (IEndpoint)Activator.CreateInstance(enpointType, null)!;

        endpoint.ControllerBase = this;

        var endpointType = endpoint.GetType();

        var queryFilter = endpointType.GetMethod(nameof(IEndpoint<object, FiltrationParams>.QueryFilter))!;

        var filterationParameterType = queryFilter.GetParameters()
            .First(p => p.ParameterType.BaseType == typeof(FiltrationParams)).ParameterType;

        var getFiltrationParamsMethod =
            typeof(HttpExtensions).GetMethod(nameof(HttpExtensions.GetFiltrationParamsAsync))!.MakeGenericMethod(
                filterationParameterType);

        dynamic filterationParamsTask = getFiltrationParamsMethod.Invoke(null, new object[] { HttpContext })!;

        var filtrationParams = await filterationParamsTask;
        if (filtrationParams == null) return BadRequest();

        var generatorWrapper = new SqlGeneratorFactory(_logger,_appsettings, actionName, typeof(IDemoSelect));

        var dtoWrapper = new RedisCacheFactory(_logger,_appsettings, generatorWrapper.SqlIdentifier);

        if (!dtoWrapper.IsCached)
            dtoWrapper = new RedisCacheFactory(_logger,_appsettings, generatorWrapper);

        var generatedSp7Context = typeof(Sp7Context<>).MakeGenericType(dtoWrapper.Model);

        dynamic sp6Context = Activator.CreateInstance(generatedSp7Context, new DbContextOptionsBuilder().UseSqlServer(_appsettings.SqlConnectionString).Options)!;

        var fromSqlRawMethod = typeof(RelationalQueryableExtensions).GetMethod(nameof(RelationalQueryableExtensions.FromSqlRaw))!;

        var genericFromSqlRawMethod = fromSqlRawMethod.MakeGenericMethod(dtoWrapper.Model);

        dynamic selection = genericFromSqlRawMethod.Invoke(null, new object[] { sp6Context.GeneratedModel, generatorWrapper.SqlQuery, Array.Empty<object>() })!;

        var genericFiltration = queryFilter.MakeGenericMethod(dtoWrapper.Model);

        var genericFiltrationTask = new Task<dynamic>(() =>
            genericFiltration.Invoke(endpoint, new object[] { selection, filtrationParams })!);

        genericFiltrationTask.Start();

        var result = await genericFiltrationTask;

        var headerType = _mapper.ConfigurationProvider.Internal().GetAllTypeMaps()
            .First(t => t.SourceType == filterationParameterType).DestinationType;

        var addResultHeaderMethod = GetType().GetMethod(nameof(AddResultHeader))!;

        var genericAddResultMethod = addResultHeaderMethod.MakeGenericMethod(dtoWrapper.Model, filterationParameterType, headerType);

        dynamic addResultHeaderTask = genericAddResultMethod.Invoke(this, new object[] { result, filtrationParams })!;

        var modifiedModels = await addResultHeaderTask;

        var endpointMethod = endpointType.GetMethod(nameof(IEndpoint<object, FiltrationParams>.InMemoryProcessingAsync))!;

        var genericEndpointMethod = endpointMethod.MakeGenericMethod(dtoWrapper.Model);

        dynamic genericEndpointTask = genericEndpointMethod.Invoke(endpoint, new object[] { modifiedModels.Result, filtrationParams })!;

        return await genericEndpointTask;
    }

    public async Task<FilteredList<TQueryable, THeader>> AddResultHeader<TQueryable, TParams, THeader>(IQueryable<TQueryable> queryable, TParams @params) where THeader : FiltrationHeader where TParams : FiltrationParams
    {
        var modifiedModels = await FilteredList<TQueryable, THeader>.CreateAsync(queryable, @params, _mapper);

        Response.AddFiltrationHeader(modifiedModels.Header);

        return modifiedModels;
    }

    private interface IEndpoint
    {
        Sp7ControllerBase ControllerBase { get; set; }
    }

    private interface IEndpoint<in TInterface, in TParams> : IEndpoint where TParams : FiltrationParams
    {
        public IQueryable<TGenerated> QueryFilter<TGenerated>(IQueryable<TGenerated> queryable,
            TParams filtrationParams) where TGenerated : class, TInterface;

        public Task<IActionResult> InMemoryProcessingAsync<TGenerated>(List<TGenerated> paginatedResult,
            TParams filtrationParams) where TGenerated : class, TInterface;
    }

    /// <summary>
    /// Represents an abstract endpoint that defines methods for filtering and in-memory processing.
    /// This class must be inherited and its methods implemented in derived classes.
    /// Each endpoint is associated with a specific controller.
    /// </summary>
    /// <typeparam name="TInterface">Defines the interface that sets the required fields returned by the SQL query.</typeparam>
    /// <typeparam name="TParams">Represents filtration parameters. Remember to create an AutoMapper profile to map TParams to your header.</typeparam>
    public abstract class Endpoint<TInterface, TParams> : IEndpoint<TInterface, TParams> where TParams : FiltrationParams
    {
        public Sp7ControllerBase ControllerBase { get; set; } = null!;

        public abstract IQueryable<TGenerated> QueryFilter<TGenerated>(IQueryable<TGenerated> queryable, TParams filtrationParams) where TGenerated : class, TInterface;

        public abstract Task<IActionResult> InMemoryProcessingAsync<TGenerated>(List<TGenerated> paginatedResult, TParams filtrationParams) where TGenerated : class, TInterface;
    }
}