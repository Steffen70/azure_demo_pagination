// ReSharper disable InconsistentNaming
namespace SPPaginationDemo.SqlQueries;

public interface IDemoSelect
{
    public int? STID { get; set; }
    public int? AGID { get; set; }
    public string? Name { get; set; }
    public string? Vorname { get; set; }
}