using System.Collections.Generic;

namespace DarlingBotNet.DataBase
{
    public interface IRepository<T> where T : DbEntity
    {
        IEnumerable<T> GetAll();

        void Add(T obj);
        void AddRange(params T[] objs);

        //void Remove(int id);
        void Remove(T obj);
        void RemoveRange(IEnumerable<T> objs);

        void Update(T obj);
        void UpdateRange(IEnumerable<T> objs);
        void UpdateRange(params T[] objs);
    }
}
