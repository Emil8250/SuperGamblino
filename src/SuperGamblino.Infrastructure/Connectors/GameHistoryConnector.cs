﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using SuperGamblino.Core.Configuration;
using SuperGamblino.Core.Entities;

namespace SuperGamblino.Infrastructure.Connectors
{
    public class GameHistoryConnector : DatabaseConnector
    {
        private const string GameHistoriesKey = "GameHistoryConnector-GetGameHistories";

        public GameHistoryConnector(ILogger<GameHistoryConnector> logger, ConnectionString connectionString,
            IMemoryCache memoryCache) : base(logger, connectionString, memoryCache)
        {
        }

        public virtual async Task<IEnumerable<GameHistory>> GetGameHistories(ulong userId)
        {
            if (MemoryCache.TryGetValue(GameHistoriesKey + userId, out IEnumerable<GameHistory> gameHistories))
                return gameHistories;
            await using var connection = new MySqlConnection(ConnectionString);
            try
            {
                await connection.OpenAsync();
                var result = (await connection.GetAllAsync<GameHistory>()).AsList();
                MemoryCache.Set(GameHistoriesKey, result);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Exception occured while executing GetGameHistories method in Database class!");
                return null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public virtual async Task<bool> AddGameHistory(GameHistory history)
        {
            MemoryCache.Remove(GameHistoriesKey);
            await using var connection = new MySqlConnection(ConnectionString);
            try
            {
                await connection.OpenAsync();
                await connection.InsertAsync(history);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Exception occured while executing AddGameHistory with method in Database class!");
                return false;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}