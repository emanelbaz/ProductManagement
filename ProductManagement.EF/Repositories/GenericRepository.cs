using Microsoft.EntityFrameworkCore;
using ProductManagement.Core.Interfaces;
using ProductManagement.EF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.EF.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _context.Set<T>().ToListAsync();

        public async Task<T?> GetByIdAsync(int id) =>
            await _context.Set<T>().FindAsync(id);

        public async Task AddAsync(T entity) =>
            await _context.Set<T>().AddAsync(entity);

        public void Update(T entity) =>
            _context.Set<T>().Update(entity);

        public void Delete(T entity) =>
            _context.Set<T>().Remove(entity);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _context.Set<T>().Where(predicate).ToListAsync();
    }
}
