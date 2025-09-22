// André Nícolas Granemann Coelho
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    protected string ConnectionString { get; private set; }
    protected DatabaseType DatabaseType { get; private set; }
    protected TestBase()
    {
        //var config = CreateSqlServerConfig();
         var config = CreateMySqlConfig();
        ConnectionString = config.ConnectionString;
        DatabaseType = config.DatabaseType;
    }
  //  private (string ConnectionString, DatabaseType DatabaseType) CreateSqlServerConfig()
   // {
    //    var connectionString = "Server=localhost;Database=db_academia_do_ze;User Id=coelho;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=True;";

 //       return (connectionString, DatabaseType.SqlServer);

//    }
//}
    private (string ConnectionString, DatabaseType DatabaseType) CreateMySqlConfig()
    {
        string connectionString = "Server=127.0.0.1;Port=3307;Database=db_academia_do_ze;User Id=root;Password=1234;";

        return (connectionString, DatabaseType.MySql);

    }
}