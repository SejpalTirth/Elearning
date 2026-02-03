using AutoMapper;
using DTOs._3UserService;
using UserService.DAL.Models;

namespace UserService.BLL.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
        }
    }
}
