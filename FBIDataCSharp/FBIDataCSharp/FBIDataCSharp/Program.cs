using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Reflection;
using Newtonsoft.Json;
using System.Threading;

namespace FBIDataCSharp
{
    class Program
    {
        static string connectionString = "User Id=RM95667;Password=281088;Data Source=oracle.fiap.com.br:1521/ORCL";
        static void Main(string[] args)
        {
            CriarTabelaNoOracle<FBIClass.Item>(FBIClass.tableName);
            CriarTabelaNoOracle<InterpolClass.Notice>(InterpolClass.tableName);

            FBIClass.FetchDataForFBI(connectionString);
            InterpolClass.FetchDataForInterpol(connectionString);

            //using (OracleConnection connection = new OracleConnection(connectionString))
            //{
            //    connection.Open();
            //    using (OracleCommand command = connection.CreateCommand())
            //    {
            //        command.CommandText = $"SELECT * FROM {tableName}";

            //        using (OracleDataReader reader = command.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                // Processar os resultados
            //                Console.WriteLine(reader["Nome"]);
            //            }
            //        }
            //    }
            //}
        }

        static void CriarTabelaNoOracle<T>(string nomeTabela)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (OracleCommand command = connection.CreateCommand())
                {
                    try
                    {
                        command.CommandText = $"DROP TABLE {nomeTabela}";
                        command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                        //a tabela pode não existir
                    }
                }

                // Criação do comando para criar a tabela
                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = $"CREATE TABLE {nomeTabela} (";
                    PropertyInfo[] propriedades = typeof(T).GetProperties();
                    foreach (PropertyInfo propriedade in propriedades)
                    {
                        string fieldType = "VARCHAR2(255)";
                        switch (propriedade.PropertyType)
                        {
                            case Type t when t == typeof(string):
                                fieldType = "CLOB";
                                break;
                            case Type t when t == typeof(int):
                                fieldType = "NUMBER";
                                break;
                            case Type t when t == typeof(DateTime):
                                fieldType = "DATE";
                                break;
                            default:
                                break;
                        }

                        command.CommandText += $"{propriedade.Name} {fieldType}, ";
                    }
                    command.CommandText = command.CommandText.TrimEnd(',', ' ') + ")";
                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"Tabela '{nomeTabela}' criada com sucesso no banco de dados Oracle!");
        }

       
    }
}
