using AutoMapper;
using UserService.BLL.DTOs;
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
