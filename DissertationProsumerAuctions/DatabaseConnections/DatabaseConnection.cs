using DissertationProsumerAuctions.Models;
using Serilog;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions.DatabaseConnections
{
    /// <summary>
    /// Singleton implementation of database connection.
    /// Implements IDatabaseConnection for dependency injection support.
    /// TODO: Consider migrating to dependency injection container in the future.
    /// </summary>
    public class DatabaseConnection : IDatabaseConnection
    {
        private static DatabaseConnection _instance;
        private SQLiteAsyncConnection _database;

        private DatabaseConnection()
        {
            // Try to find database in multiple locations
            // 1. Check in base directory (bin/Debug or bin/Release)
            // 2. Check in project root (parent of bin directory)
            // 3. Check in current working directory
            string databasePath = FindDatabaseFile();
            // Log.Information("Database path: {DatabasePath}", databasePath);

            if (string.IsNullOrEmpty(databasePath) || !File.Exists(databasePath))
            {
                throw new FileNotFoundException($"Database file 'myDatabase.db' not found. Searched in: {AppDomain.CurrentDomain.BaseDirectory} and parent directories.");
            }

            _database = new SQLiteAsyncConnection(databasePath);
        }

        private string FindDatabaseFile()
        {
            string fileName = "myDatabase.db";
            
            // First, try to find the database in project root (going up from bin/Debug/net8.0)
            // This ensures we get the real database with data, not an empty one
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 4; i++)
            {
                string testPath = Path.Combine(currentDir, fileName);
                if (File.Exists(testPath))
                {
                    // Check if file has content (not empty)
                    FileInfo fileInfo = new FileInfo(testPath);
                    if (fileInfo.Length > 0)
                    {
                        return testPath;
                    }
                }
                
                currentDir = Directory.GetParent(currentDir)?.FullName ?? "";
                if (string.IsNullOrEmpty(currentDir)) break;
            }

            // Fallback: Try base directory (where executable runs from)
            string baseDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (File.Exists(baseDirPath))
            {
                FileInfo fileInfo = new FileInfo(baseDirPath);
                if (fileInfo.Length > 0)
                {
                    return baseDirPath;
                }
            }

            // Try current working directory
            string workingDirPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            if (File.Exists(workingDirPath))
            {
                FileInfo fileInfo = new FileInfo(workingDirPath);
                if (fileInfo.Length > 0)
                {
                    return workingDirPath;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the singleton instance of DatabaseConnection.
        /// TODO: Replace with dependency injection when DI container is added.
        /// </summary>
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

        /// <summary>
        /// Gets the singleton instance as IDatabaseConnection interface.
        /// </summary>
        public static IDatabaseConnection InstanceAsInterface => Instance;

        public SQLiteAsyncConnection Database
        {
            get { return _database; }
        }

        public async Task<List<ProsumerLoadDataModel>> GetProsumerLoadByIdAsync(int id, string timestamp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(timestamp))
                {
                    return new List<ProsumerLoadDataModel>();
                }

                return await _database.QueryAsync<ProsumerLoadDataModel>(
                    "SELECT * FROM ProsumerLoads WHERE ProsumerId = ? AND Time = ? LIMIT 1;", 
                    id, timestamp);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error retrieving prosumer load data for ProsumerId: {ProsumerId}, Time: {Time}", id, timestamp);
                return new List<ProsumerLoadDataModel>();
            }
        }

        public async Task<List<ProsumerGeneratorDataModel>> GetProsumerGenerationByIdAsync(int id, string timestamp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(timestamp))
                {
                    return new List<ProsumerGeneratorDataModel>();
                }

                return await _database.QueryAsync<ProsumerGeneratorDataModel>(
                    "SELECT * FROM ProsumerGeneration WHERE ProsumerId = ? AND Time = ? LIMIT 1;", 
                    id, timestamp);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error retrieving prosumer generation data for ProsumerId: {ProsumerId}, Time: {Time}", id, timestamp);
                return new List<ProsumerGeneratorDataModel>();
            }
        }

        public async Task<List<EnergyMarketPriceDataModel>> GetEnergyMarketPricesbyTime(string timestamp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(timestamp))
                {
                    return new List<EnergyMarketPriceDataModel>();
                }

                return await _database.QueryAsync<EnergyMarketPriceDataModel>(
                    "SELECT * FROM EnergyMarketPrices WHERE Time = ? LIMIT 1;", 
                    timestamp);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error retrieving energy market price data for Time: {Time}", timestamp);
                return new List<EnergyMarketPriceDataModel>();
            }
        }
    }
}
