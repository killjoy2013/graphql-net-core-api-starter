using GraphQL.WebApi.Models;
using System.Collections.Generic;

namespace GraphQL.WebApi.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        T Insert(T entity);
        T Update(T entity);
        void Delete(int id);
    }
}
