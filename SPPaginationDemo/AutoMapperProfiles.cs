using AutoMapper;
using SPPaginationDemo.Dto;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.Filtration.Custom;
using SPPaginationDemo.Model;

namespace SPPaginationDemo;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<WeatherForecast, WeatherForecastDto>();

        CreateMap<FiltrationParams, FiltrationHeader>();
        CreateMap(typeof(FilteredList<,>), typeof(FilteredList<>));

        CreateMap<DayFilterParams, DayFilterHeader>();
    }
}