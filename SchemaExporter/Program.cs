using Newtonsoft.Json.Schema.Generation;
using OuterWildsRPG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GenerateSchema<QuestFileData>("quests");
            GenerateSchema<QuestData>("quest");
        }

        static void GenerateSchema<T>(string name)
        {
            var generator = new JSchemaGenerator();
            generator.GenerationProviders.Add(new StringEnumGenerationProvider()
            {
                CamelCaseText = true,
            });
            generator.DefaultRequired = Newtonsoft.Json.Required.DisallowNull;
            var schema = generator.Generate(typeof(T));
            var filePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), $"../../../schemas/{name}.schema.json"));
            using (StreamWriter w = new StreamWriter(filePath))
            {
                w.WriteLine(schema.ToString());
            }
        }
    }
}
