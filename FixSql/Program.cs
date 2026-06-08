using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string cs = "Data Source=azuredemodb.database.windows.net;Initial Catalog=free-sql-db-4227077;Persist Security Info=True;User ID=sqladmin;Password=Amazon@810649;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=0";
        string[] tables = { "FinanceTransactions", "IncomeTransactions", "ExpenseTransactions", "LoanTransactions", "LoanRepayments", "SubsidyTransactions", "MiscellaneousTransactions" };

        using var conn = new SqlConnection(cs);
        await conn.OpenAsync();

        foreach (var table in tables)
        {
            try
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_{table}_Users')
                        ALTER TABLE {table} DROP CONSTRAINT FK_{table}_Users;
                    
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_{table}_UserId' AND object_id = OBJECT_ID('{table}'))
                        DROP INDEX idx_{table}_UserId ON {table};

                    ALTER TABLE {table} ALTER COLUMN UserId NVARCHAR(100) NOT NULL;
                ";
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Successfully altered {table}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to alter {table}: {ex.Message}");
            }
        }
    }
}
