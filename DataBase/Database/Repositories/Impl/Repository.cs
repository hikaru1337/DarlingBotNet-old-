using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DarlingBotNet.DataBase
{
    public abstract class Repository<T> : IRepository<T> where T : DbEntity
    {
        protected DbContext _context { get; set; }
        protected DbSet<T> _set { get; set; }

        public Repository(DbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public void Add(T obj) =>
            _set.Add(obj);

        public void AddRange(params T[] objs) =>
            _set.AddRange(objs);

        public IEnumerable<T> GetAll() =>
            _set.ToList();

        //public void Remove(int id) =>
        //    _set.Remove(this.GetById(id));

        public void Remove(T obj) =>
            _set.Remove(obj);

        public void RemoveRange(IEnumerable<T> objs) =>
            _set.RemoveRange(objs);

        public void Update(T obj)
        {
            _context.Entry(obj).State = EntityState.Modified;
            _context.SaveChangesAsync();
            //_set.Update(obj);
        }

        public void UpdateRange(params T[] objs) =>
            _set.UpdateRange(objs);

        public void UpdateRange(IEnumerable<T> objs) => _set.UpdateRange(objs);
    }
}
