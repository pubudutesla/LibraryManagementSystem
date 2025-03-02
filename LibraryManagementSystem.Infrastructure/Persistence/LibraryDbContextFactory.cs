using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LibraryManagementSystem.Infrastructure.Persistence
{
    public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
    {
        public LibraryDbContext CreateDbContext(string[] args)
        {
            var path = Directory.GetCurrentDirectory();

            // Ensure we correctly locate appsettings.json
            if (!File.Exists(Path.Combine(path, "appsettings.json")))
            {
                path = Directory.GetParent(path)?.FullName;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(path)  // Dynamically set the path
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new LibraryDbContext(optionsBuilder.Options);
        }
    }
}
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Configuration;
//using System.IO;

//namespace LibraryManagementSystem.Infrastructure.Persistence
//{
//    public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
//    {
//        public LibraryDbContext CreateDbContext(string[] args)
//        {
//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json")
//                .Build();

//            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
//            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

//            return new LibraryDbContext(optionsBuilder.Options);
//        }
//    }
//}