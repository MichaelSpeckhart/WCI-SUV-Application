using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Interface.Database;
using WCI_SUV.DB.Data;

namespace WCI_SUV.DB.Repositories.Implementations
{
    public class ConveyorRepository : IGenericRepository<Conveyor>
    {

        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<Conveyor> _dbSet;

        public ConveyorRepository(ApplicationDbContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<Conveyor>();
        }

        public async Task<IEnumerable<Conveyor>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<Conveyor> GetByIdAsync(Int64 id)
        {
            return await _dbSet.FindAsync(id) ?? throw new NotImplementedException();
        }

        public async Task<Conveyor> AddAsync(Conveyor entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Conveyor> UpdateAsync(Conveyor entity)
        {
            _dbSet.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Conveyor> DeleteAsync(Int64 id)
        {
            var entity = await GetByIdAsync(id);
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

    }
}
