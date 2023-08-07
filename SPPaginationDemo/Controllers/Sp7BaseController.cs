using AutoMapper;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.ModelGenerator;
using System.Reflection;
#pragma warning disable CS1998

namespace SPPaginationDemo.Controllers;

[ApiController]
[Route("[controller]")]
public abstract class Sp7ControllerBase : Controller
{
    private readonly IMapper _mapper;
    private static readonly Dictionary<string, DynamicTypeGenerator> DynamicTypeGenerators = new();

    private const string ConnectionString = @"Server=SPAG-DS\SQL2019;Database=SP6;Integrated Security=true;Connection Timeout=60;TrustServerCertificate=true";

    protected Sp7ControllerBase(IMapper mapper)
    {
        _mapper = mapper;
    }

    private IEndpoint? GetEndpoint(string actionName)
    {
        var enpointType = GetType().GetNestedType(actionName, BindingFlags.Public);
        if (enpointType == null)
            return null;

        return (IEndpoint)Activator.CreateInstance(enpointType, null)!;
    }

    [HttpGet("sql-query-identifier/{*actionName}")]
    public async Task<ActionResult<string>> GetSqlQueryIdentifierAsync(string actionName)
    {
        var endpoint = GetEndpoint(actionName);

        if (endpoint == null)
            return BadRequest();

        var sqlQuery = await endpoint.SqlQuery;

        var sqlIdentifier = DynamicTypeGenerator.GetIdentifier(sqlQuery);

        if (DynamicTypeGenerators.ContainsKey(sqlIdentifier))
            return Ok(sqlIdentifier);

        var dynamicTypeGenerator = new DynamicTypeGenerator(sqlQuery, typeof(IGeneratedEntity), ConnectionString);
        DynamicTypeGenerators.Add(sqlIdentifier, dynamicTypeGenerator);

        return Ok(sqlIdentifier);
    }

    [HttpGet("assembly-bytes/{sqlIdentifier}")]
    public ActionResult<string> GetAssemblyBytes(string sqlIdentifier)
    {
        if (!DynamicTypeGenerators.ContainsKey(sqlIdentifier))
            return BadRequest();

        var dynamicTypeGenerator = DynamicTypeGenerators[sqlIdentifier];

        var assemblyBytes = dynamicTypeGenerator.AssemblyString;

        return Ok(assemblyBytes);
    }

    [Route("{*actionName}")]
    public async Task<IActionResult> HandleRequestAsync(string actionName)
    {
        var endpoint = GetEndpoint(actionName);

        if (endpoint == null)
            return BadRequest();

        endpoint.ControllerBase = this;

        var demoSelect = await endpoint.SqlQuery;

        var sqlIdentifier = DynamicTypeGenerator.GetIdentifier(demoSelect);

        var endpointType = endpoint.GetType();

        var queryFilter = endpointType.GetMethod(nameof(IEndpoint<object, FiltrationParams>.QueryFilter))!;

        var filterationParameterType = queryFilter.GetParameters()
            .First(p => p.ParameterType.BaseType == typeof(FiltrationParams)).ParameterType;

        var getFiltrationParamsMethod =
            typeof(HttpExtensions).GetMethod(nameof(HttpExtensions.GetFiltrationParamsAsync))!.MakeGenericMethod(
                filterationParameterType);

        dynamic filterationParamsTask = getFiltrationParamsMethod.Invoke(null, new object[] { HttpContext })!;

        var filtrationParams = await filterationParamsTask;

        DynamicTypeGenerators.TryGetValue(sqlIdentifier, out var dynamicTypeGenerator);

        if (dynamicTypeGenerator == null)
        {
            if (filtrationParams == null) return BadRequest();

            dynamicTypeGenerator = new DynamicTypeGenerator(demoSelect, typeof(IGeneratedEntity), ConnectionString);

            DynamicTypeGenerators.Add(sqlIdentifier, dynamicTypeGenerator);
        }

        var generatedSp7Context = typeof(Sp7Context<>).MakeGenericType(dynamicTypeGenerator.Model);

        dynamic sp6Context = Activator.CreateInstance(generatedSp7Context, new DbContextOptionsBuilder().UseSqlServer(ConnectionString).Options)!;

        var fromSqlRawMethod = typeof(RelationalQueryableExtensions).GetMethod(nameof(RelationalQueryableExtensions.FromSqlRaw))!;

        var genericFromSqlRawMethod = fromSqlRawMethod.MakeGenericMethod(dynamicTypeGenerator.Model);

        dynamic selection = genericFromSqlRawMethod.Invoke(null, new object[] { sp6Context.GeneratedModel, demoSelect, Array.Empty<object>() })!;

        var genericFiltration = queryFilter.MakeGenericMethod(dynamicTypeGenerator.Model);

        var genericFiltrationTask = new Task<dynamic>(() =>
            genericFiltration.Invoke(endpoint, new object[] { selection, filtrationParams })!);

        genericFiltrationTask.Start();

        var result = await genericFiltrationTask;

        var headerType = _mapper.ConfigurationProvider.Internal().GetAllTypeMaps()
            .First(t => t.SourceType == filterationParameterType).DestinationType;

        var addResultHeaderMethod = GetType().GetMethod(nameof(AddResultHeader))!;

        var genericAddResultMethod = addResultHeaderMethod.MakeGenericMethod(dynamicTypeGenerator.Model, filterationParameterType, headerType);

        dynamic addResultHeaderTask = genericAddResultMethod.Invoke(this, new object[] { result, filtrationParams })!;

        var modifiedModels = await addResultHeaderTask;

        var endpointMethod = endpointType.GetMethod(nameof(IEndpoint<object, FiltrationParams>.InMemoryProcessingAsync))!;

        var genericEndpointMethod = endpointMethod.MakeGenericMethod(dynamicTypeGenerator.Model);

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
        Task<string> SqlQuery { get; }

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

        public Task<string> SqlQuery => GetSqlQueryAsync();
        protected virtual async Task<string> GetSqlQueryAsync() => throw new NotImplementedException();

        public abstract IQueryable<TGenerated> QueryFilter<TGenerated>(IQueryable<TGenerated> queryable, TParams filtrationParams) where TGenerated : class, TInterface;

        public abstract Task<IActionResult> InMemoryProcessingAsync<TGenerated>(List<TGenerated> paginatedResult, TParams filtrationParams) where TGenerated : class, TInterface;
    }
}