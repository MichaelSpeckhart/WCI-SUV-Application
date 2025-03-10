//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using WCI_SUV.Core.Interface.Database;
//using WCI_SUV.Core.Entities;
//using WCI_SUV.DB.Data;

//namespace WCI_SUV.DB.Repositories.Implementations
//{
//    public class NodeRepository : IGenericRepository<Node>
//    {
//        protected readonly ApplicationDbContext _dbContext;
//        protected readonly DbSet<Node> _dbSet; 

//        public NodeRepository(ApplicationDbContext context)
//        {
//            _dbContext = context;
//            _dbSet = context.Set<Node>();
//        }

//        public async Task<IEnumerable<Node>> GetAllAsync()
//        {
//            return await _dbSet.ToListAsync();
//        }

//        public async Task<Node> GetByIdAsync(string id)
//        {
//            return await _dbSet.FindAsync(id) ?? throw new NotImplementedException();
//        }



//    }
//}
