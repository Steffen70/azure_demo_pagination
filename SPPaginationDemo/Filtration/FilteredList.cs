using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SPPaginationDemo.CallLogger;

namespace SPPaginationDemo.Filtration;

public class FilteredList<TList, THeader> where THeader : FiltrationHeader
{
    [Log]
    public THeader Header { get; private set; } = null!;
    [Log]
    public List<TList> Result { get; } = new();

    [Log]
    public FilteredList() { }

    [Log]
    public FilteredList(IEnumerable<TList> items, THeader header)
    {
        Header = header;

        Result.AddRange(items);
    }

    [Log]
    public static async Task<FilteredList<TList, THeader>> CreateAsync<TParams>(
        IQueryable<TList> source, TParams @params, IMapper mapper)
        where TParams : FiltrationParams
    {
        var (items, header) = await FetchDataAsync<TParams>(source, @params, mapper);

        return new FilteredList<TList, THeader>(items, header);
    }

    [Log]
    public static async Task<FilteredList<TList, THeader>> CreateAndMapInMemoryAsync<TParams, TEntity>(
        IQueryable<TEntity> source, TParams @params, IMapper mapper)
        where TParams : FiltrationParams
    {
        var (items, header) = await FetchDataAsync(source, @params, mapper);

        var dtos = mapper.Map<IEnumerable<TList>>(items);

        return new FilteredList<TList, THeader>(dtos, header);
    }

    [Log]
    protected static async Task<Tuple<IEnumerable<TList>, THeader>> FetchDataAsync<TParams>(
        IQueryable<TList> source, TParams @params, IMapper mapper)
        where TParams : FiltrationParams
        => await FetchDataAsync<TList, TParams>(source, @params, mapper);

    [Log]
    protected static async Task<Tuple<IEnumerable<T>, THeader>> FetchDataAsync<T, TParams>(
        IQueryable<T> source, TParams @params, IMapper mapper)
        where TParams : FiltrationParams
    {
        var header = mapper.Map<THeader>(@params);

        header.TotalItems = await source.CountAsync();

        var items = await source
            .Skip((header.CurrentPage - 1) * header.PageSize)
            .Take(header.PageSize)
            .ToListAsync();

        header.TotalPages = (int)Math.Ceiling(header.TotalItems / (double)header.PageSize);

        return new Tuple<IEnumerable<T>, THeader>(items, header);
    }
}