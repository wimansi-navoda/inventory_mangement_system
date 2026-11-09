using System.Data;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;

public static class Db
{
    const string Server = @"(localdb)\MSSQLLocalDB";
    public static string ConnStr =
        $"Server={Server};Database=InventoryDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static void Init()
    {
        using (var c = new SqlConnection(
            $"Server={Server};Database=master;Trusted_Connection=True;TrustServerCertificate=True;"))
        {
            c.Open();
            new SqlCommand("IF DB_ID('InventoryDB') IS NULL CREATE DATABASE InventoryDB", c).ExecuteNonQuery();
        }

        Exec(@"
IF OBJECT_ID('Users') IS NULL CREATE TABLE Users(
  Id INT IDENTITY PRIMARY KEY, Username NVARCHAR(50) UNIQUE NOT NULL,
  Email NVARCHAR(100) UNIQUE, PasswordHash NVARCHAR(200) NOT NULL, ResetCode NVARCHAR(10));
IF OBJECT_ID('Products') IS NULL CREATE TABLE Products(
  ProductID NVARCHAR(20) PRIMARY KEY, ProductName NVARCHAR(100) NOT NULL,
  Category NVARCHAR(50), Price DECIMAL(10,2) NOT NULL, Quantity INT NOT NULL,
  ReorderLevel INT NOT NULL DEFAULT 5);
IF OBJECT_ID('Sales') IS NULL CREATE TABLE Sales(
  SaleID INT IDENTITY PRIMARY KEY, SaleDate DATETIME DEFAULT GETDATE(), TotalAmount DECIMAL(10,2));
IF OBJECT_ID('SaleDetails') IS NULL CREATE TABLE SaleDetails(
  Id INT IDENTITY PRIMARY KEY, SaleID INT REFERENCES Sales(SaleID),
  ProductID NVARCHAR(20) REFERENCES Products(ProductID), Qty INT, UnitPrice DECIMAL(10,2));");

        if ((int)Scalar("SELECT COUNT(*) FROM Users") == 0)
            Exec("INSERT INTO Users(Username,Email,PasswordHash) VALUES(@u,@e,@p)",
                 ("@u", "admin"), ("@e", "admin@example.com"), ("@p", Hash("admin123")));
    }

    static SqlCommand Cmd(SqlConnection c, string sql, (string, object)[] p)
    {
        var cmd = new SqlCommand(sql, c);
        foreach (var (n, v) in p) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        return cmd;
    }

    public static int Exec(string sql, params (string, object)[] p)
    {
        using var c = new SqlConnection(ConnStr); c.Open();
        return Cmd(c, sql, p).ExecuteNonQuery();
    }

    public static object Scalar(string sql, params (string, object)[] p)
    {
        using var c = new SqlConnection(ConnStr); c.Open();
        return Cmd(c, sql, p).ExecuteScalar();
    }

    public static DataTable Table(string sql, params (string, object)[] p)
    {
        using var c = new SqlConnection(ConnStr); c.Open();
        var dt = new DataTable();
        new SqlDataAdapter(Cmd(c, sql, p)).Fill(dt);
        return dt;
    }

    public static string Hash(string pw)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var h = Rfc2898DeriveBytes.Pbkdf2(pw, salt, 100000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(h);
    }

    public static bool Verify(string pw, string stored)
    {
        var parts = stored.Split(':');
        var h = Rfc2898DeriveBytes.Pbkdf2(pw, Convert.FromBase64String(parts[0]), 100000,
                                          HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(h, Convert.FromBase64String(parts[1]));
    }
}