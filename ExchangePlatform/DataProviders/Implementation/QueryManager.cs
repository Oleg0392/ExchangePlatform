using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using ExchangePlatform.DataProviders.Intrefaces;

namespace ExchangePlatform.DataProviders.Implenetation
{
    public class QueryManager : IQueryManager
    {
        IConfiguration configuration { get; set; }
        protected object[,] ResultObjectArray2D;
        protected object[] ResultObjectArray1D { get; set; }
        protected object ResultObject { get; set; }
        
        public QueryManager(IConfiguration config)
        {
            configuration = config;
        }

        public int ExecuteNonQuery(SqlCommand command)
        {
            int queryResult = 0;
            
            SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));  //WorkMachineDb
            //SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("WorkMachineDb"));
            command.Connection = sqlConnection;
            sqlConnection.Open();
            using(sqlConnection)
            {
                queryResult = command.ExecuteNonQuery();
            }
            sqlConnection.Close();
            return queryResult;
        }

        public void ExecuteQuery(SqlCommand command)
        {
            ClearResultFields();
            SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            //SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("WorkMachineDb"));
            command.Connection = sqlConnection;
            List<object[]> listData = new List<object[]>();            
            int VisibleFieldCount = 0;
            sqlConnection.Open();
            using (sqlConnection)
            {
                SqlDataReader dataReader = command.ExecuteReader();
                if (dataReader == null) return;
                if (dataReader.HasRows)
                {
                    int RowIndex = 0;
                    VisibleFieldCount = dataReader.VisibleFieldCount;
                    while (dataReader.Read())
                    {
                        listData.Add(new object[VisibleFieldCount]);
                        dataReader.GetValues(listData[RowIndex]);
                        RowIndex++;
                    }
                    //successExecuting = true;
                }
                else return;
            }
            sqlConnection.Close();

            if (listData.Count == 1)
            {
                if (VisibleFieldCount == 1)                 //одна строка и один столбец
                {
                    ResultObject = listData[0][0];
                    return;
                }

                ResultObjectArray1D = listData[0];          //одна строка и много столбцов
                return;
            }

            if (VisibleFieldCount == 1)                     //много строк и один столбец
            {
                ResultObjectArray1D = new object[listData.Count];
                for (int i = 0; i < listData.Count; i++) ResultObjectArray1D[i] = listData[i][0];
                return;
            }

            ResultObjectArray2D = new object[listData.Count, VisibleFieldCount];     //много строк и много столбцов
            for (int i = 0; i < listData.Count; i++)
            {
                for (int j = 0; j < VisibleFieldCount; j++) ResultObjectArray2D[i, j] = listData[i][j];
            }
        }

        public object GetResultObject()
        {
            if (ResultObject == null)
            {
                if (ResultObjectArray1D != null) return ResultObjectArray1D[0];
                if (ResultObjectArray2D != null) return ResultObjectArray2D[0, 0];
            }
            return ResultObject;
        }

        public object[] GetResultObjectArray1D()
        {
            if (ResultObjectArray1D == null)
            {
                if (ResultObjectArray2D != null)
                {
                    ResultObjectArray1D = new object[ResultObjectArray2D.GetLength(1)];
                    for (int i = 0; i < ResultObjectArray2D.GetLength(1); i++) ResultObjectArray1D[i] = ResultObjectArray2D[0, i];
                    return ResultObjectArray1D;
                }
                if (ResultObject != null) ResultObjectArray1D = new object[1] { ResultObject };

            }
            return ResultObjectArray1D;
        }

        public object[,] GetResultObjectArray2D()
        {
            if (ResultObjectArray2D == null)
            {
                if (ResultObjectArray1D != null)
                {
                    ResultObjectArray2D = new object[1, ResultObjectArray1D.Length];
                    for (int i = 0; i < ResultObjectArray1D.Length; i++) ResultObjectArray2D[0, i] = ResultObjectArray1D[i];
                    return ResultObjectArray2D;
                }
                if (ResultObject != null) ResultObjectArray2D = new object[1, 1] { { ResultObject } };
            }
            return ResultObjectArray2D;
        }

        void ClearResultFields()
        {
            ResultObject = null;
            ResultObjectArray1D = null;
            ResultObjectArray2D = null;
        }

        // асинхронные версии

        public async Task<int> ExecuteNonQueryAsync(SqlCommand command)
        {
            int queryResult = 0;

            SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            command.Connection = sqlConnection;
            sqlConnection.Open();
            using (sqlConnection)
            {
                queryResult = await command.ExecuteNonQueryAsync();
            }
            sqlConnection.Close();
            return queryResult;
        }

        public async Task ExecuteQueryAsync(SqlCommand command)
        {
            SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            command.Connection = sqlConnection;
            List<object[]> listData = new List<object[]>();
            int VisibleFieldCount = 0;
            sqlConnection.Open();
            using (sqlConnection)
            {
                SqlDataReader dataReader = await command.ExecuteReaderAsync();
                if (dataReader == null) return;
                if (dataReader.HasRows)
                {
                    int RowIndex = 0;
                    VisibleFieldCount = dataReader.VisibleFieldCount;
                    while (dataReader.Read())
                    {
                        listData.Add(new object[VisibleFieldCount]);
                        dataReader.GetValues(listData[RowIndex]);
                        RowIndex++;
                    }
                }
                else return;
            }
            sqlConnection.Close();

            if (listData.Count == 1)
            {
                if (VisibleFieldCount == 1)                 //одна строка и один столбец
                {
                    ResultObject = listData[0][0];
                    return;
                }

                ResultObjectArray1D = listData[0];          //одна строка и много столбцов
                return;
            }

            if (VisibleFieldCount == 1)                     //много строк и один столбец
            {
                ResultObjectArray1D = new object[listData.Count];
                for (int i = 0; i < listData.Count; i++) ResultObjectArray1D[i] = listData[i][0];
                return;
            }

            ResultObjectArray2D = new object[listData.Count, VisibleFieldCount];     //много строк и много столбцов
            for (int i = 0; i < listData.Count; i++)
            {
                for (int j = 0; j < VisibleFieldCount; j++) ResultObjectArray2D[i, j] = listData[i][j];
            }
        }
    }
}
