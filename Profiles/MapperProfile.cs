using AutoMapper;
using Hangman.Models;
using Hangman.Models.ResponseModels;

namespace Hangman.Profiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<User,UserRegisterResponseDto>();
        }
    }
}
