using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FBIDataCSharp
{
    class InterpolClass
    {
        public static string tableName = "InterpolData";
        public static void FetchDataForInterpol(string connectionString)
        {
            var client = new RestClient("https://ws-public.interpol.int");
            int page = 1;
            bool keepDoing = true;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                while (keepDoing)
                {
                    var request = new RestRequest($"/notices/v1/red?page={page}&resultPerPage=160", Method.Get);
                    var response = client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        //Processar a resposta
                       Rootobject ro = JsonConvert.DeserializeObject<Rootobject>(response.Content);
                        if (ro._embedded.notices.Length < 160)
                            keepDoing = false;

                        foreach (var item in ro._embedded.notices)
                        {
                            using (OracleCommand command = connection.CreateCommand())
                            {
                                // Obtenção dos atributos da classe usando reflexão
                                PropertyInfo[] propriedades = typeof(Notice).GetProperties();

                                // Construção da string de colunas e valores
                                string colunas = string.Join(", ", propriedades.Select(p => p.Name));
                                string valores = string.Join(", ", propriedades.Select(p => $":{p.Name}"));

                                // Montagem da instrução SQL de inserção
                                command.CommandText = $"INSERT INTO {tableName} ({colunas}) VALUES ({valores})";

                                // Adição de parâmetros
                                foreach (PropertyInfo propriedade in propriedades)
                                {
                                    command.Parameters.Add($":{propriedade.Name}", propriedade.GetValue(item) ?? DBNull.Value);
                                }

                                // Execução da instrução SQL
                                command.ExecuteNonQuery();
                                Console.WriteLine($"Wanted inserted: {item.name}");
                            }
                        }
                    }
                    else
                    {
                        // Lidar com erro
                        Console.WriteLine("Erro: " + response.ErrorMessage);
                        keepDoing = false;
                    }
                    Thread.Sleep(500);
                    page++;
                    keepDoing = false;
                }
            }
        }

        public class Rootobject
        {
            public int total { get; set; }
            public Query query { get; set; }
            public _Embedded _embedded { get; set; }
            public _Links1 _links { get; set; }
        }

        public class Query
        {
            public int page { get; set; }
            public int resultPerPage { get; set; }
        }

        public class _Embedded
        {
            public Notice[] notices { get; set; }
        }

        public class Notice
        {
            public string date_of_birth { get; set; }
            //public string[] nationalities { get; set; }
            public string entity_id { get; set; }
            public string forename { get; set; }
            public string name { get; set; }
            //public _Links _links { get; set; }
        }

        public class _Links
        {
            public Self self { get; set; }
            public Images images { get; set; }
            public Thumbnail thumbnail { get; set; }
        }

        public class Self
        {
            public string href { get; set; }
        }

        public class Images
        {
            public string href { get; set; }
        }

        public class Thumbnail
        {
            public string href { get; set; }
        }

        public class _Links1
        {
            public Self1 self { get; set; }
            public First first { get; set; }
            public Last last { get; set; }
        }

        public class Self1
        {
            public string href { get; set; }
        }

        public class First
        {
            public string href { get; set; }
        }

        public class Last
        {
            public string href { get; set; }
        }

    }
}
