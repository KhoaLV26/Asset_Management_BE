using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Enums
{
    public enum EnumAssetStatus
    {
        NotAvailable = 1,
        Available = 2,
        Assigned = 3,
        WaitingForRecycling = 4,
        Recycled = 5
    }
}