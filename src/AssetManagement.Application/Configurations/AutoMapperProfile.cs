using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Models;
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
            CreateMap<Assignment, AssignmentResponse>();

            CreateMap<ReturnRequest, ReturnRequestResponse>()
                .ForMember(dest => dest.AssetCode, opt => opt.MapFrom(src => src.Assignment.Asset.AssetCode))
                .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.Assignment.Asset.AssetName))
                .ForMember(dest => dest.AcceptanceByName, opt => opt.MapFrom(src => src.UserAccept.Username))
                .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.Assignment.UserBy.Username));
        }
    }
}