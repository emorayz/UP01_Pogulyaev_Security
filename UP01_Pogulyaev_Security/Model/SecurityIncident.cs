using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UP01_Pogulyaev_Security.Model
{
    /// <summary>
    /// Инцидент информационной безопасности.
    /// Таблица SecurityIncidents базы данных DB_Pogulyaev_Security.
    /// </summary>
    [Table("SecurityIncidents")]
    public class SecurityIncident
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ThreatCategoryID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; }

        // Критичность инцидента: 1 - низкая, 5 - критическая
        [Required]
        [Range(1, 5)]
        public int Severity { get; set; }

        public string Description { get; set; }

        [StringLength(50)]
        public string IPAddress { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        // Навигационные свойства
        [ForeignKey("ThreatCategoryID")]
        public virtual ThreatCategory ThreatCategory { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}
