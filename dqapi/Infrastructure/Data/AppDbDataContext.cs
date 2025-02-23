using Dapper;
using dqapi.Application.Common;
using System.Data;
using System.Text.Json;

namespace dqapi.Infrastructure.Data
{
    public class AppDbDataContext
    {
        private readonly IDbConnection _dbConnection;
        private readonly JsonHelper _jsonHelper;
        public AppDbDataContext(IDbConnection dbConnection, JsonHelper jsonHelper)
        {
            _dbConnection = dbConnection;
            _jsonHelper = jsonHelper;
        }

        public JsonElement requestDbForJson(string schema, string storedProcedureName, object requestParams)
        {
            string str = _dbConnection.ConnectionString;
            string? jsonResponse = _dbConnection.QueryFirstOrDefault<string>(
                schema + "." + storedProcedureName,
                new { @params = _jsonHelper.SerializeObject(requestParams) },
                commandType: CommandType.StoredProcedure
            );

            return _jsonHelper.DeserializeJson<JsonElement>(jsonResponse);
        }

        public byte[] requestDb(string schema, string storedProcedureName, object requestParams)
        {
            byte[]? dbResponse = _dbConnection.QueryFirstOrDefault<byte[]>(
                schema + "." + storedProcedureName,
                new { @params = _jsonHelper.SerializeObject(requestParams) },
                commandType: CommandType.StoredProcedure
            );

            if (dbResponse == null)
            {
                throw new InvalidOperationException("The database query didn't return any data.");
            }

            return dbResponse;
        }
    }
}
