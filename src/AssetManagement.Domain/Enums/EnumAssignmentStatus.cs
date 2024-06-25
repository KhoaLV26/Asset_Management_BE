using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Enums
{
    public enum EnumAssignmentStatus
    {
        Accepted = 1,
        WaitingForAcceptance = 2,
        Declined = 3,
        Returned = 4
    }
}