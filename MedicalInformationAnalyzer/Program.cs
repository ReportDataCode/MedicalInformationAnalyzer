using Azure;
using System;
using Azure.AI.TextAnalytics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TextMiner
{
    class Program
    {
        // Replace the key variable with the key for your Language resource.
        private static readonly AzureKeyCredential credentials = new("replace-with-your-key-here");

        // Replace the endpointUri variable with the endpoint for your Language resource.
        private static readonly Uri endpointUri = new("replace-with-your-endpoint-here");

        // Sample method for extracting information from health-care text.
        static async Task healthInformationExtractorTask(TextAnalyticsClient client,
            string document = "Prescribed 100mg ibuprofen, taken twice daily.")
        {
            List<string> batchInputList = new List<string>()
            {
                document
            };
            AnalyzeHealthcareEntitiesOperation healthcareEntitiesOperation =
                await client.StartAnalyzeHealthcareEntitiesAsync(batchInputList);
            await healthcareEntitiesOperation.WaitForCompletionAsync();

            await foreach (AnalyzeHealthcareEntitiesResultCollection documentsInPage in healthcareEntitiesOperation
                               .Value)
            {
                Console.WriteLine(
                    $"Results of Azure Text Analytic for health async model, version: \"{documentsInPage.ModelVersion}\"");
                Console.WriteLine("");

                foreach (AnalyzeHealthcareEntitiesResult entitiesInDoc in documentsInPage)
                {
                    if (!entitiesInDoc.HasError)
                    {
                        foreach (HealthcareEntity entity in entitiesInDoc.Entities)
                        {
                            // View Recognized healthcare entities.
                            Console.WriteLine($"  Entity: {entity.Text}");
                            Console.WriteLine($"  Category: {entity.Category}");
                            Console.WriteLine($"  Offset: {entity.Offset}");
                            Console.WriteLine($"  Length: {entity.Length}");
                            Console.WriteLine($"  NormalizedText: {entity.NormalizedText}");
                        }

                        Console.WriteLine(
                            $"  Found {entitiesInDoc.EntityRelations.Count} relations in the current document:");
                        Console.WriteLine("");

                        // View recognized healthcare relations.
                        foreach (HealthcareEntityRelation healthcareEntityRelation in entitiesInDoc.EntityRelations)
                        {
                            Console.WriteLine($"    Relation: {healthcareEntityRelation.RelationType}");
                            Console.WriteLine(
                                $"    For this relation there are {healthcareEntityRelation.Roles.Count} roles");

                            // View relation roles.
                            foreach (HealthcareEntityRelationRole entityRelationRole in healthcareEntityRelation.Roles)
                            {
                                Console.WriteLine($"      Role Name: {entityRelationRole.Name}");

                                Console.WriteLine($"      Associated Entity Text: {entityRelationRole.Entity.Text}");
                                Console.WriteLine(
                                    $"      Associated Entity Category: {entityRelationRole.Entity.Category}");
                                Console.WriteLine("");
                            }

                            Console.WriteLine("");
                        }
                    }
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine($"  Document error code: {entitiesInDoc.Error.ErrorCode}.");
                        Console.WriteLine($"  Message: {entitiesInDoc.Error.Message}");
                    }

                    Console.WriteLine("");
                }
            }
        }

        static async Task Main(string[] argStrings)
        {
            var client = new TextAnalyticsClient(endpointUri, credentials);
            await healthInformationExtractorTask(client);
        }
    }
}