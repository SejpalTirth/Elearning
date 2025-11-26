using System;
using System.Collections.Generic;
using AssessmentService.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.DAL;

public partial class AssessmentDbContext : DbContext
{
    public AssessmentDbContext()
    {
    }

    public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizSubmission> QuizSubmissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Answers__3214EC07DD619082");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Answers_Question");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC07B14F9733");

            entity.Property(e => e.Marks).HasDefaultValue(1);
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .HasDefaultValue("MCQ");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK_Questions_Quiz");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Quizzes__3214EC072EEEE80B");

            entity.Property(e => e.Title).HasMaxLength(250);
            entity.Property(e => e.TotalMarks).HasDefaultValue(0);
        });

        modelBuilder.Entity<QuizSubmission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__QuizSubm__3214EC0707C1C574");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizSubmissions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK_QuizSubmissions_Quiz");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
