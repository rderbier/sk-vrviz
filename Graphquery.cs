

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace RDR
{
    public class GraphQLRequest
    {
        public string query { get; set; }
    }
    public class SchemaElement
    {
        public string predicate { get; set; }
        public string type { get; set; }
        public Boolean list { get; set; }
        public Boolean index { get; set; }
    }
    public class Field
    {
        public string name { get; set; }

    }
    public class TypeElement
    {
        public string name { get; set; }
        public Field[] fields { get; set; }

    }
    public class GraphSchema
    {
        public SchemaElement[] schema { get; set; }
        public TypeElement[] types { get; set; }

    }
    public class SchemaResponse
    {
        public GraphSchema data { get; set; }
    }
    public static class Graphquery
    {
        private static HttpClient client = new HttpClient();


        public static async Task<String> GetData()
        {


            var data = new StringContent("schema{}", Encoding.UTF8, "application/dql");
            // var request = new RestRequest("query").AddBody(body);
            // var response = await _client.PostAsync(request);
            // var result = response.Content;
            var response = await client.PostAsync("https://play.dgraph.io/query", data);
            string result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine("Graph response " + result);
            return result;
        }
        public static async Task<GraphSchema> GetSchema()
        {
            var body = new GraphQLRequest { query = "query {queryFilm(first:1 offset:1) {id name}}" };
            var data = new StringContent("schema{}", Encoding.UTF8, "application/dql");
            // var request = new RestRequest("query").AddBody(body);
            // var response = await _client.PostAsync(request);
            // var result = response.Content;
            var response = await client.PostAsync("https://play.dgraph.io/query", data);
            string jsonString = response.Content.ReadAsStringAsync().Result;
            SchemaResponse resp =
               JsonSerializer.Deserialize<SchemaResponse>(jsonString);

            Console.WriteLine("Graph response " + jsonString);
            return resp.data;

        }


    }

}
