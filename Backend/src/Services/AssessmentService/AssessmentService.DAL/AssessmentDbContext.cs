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

    public virtual DbSet<Answers> Answers { get; set; }

    public virtual DbSet<Questions> Questions { get; set; }

    public virtual DbSet<QuizSubmissions> QuizSubmissions { get; set; }

    public virtual DbSet<Quizzes> Quizzes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=SYN-009738\\SQLEXPRESS;Database=Elearning;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answers>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Answers__3214EC07DD619082");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Answers_Question");
        });

        modelBuilder.Entity<Questions>(entity =>
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

        modelBuilder.Entity<QuizSubmissions>(entity =>
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

        modelBuilder.Entity<Quizzes>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Quizzes__3214EC072EEEE80B");

            entity.Property(e => e.Title).HasMaxLength(250);
            entity.Property(e => e.TotalMarks).HasDefaultValue(0);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
