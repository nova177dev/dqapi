using Dapper;
using dqapi.Application.Common;
using dqapi.Domain.Entities;
using dqapi.Infrastructure.DTOs;
using System.Data;
using System.Reflection.Metadata;
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
            string? jsonResponse = _dbConnection.QueryFirstOrDefault<string>(
                schema + "." + storedProcedureName,
                new { @params = _jsonHelper.SerializeObject(requestParams) },
                commandType: CommandType.StoredProcedure
            );

            return _jsonHelper.DeserializeJson<JsonElement>(jsonResponse);
        }

        //public (JsonElement? Entity, ErrorMessage? Error) requestDbForJson2(string schema, string storedProcedureName, object requestParams)
        //{
        //    string? jsonResponse = _dbConnection.QueryFirstOrDefault<string>(
        //        schema + "." + storedProcedureName,
        //        new { @params = _jsonHelper.SerializeObject(requestParams) },
        //        commandType: CommandType.StoredProcedure
        //    );

        //    var result = _jsonHelper.DeserializeJson<JsonElement>(jsonResponse);

        //    return result.GetProperty("ResponseCode").GetInt32() switch
        //    {
        //        200 => (result, null),
        //        201 => (result, null),
        //        _ => (null, new ErrorMessage { ResponseCode = 400, ResponseMessage = "Bad Request" })
        //    };
        //}
    }
}
