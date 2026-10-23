using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UP01_Pogulyaev_Security.Model
{
    /// <summary>
    /// Пользователь системы (сотрудник отдела ИБ).
    /// Таблица Users базы данных DB_Pogulyaev_Security.
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Login { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        [Required]
        [StringLength(200)]
        public string FIO { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(255)]
        public string Photo { get; set; }

        // Навигационное свойство: инциденты, зарегистрированные пользователем
        public virtual ICollection<SecurityIncident> SecurityIncidents { get; set; } = new List<SecurityIncident>();
    }
}
