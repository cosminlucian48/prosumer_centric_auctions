using DissertationProsumerAuctions.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions.DatabaseConnections
{
    public class DatabaseConnection
    {
        private static DatabaseConnection _instance;
        private SQLiteAsyncConnection _database;

        private DatabaseConnection()
        {
            string databasePath = Environment.GetEnvironmentVariable("DB_PATH")
                                  ?? Path.Combine(AppContext.BaseDirectory, "myDatabase.db");
            
            // Fallback for IDE runs where the file may still sit in the project root.
            if (!File.Exists(databasePath))
            {
                databasePath = Path.Combine(Directory.GetCurrentDirectory(), "myDatabase.db");
            }

            _database = new SQLiteAsyncConnection(databasePath);
        }

        public static DatabaseConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseConnection();
                }
                return _instance;
            }
        }

        public SQLiteAsyncConnection Database
        {
            get { return _database; }
        }

        public async Task<List<ProsumerLoadDataModel>> GetProsumerLoadByIdAsync(int id, string timestamp)
        {
            return await _database.QueryAsync<ProsumerLoadDataModel>($"SELECT * from ProsumerLoads where ProsumerId={id} and Time='{timestamp}' LIMIT 1;");
        }

        public async Task<List<ProsumerGeneratorDataModel>> GetProsumerGenerationByIdAsync(int id, string timestamp)
        {
            return await _database.QueryAsync<ProsumerGeneratorDataModel>($"SELECT * from ProsumerGeneration where ProsumerId={id} and Time='{timestamp}' LIMIT 1;");
        }

        public async Task<List<EnergyMarketPriceDataModel>> GetEnergyMarketPricesbyTime(string timestamp)
        {
            return await _database.QueryAsync<EnergyMarketPriceDataModel>($"SELECT * from EnergyMarketPrices where Time='{timestamp}' LIMIT 1;");

        }
    }
}
