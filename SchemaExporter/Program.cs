using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Objects.Perks;
using OuterWildsRPG.Objects.Quests;
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
            GenerateSchema<DropListData>("drops");
            GenerateSchema<DropData>("drop");
            GenerateSchema<PerkListData>("perks");
            GenerateSchema<PerkData>("perk");
            GenerateSchema<QuestListData>("quests");
            GenerateSchema<QuestData>("quest");
        }

        static void GenerateSchema<T>(string name)
        {
            var settings = new JsonSchemaGeneratorSettings()
            {
                FlattenInheritanceHierarchy = true,
                AllowReferencesWithProperties = true,
                DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull,
                SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
                {
                    Converters = {
                        new StringEnumConverter() {
                            NamingStrategy = new CamelCaseNamingStrategy()
                        }
                    }
                }
            };
            var schema = JsonSchema.FromType<T>(settings);
            var schemaJson = schema.ToJson();

            var filePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), $"../../../schemas/{name}.schema.json"));
            using (StreamWriter w = new StreamWriter(filePath))
            {
                w.WriteLine(schemaJson);
            }
        }
    }
}
