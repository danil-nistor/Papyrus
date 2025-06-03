using Microsoft.EntityFrameworkCore;
using Papyrus.Models;

namespace Papyrus.Data
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            // Статусы документа
            modelBuilder.Entity<Status>().HasData(
                new Status { Id = 1, Name = "На согласовании" },
                new Status { Id = 2, Name = "Одобрен" },
                new Status { Id = 3, Name = "Отклонен" }
            );

            // Отделы
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "HR" },
                new Department { Id = 2, Name = "IT" },
                new Department { Id = 3, Name = "Бухгалтерия" }
            );
        }
    }
}
