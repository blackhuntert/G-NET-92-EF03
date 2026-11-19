using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Entityes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform
{
    internal class OnlineLearningPlatformDbContext : DbContext
    {
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Instructor> Instructors => Set<Instructor>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-0FQ6K7A;Database=OnlineLearningPlatformDb;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OnlineLearningPlatformDbContext).Assembly);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Instructor>().HasData(
                new Instructor { Id = 1, FullName = "Ahmed Tarek", Email = "ahmed.tarek@onlinelearning.com", Bio = "Senior .NET Architect", JoinedDate = new DateTime(2024, 1, 15) },
                new Instructor { Id = 2, FullName = "Sara Nabil", Email = "sara.nabil@onlinelearning.com", Bio = "Full-Stack Developer", JoinedDate = new DateTime(2024, 3, 10) }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Programming", Description = "Software engineering", ParentCategoryId = null },
                new Category { Id = 2, Name = "Backend", Description = "Server-side development", ParentCategoryId = 1 },
                new Category { Id = 3, Name = "ASP.NET", Description = "C# and ASP.NET Core", ParentCategoryId = 2 }
            );

            modelBuilder.Entity<Course>().HasData(
                new Course { Id = 1, Title = "EF Core Mastery", Description = "Deep dive into EF Core", Price = 49.99m, Level = CourseLevel.Intermediate, IsPublished = true, PublishedDate = new DateTime(2025, 2, 1), InstructorId = 1, CategoryId = 3 },
                new Course { Id = 2, Title = "Microservices", Description = "Draft course", Price = 89.99m, Level = CourseLevel.Advanced, IsPublished = false, PublishedDate = null, InstructorId = 2, CategoryId = 2 }
            );

            modelBuilder.Entity<Student>().HasData(
                new Student { Id = 1, FullName = "Omar Hassan", Email = "omar@student.com", DateOfBirth = new DateOnly(2003, 5, 12) },
                new Student { Id = 2, FullName = "Mona Adel", Email = "mona@student.com", DateOfBirth = new DateOnly(2004, 8, 22) }
            );

            modelBuilder.Entity<Enrollment>().HasData(
                new Enrollment { StudentId = 1, CourseId = 1, EnrollmentDate = new DateTime(2025, 2, 10), ProgressPercentage = 40, Status = EnrollmentStatus.Active }
            );
        }
    }
}
