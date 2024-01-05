using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IOCore.Libs
{
    public abstract class CoreDbContext : DbContext
    {
        public static string DbPath = Path.Combine("D:\\WorkSpaceFor_Y3\\NT219 - Cryptography\\Project-Cryptography-NT219\\Database", "", "", "main.db");

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                Utils.CreateDirectoryIfNotExist(Path.GetDirectoryName(DbPath));
                options.UseSqlite($"Data Source={DbPath}");
            }
        }

        public abstract void Verify();
    }

    public class DBManager
    {
        private static readonly Lazy<DBManager> lazy = new(() => new());
        public static DBManager Inst => lazy.Value;

        public readonly object Locker = new();

        public CoreDbContext MainDbContext { get; private set; }
        public bool IsLoad { get; private set; }

        private DBManager()
        {
        }

        public void InitAsync(CoreDbContext dbContext, IProgress<bool> progress)
        {
            if (dbContext == null)
            {
                progress?.Report(false);
                return;
            }

            _ = Task.Run(() =>
            {
                progress?.Report(Load(dbContext));
            });
        }

        private bool Load(CoreDbContext dbContext)
        {
            lock (Locker)
            {
                if (dbContext == null)
                    return false;

                MainDbContext = dbContext;

                try
                {
                    MainDbContext.Database.EnsureCreated();
                    MainDbContext.Verify();
                    IsLoad = true;
                    return true;
                }
                catch
                {
                    try
                    {
                        MainDbContext.Database.EnsureDeleted();
                        MainDbContext.Database.EnsureCreated();
                        IsLoad = true;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }
    }
}