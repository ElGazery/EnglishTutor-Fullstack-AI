using EnglishTutor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Infrastructure.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Content)
                .IsRequired();

            builder.Property(m => m.Role)
                .IsRequired()
                .HasConversion<string>();   // store enum as string in DB

            builder.Property(m => m.AnswerMode)
                .IsRequired()
                .HasConversion<string>();   // store enum as string in DB

            builder.Property(m => m.CreatedAt)
                .IsRequired();
        }
    }
}
