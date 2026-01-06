using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using TdA26_CyganStudios.Models.Db;

namespace TdA26_CyganStudios;

public sealed class AppDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<DbCourse> Courses { get; set; }

    public DbSet<DbMaterial> Materials { get; set; }

    public DbSet<DbFileMaterial> FileMaterials { get; set; }

    public DbSet<DbUrlMaterial> UrlMaterials { get; set; }

    public DbSet<DbQuiz> Quizzes { get; set; }

    public DbSet<DbQuizSubmision> QuizSubmisions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // DbMaterial
        builder.Entity<DbMaterial>()
            .Property(m => m.Uuid)
            .ValueGeneratedNever();

        builder.Entity<DbMaterial>().UseTpcMappingStrategy();

        builder.Entity<DbFileMaterial>().ToTable("FileMaterials");
        builder.Entity<DbUrlMaterial>().ToTable("UrlMaterials");

        // DbCourse
        builder.Entity<DbCourse>()
            .HasMany(c => c.Materials)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DbCourse>()
            .HasMany(c => c.Quizzes)
            .WithOne(q => q.Course)
            .HasForeignKey(q => q.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // DbQuiz
        builder.Entity<DbQuiz>()
            .Property(m => m.Uuid)
            .ValueGeneratedNever();

        builder.Entity<DbQuiz>()
            .Property(q => q.Questions)
            .HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<DbQuestion>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })!);

        builder.Entity<DbQuiz>()
            .HasMany(q => q.Submisions)
            .WithOne(s => s.Quiz)
            .HasForeignKey(s => s.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        // DbQuizSubmision
        builder.Entity<DbQuizSubmision>()
            .Property(m => m.Uuid)
            .ValueGeneratedNever();

        builder.Entity<DbQuizSubmision>()
            .Property(s => s.Answers)
            .HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<IList<DbQuizAnswer>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })!);
    }
}
