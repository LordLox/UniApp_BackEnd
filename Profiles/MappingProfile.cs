using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Define mappings between DTOs and domain models
        CreateMap<User, UserCreateDto>().ReverseMap();
        CreateMap<User, UserUpdateDto>().ReverseMap();
        CreateMap<User, BarcodeDataDto>().ReverseMap();
        CreateMap<User, UserInfoDto>().ReverseMap();
        CreateMap<Event, EventCreateDto>().ReverseMap();
        CreateMap<Event, EventUpdateDto>().ReverseMap();
    }
}