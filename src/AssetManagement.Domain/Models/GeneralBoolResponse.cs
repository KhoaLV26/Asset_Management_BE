﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Models
{
    public class GeneralBoolResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}