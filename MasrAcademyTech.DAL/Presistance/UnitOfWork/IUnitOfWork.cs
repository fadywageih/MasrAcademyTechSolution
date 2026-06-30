using MasrAcademyTech.DAL.Presistance.Repositories.Generic;

namespace MasrAcademyTech.DAL.Presistance.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;
        Task<int> CompleteAsync();
    }
}
