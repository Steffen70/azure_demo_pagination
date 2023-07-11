using AutoMapper;
using SPPaginationDemo.Filtration.Custom;

namespace SPPaginationDemo.Filtration;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<FiltrationParams, FiltrationHeader>();
        CreateMap<CustomFiltrationParams, CustomFilterationHeader>();
    }
}
