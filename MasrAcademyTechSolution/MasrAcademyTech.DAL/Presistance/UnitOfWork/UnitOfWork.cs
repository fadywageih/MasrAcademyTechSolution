using MasrAcademyTech.DAL.Presistance.Data;
using MasrAcademyTech.DAL.Presistance.Repositories.Generic;
using System.Collections.Concurrent;

namespace MasrAcademyTech.DAL.Presistance.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();

        public UnitOfWork(ApplicationDBContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            return (IGenericRepository<T>)_repositories.GetOrAdd(typeof(T),
                _ => new GenericRepository<T>(_context));
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
