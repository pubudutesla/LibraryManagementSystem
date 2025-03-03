using AutoMapper;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Book, BookDto>().ReverseMap();
            CreateMap<Member, MemberResponseDto>();
            CreateMap<MemberRegistrationDto, Member>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}