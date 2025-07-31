using Newtonsoft.Json;

namespace ToDoAccessor.Models
{
    public class Todo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
