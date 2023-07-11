using System.Collections;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace SPPaginationDemo.ModelGenerator;

public interface IPaginationCallback<in TFiltrationParams, in TInterfaced>
{
    ActionResult<IEnumerable> Call<TGenerated>(IQueryable<TGenerated> queryable,
        TFiltrationParams filtrationParams, IMapper autoMapper) where TGenerated : class, TInterfaced;
}