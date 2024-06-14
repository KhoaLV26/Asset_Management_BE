using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Configurations
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserRegisterResponse>()
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location.Name))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
            CreateMap<User, GetUserResponse>()
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location.Name))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
            CreateMap<Role, RoleResponse>();
            CreateMap<Category, CategoryResponse>();
            CreateMap<Asset, AssetResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location.Name));
        }
    }
}