using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface IAssetService
    {
        Task<AssetResponse> CreateAssetAsync(AssetRequest assetRequest);
    }
}