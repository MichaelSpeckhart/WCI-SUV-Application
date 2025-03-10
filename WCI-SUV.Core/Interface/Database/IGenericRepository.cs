using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Models;

namespace WCI_SUV.Core.Interface.Database
{
    public interface IGenericRepository<T> where T : BaseEntity
    {

        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(Int64 id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(Int64 id);


    }
}
