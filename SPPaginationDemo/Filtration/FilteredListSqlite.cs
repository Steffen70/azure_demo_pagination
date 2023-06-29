using System.Data;
using System.Data.SQLite;
using AutoMapper;

namespace SPPaginationDemo.Filtration;

public class FilteredListSqlite<THeader> where THeader : FiltrationHeader
{
    public THeader Header { get; }
    public DataTable Result { get; } = new();

    public FilteredListSqlite(DataTable items, THeader header)
    {
        Header = header;

        Result.Merge(items);
    }

    public static async Task<FilteredListSqlite<THeader>> CreateAsync<TParams>(
        string source, TParams @params, IMapper mapper)
        where TParams : FiltrationParams
        => await CreateAsync(source, source.Replace("SELECT * FROM", "SELECT COUNT(*) FROM", StringComparison.CurrentCultureIgnoreCase), @params, mapper);

    public static async Task<FilteredListSqlite<THeader>> CreateAsync<TParams>(
        string source, string countSql, TParams @params, IMapper mapper)
        where TParams : FiltrationParams
    {
        var header = mapper.Map<THeader>(@params);

        await using var connection = new SQLiteConnection(@"Data Source=sp_pagination_demo.sqlite;");
        await connection.OpenAsync();

        // Query to count total items
        await using (var countCommand = new SQLiteCommand(countSql, connection))
        {
            header.TotalItems = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
        }

        // Query to fetch items with LIMIT and OFFSET
        var query = $"{source} LIMIT @Limit OFFSET @Offset";

        await using var fetchCommand = new SQLiteCommand(query, connection);

        fetchCommand.Parameters.AddWithValue("@Limit", @params.PageSize);
        fetchCommand.Parameters.AddWithValue("@Offset", (@params.CurrentPage - 1) * @params.PageSize);

        await using var reader = await fetchCommand.ExecuteReaderAsync();

        var dataTable = new DataTable();
        dataTable.Load(reader);

        header.TotalPages = (int)Math.Ceiling(header.TotalItems / (double)@params.PageSize);

        return new FilteredListSqlite<THeader>(dataTable, header);
    }
}