using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.Filtration.Custom;
using SPPaginationDemo.ModelGenerator;

namespace SPPaginationDemo.Controllers;


public class PaginatedController : Sp7ControllerBase
{
    public PaginatedController(IMapper mapper) : base(mapper) { }

    public class DemoSelect : Endpoint<IGeneratedEntity, CustomFiltrationParams>
    {
        //TODO: Add default implementation to Endpoint<TGenerated> class
        protected override async Task<string> GetSqlQueryAsync() => await System.IO.File.ReadAllTextAsync("demo_sql_abfrage.sql");

        public override IQueryable<TGenerated> QueryFilter<TGenerated>(IQueryable<TGenerated> queryable, CustomFiltrationParams filtrationParams)
        {
            var orderedQuery = queryable
                .Where(d => filtrationParams.CustomFilter != null && d.Vorname != null && d.Vorname.ToUpper().Contains(filtrationParams.CustomFilter))
                .OrderBy(d => d.STID)
                .ThenBy(d => d.AGID)
                .ThenBy(d => d.Name)
                .ThenBy(d => d.Vorname);

            return orderedQuery;
        }

        public override async Task<IActionResult> InMemoryProcessingAsync<TGenerated>(List<TGenerated> paginatedResult, CustomFiltrationParams filtrationParams)
        {
            //TODO: List to DataTable extension method

            paginatedResult.ForEach(d => d.Vorname = d.Vorname?.ToUpper());

            //TODO: Make DataTable serializable

            return ControllerBase.Ok(paginatedResult);
        }
    }
}