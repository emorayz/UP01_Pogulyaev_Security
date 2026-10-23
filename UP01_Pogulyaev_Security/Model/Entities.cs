using System.Data.Entity;

namespace UP01_Pogulyaev_Security.Model
{
    /// <summary>
    /// Контекст базы данных DB_Pogulyaev_Security (Entity Framework 6, Code First к существующей БД).
    /// Реализован как одиночка через метод GetContext() — контекст создаётся один раз
    /// и вызывается по мере необходимости.
    /// </summary>
    public class Entities : DbContext
    {
        private static Entities _context;

        static Entities()
        {
            // База данных уже создана скриптом db/schema.sql — отключаем инициализаторы EF
            Database.SetInitializer<Entities>(null);
        }

        public Entities() : base("name=Entities")
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<ThreatCategory> ThreatCategories { get; set; }
        public virtual DbSet<SecurityIncident> SecurityIncidents { get; set; }

        /// <summary>
        /// Возвращает единственный экземпляр контекста базы данных.
        /// </summary>
        public static Entities GetContext()
        {
            if (_context == null)
                _context = new Entities();
            return _context;
        }
    }
}
