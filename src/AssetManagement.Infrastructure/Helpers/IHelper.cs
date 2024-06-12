using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Infrastructure.Helpers
{
    public interface IHelper
    {
        string GetUsername (string Firstname, string Lastname);
    }
}
