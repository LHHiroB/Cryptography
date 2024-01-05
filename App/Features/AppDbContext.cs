using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Windows.Documents;
using IOCore.Libs;

namespace IOApp.Features
{
    internal abstract class FileSystemEntity
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public bool IsFavorite { get; set; }
        public string Note { get; set; }
        public ulong CreatedAt { get; private set; }

        public FileSystemEntity(string path)
        {
            Path = path;
            CreatedAt = TimeUtils.GetCurrentUnixTimestamp();
        }
    }

    [Table("Files")]
    internal class FileEntity : FileSystemEntity
    {
        public FileUtils.Type FileType { get; set; }
        public FileSystemItem.PrivacyType PrivacyType { get; set; }

        public FileEntity(string path) : base(path)
        {
        }
    }

    internal class AppDbContext : CoreDbContext
    {
        public static AppDbContext Inst => DBManager.Inst.MainDbContext as AppDbContext;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            base.OnConfiguring(options);

            if (!options.IsConfigured)
            {
#if !INIT
                options.UseModel(CompiledModels.AppDbContextModel.Instance);
#endif
            }
        }

        public DbSet<FileEntity> Files { get; private set; }

        public override void Verify()
        {
            try
            {
                Files.Any();
            }
            catch
            {
                throw;
            }
        }
    }
}