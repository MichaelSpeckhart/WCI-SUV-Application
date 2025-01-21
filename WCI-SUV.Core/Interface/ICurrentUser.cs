using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Interface
{
    public interface ICurrentUser
    {
        string Id { get; }
        string UserName { get; }
        bool isAuthenticated { get; }
        IEnumerable<string> Roles { get; } 
    }
}
