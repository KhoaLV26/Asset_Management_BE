using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Infrastructure.Helpers
{
    public class Helper : IHelper
    {
        public string GetUsername(string Firstname, string Lastname)
        {
            var username = $"{Firstname.Replace(" ", "")}{string.Join("", Lastname.Split(' ').Select(x => x.Substring(0, 1)))}";
            return username ;
        }
    }
}
