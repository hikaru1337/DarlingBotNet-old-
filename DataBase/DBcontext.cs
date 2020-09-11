using DarlingBotNet.DataBase.RussiaGame;
using DarlingBotNet.Services;
using Microsoft.EntityFrameworkCore;

namespace DarlingBotNet.DataBase
{
    public class DBcontext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Guilds> Guilds { get; set; }
        public DbSet<Channels> Channels { get; set; }
        public DbSet<LVLROLES> LVLROLES { get; set; }
        public DbSet<EmoteClick> EmoteClick { get; set; }
        public DbSet<PrivateChannels> PrivateChannels { get; set; }
        public DbSet<Clans> Clans { get; set; }
        public DbSet<Warns> Warns { get; set; }
        public DbSet<TempUser> TempUser { get; set; }
        public DbSet<RussiaGame_Profile> RG_Profile { get; set; }
        public DbSet<RussiaGame_Item> RG_Item { get; set; }
        public DbSet<RussiaGame_Study> RG_Study { get; set; }
        public DbSet<RussiaGame_Studys> RG_Studys { get; set; }
        public DbSet<RussiaGame_Work> RG_Work { get; set; }

        //public void ApplicationContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            //optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder(BotSettings.ConnectionString).ToString());
            optionsBuilder.UseSqlite(BotSettings.ConnectionString);
            //optionsBuilder.UseSqlServer("Server=DESKTOP-MRVJEIO\\SQLEXPRESS;Database=DarlingBotCore;Trusted_Connection=True;");

        }
    }



    //public interface IGenericRepository<TEntity> where TEntity : class
    //{
    //    TEntity Create(TEntity item);
    //    void CreateRange(IEnumerable<TEntity> item);
    //    IEnumerable<TEntity> Get();
    //    IEnumerable<TEntity> Get(Func<TEntity, bool> predicate);
    //    TEntity GetF(Func<TEntity, bool> predicate);
    //    void Remove(TEntity item);
    //    void Update(TEntity item);
    //    void RemoveRange(IEnumerable<TEntity> entities);
    //}

    //public class EEF<TEntity> : IGenericRepository<TEntity> where TEntity : class
    //{
    //    DbContext _context;
    //    DbSet<TEntity> _dbSet;

    //    public EEF(DbContext context)
    //    {
    //        _context = context;
    //        _dbSet = context.Set<TEntity>();
    //    }

    //    public IEnumerable<TEntity> GetWithInclude(params Expression<Func<TEntity, object>>[] includeProperties)
    //    {
    //        return Include(includeProperties).ToList();
    //    }

    //    public IEnumerable<TEntity> GetWithInclude(Func<TEntity, bool> predicate,
    //        params Expression<Func<TEntity, object>>[] includeProperties)
    //    {
    //        var query = Include(includeProperties);
    //        return query.Where(predicate).ToList();
    //    }

    //    private IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] includeProperties)
    //    {
    //        IQueryable<TEntity> query = _dbSet;
    //        return includeProperties
    //            .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
    //    }

    //    public IEnumerable<TEntity> Get()
    //    {
    //        return _dbSet.ToListAsync().Result;
    //    }

    //    public IEnumerable<TEntity> Get(Func<TEntity, bool> predicate)
    //    {
    //        return _dbSet.Where(predicate).ToList();
    //    }
    //    public TEntity GetF(Func<TEntity, bool> predicate)
    //    {
    //        return _dbSet.FirstOrDefault(predicate);
    //    }

    //    public TEntity Create(TEntity item)
    //    {
    //        _dbSet.Add(item);
    //        _context.SaveChangesAsync();
    //        return item;
    //    }

    //    public void CreateRange(IEnumerable<TEntity> item)
    //    {
    //        _dbSet.AddRange(item);
    //        _context.SaveChangesAsync();
    //    }
    //    public void Update(TEntity item)
    //    {
    //        _context.Entry(item).State = EntityState.Modified;
    //        _context.SaveChangesAsync();
    //    }

    //    public void UpdateRange(IEnumerable<TEntity> item)
    //    {
    //        _context.Entry(item).State = EntityState.Modified;
    //        _context.SaveChangesAsync();
    //    }

    //    public void Remove(TEntity item)
    //    {
    //        _dbSet.Remove(item);
    //        _context.SaveChangesAsync();
    //    }

    //    public void RemoveRange(IEnumerable<TEntity> item)
    //    {
    //        _dbSet.RemoveRange(item);
    //        _context.SaveChangesAsync();
    //    }

    //}
}
