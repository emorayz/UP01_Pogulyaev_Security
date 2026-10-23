using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UP01_Pogulyaev_Security.Model
{
    /// <summary>
    /// Категория угрозы информационной безопасности.
    /// Таблица ThreatCategories базы данных DB_Pogulyaev_Security.
    /// </summary>
    [Table("ThreatCategories")]
    public class ThreatCategory
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        // Навигационное свойство: инциденты данной категории
        public virtual ICollection<SecurityIncident> SecurityIncidents { get; set; } = new List<SecurityIncident>();
    }
}
