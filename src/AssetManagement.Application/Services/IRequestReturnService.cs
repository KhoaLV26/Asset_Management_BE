﻿using AssetManagement.Application.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface IRequestReturnService
    {
        Task<IEnumerable<ReturnRequestResponse>> GetReturnRequestResponses(Guid locationId);
    }
}