namespace NotificationService.BLL.Service
{
    public static class EmailTemplateRenderer
    {
        public static string Render(string bodyTemplate, object model)
        {
            string output = bodyTemplate;

            foreach (var prop in model.GetType().GetProperties())
            {
                string value = prop.GetValue(model)?.ToString() ?? "";
                output = output.Replace($"{{{{{prop.Name}}}}}", value);
            }

            return output;
        }
    }
}
