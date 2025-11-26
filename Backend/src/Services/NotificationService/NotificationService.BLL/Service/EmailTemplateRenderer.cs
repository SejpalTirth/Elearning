using System.Text.Json;

namespace NotificationService.BLL.Service
{
    public static class EmailTemplateRenderer
    {
        public static string Render(string template, object model)
        {
            string output = template;

            JsonElement json;

            if (model is JsonElement je)
            {
                json = je;
            }
            else
            {
                json = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(model));
            }

            foreach (var prop in json.EnumerateObject())
            {
                string key = prop.Name;
                string value = prop.Value.ToString();

                output = output.Replace($"{{{{{key}}}}}", value) // Supports {{Key}}
                               .Replace($"{{{key}}}", value);      // Supports {Key}
            }

            return output;
        }
    }
}
