using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            DatabaseContext db = new DatabaseContext();
            db.Books.ToList();// Burası tetiklendiği anda database context tetikleniyor ve arka planda aşağıdaki book classı çalışıyor ve kontrol edilip database oluşturuluyor.


            //Sürekli eklemesin diye kapattık.
            //Book book = new Book()
            //{
            //    Name = "Hayri Pıtırcık",
            //    Description = "Aleyna cok seviyor.",
            //    PublishDate = DateTime.Now
            //};
            //db.Books.Add(book);
            //db.SaveChanges();
            db.ExecuteInsertFakeData();

            return View();
        }

    }
    public class DatabaseContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DatabaseContext() // Constructor method - yapıcı metod
        {
            Database.SetInitializer(new DbInitializer());
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .MapToStoredProcedures(config =>        //storedprocedures içindeki update,ınsert,delete lerin isimlerini değiştik.
                {
                    config.Insert(i => i.HasName("BookInsertSP"));
                    config.Update(u =>
                    {
                        u.HasName("BookUpdateSP");
                        u.Parameter(p => p.Id, "bookId"); // Storedproceduresdeki update içerisindeki ıd yi bookıd ye çevirdik.Tabloya dokunmadık.
                    });
                    config.Delete(d => d.HasName("BookDeleteSP"));
                });
        }

        public void ExecuteInsertFakeData()
        {
            Database.ExecuteSqlCommand("EXEC InsertFakeDataSP");

        }
        //public void ExecuteGetBooksByPublishDateSP(int startyear, int endyear)
        //{
        //    Database.ExecuteSqlCommand("EXEC ExecuteGetBooksByPublishDateSP @p0,@p1", startyear, endyear);
        //}

    }

    public class DbInitializer : CreateDatabaseIfNotExists<DatabaseContext>
    {
        protected override void Seed(DatabaseContext context) // Database oluştuktan sonraki yapılan işlemlerdir.Seed için database in oluşturulmuş  olması gerekir.
        {
            //context.Database.ExecuteSqlCommand("Select *from books Where Id=@p0 and Id=@p1", 5, 6); //Parametreleri otomatik aldık.
            //context.Database.ExecuteSqlCommand("Select *from books Where Id=@ilk and Id=@son", //Kendimiz parametre isimleri belirledik.yoksq @p0 @p1 olarak direkt çekseydik new SqlParameter kullanmamıza gerek kalmazdı.
            //    new SqlParameter("@ilk", 5),
            //    new SqlParameter("@Son", 6));
            context.Database.ExecuteSqlCommand(
                @"
                        CREATE PROCEDURE InsertFakeDataSP
                        AS
                        BEGIN 
	                    INSERT INTO Books(Name,Description,PublishDate) VALUES('Da Vinci Code','Da Vincinin Sifresi','2003-02-01')
	                    INSERT INTO Books(Name,Description,PublishDate) VALUES('Angels & Demons','Melekler ve Seytanlar','2000-03-30')
	                    INSERT INTO Books(Name,Description,PublishDate) VALUES('Lost Symbol','Kayıp Sembol','2009-01-29')
	                    END");
            context.Database.ExecuteSqlCommand(
                @"
                                        CREATE PROCEDURE GetBooksByPublishDateSP
                    @p0 DATETIME, -- startdate
                    @p1 DATETIME  -- enddate
                    AS
                    BEGIN 
                    	SELECT TBL.PublishDate,COUNT(TBL.PublishDate) AS [Count]
                    	from (
                    	
                    		SELECT YEAR(PublishDate) AS PublishDate
                    		FROM Books 
                    		WHERE YEAR(PublishDate) BETWEEN @p0 AND @p1
                    	) AS TBL
                    	GROUP BY TBL.PublishDate
                    	END");
            context.Database.ExecuteSqlCommand(
                @"
                    CREATE VIEW GetBooksInfoVW
                        AS 
                        SELECT 
                        Id,
                        Name + ' : ' + Description + ' (' + CONVERT(nvarchar(20),PublishDate) + ')' AS INFO FROM Books
                ");
        }
    }

    [Table("Books")]
    public class Book
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? PublishDate { get; set; }

    }
}
