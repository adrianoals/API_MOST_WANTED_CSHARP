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
    class FBIClass
    {
        public static string tableName = "FBIData";
        public static void FetchDataForFBI(string connectionString)
        {
            var client = new RestClient("https://api.fbi.gov");
            int page = 1;
            bool keepDoing = true;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                while (keepDoing)
                {
                    var request = new RestRequest($"/@wanted?pageSize={50}&page={page}&sort_on=modified&sort_order=desc", Method.Get);
                    var response = client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        // Processar a resposta
                        Rootobject ro = JsonConvert.DeserializeObject<Rootobject>(response.Content);
                        if (ro.items.Length < 50)
                            keepDoing = false;

                        foreach (var item in ro.items)
                        {
                            using (OracleCommand command = connection.CreateCommand())
                            {
                                // Obtenção dos atributos da classe usando reflexão
                                PropertyInfo[] propriedades = typeof(Item).GetProperties();

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
                                Console.WriteLine($"Wanted inserted: {item.title}");
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
                }
            }
        }
        public class Rootobject
        {
            public int total { get; set; }
            public Item[] items { get; set; }
            public int page { get; set; }
        }

        public class Item
        {
            public string id { get; set; }
            public string race { get; set; }
            public string place_of_birth { get; set; }
            //public string uid { get; set; } // nao pode ter um campo com esse nome
            public string status { get; set; }
            public int? height_min { get; set; }
            public int? weight_max { get; set; }
            public string hair { get; set; }
            public string race_raw { get; set; }
            public string hair_raw { get; set; }
            public string weight { get; set; }
            public int? reward_max { get; set; }
            public string poster_classification { get; set; }
            public string eyes_raw { get; set; }
            public string title { get; set; }
            public int? reward_min { get; set; }
            public string person_classification { get; set; }
            public string url { get; set; }
            public int? weight_min { get; set; }
            public string nationality { get; set; }
            public string description { get; set; }
            public string eyes { get; set; }
            public int? height_max { get; set; }
            public string caution { get; set; }
            public string sex { get; set; }
            public string ncic { get; set; }
            public string[] subjects { get; set; }
            public DateTime modified { get; set; }
            public DateTime publication { get; set; }
            public string path { get; set; }
            //public string[] dates_of_birth_used { get; set; }

            //public object locations { get; set; }
            //public object additional_information { get; set; }
            //public object suspects { get; set; }
            //public object possible_states { get; set; }
            //public object field_offices { get; set; }
            //public object legat_names { get; set; }
            //public object complexion { get; set; }
            //public object age_range { get; set; }
            //public object build { get; set; }
            //public object age_min { get; set; }
            //public object details { get; set; }
            //public object occupations { get; set; }
            //public Image[] images { get; set; }
            //public object aliases { get; set; }
            //public object reward_text { get; set; }
            //public object remarks { get; set; }
            //public object languages { get; set; }
            //public File[] files { get; set; }
            //public object warning_message { get; set; }
            //public object age_max { get; set; }
            //public object scars_and_marks { get; set; }
            //public object possible_countries { get; set; }
            //public object[] coordinates { get; set; }
        }

        public class Image
        {
            public string thumb { get; set; }
            public string original { get; set; }
            public object caption { get; set; }
            public string large { get; set; }
        }

        public class File
        {
            public string name { get; set; }
            public string url { get; set; }
        }
    }
}
