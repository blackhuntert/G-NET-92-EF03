using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLearningPlatform.Entityes;

namespace OnlineLearningPlatform.Configurations
{
    internal class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
    {
        public void Configure(EntityTypeBuilder<Instructor> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.FullName).IsRequired().HasMaxLength(150);
            builder.Property(i => i.Email).IsRequired().HasMaxLength(200);
            builder.HasIndex(i => i.Email).IsUnique();
            builder.Property(i => i.Bio).IsRequired(false).HasMaxLength(1000);
            builder.Property(i => i.JoinedDate).IsRequired();
        }
    } 
}
