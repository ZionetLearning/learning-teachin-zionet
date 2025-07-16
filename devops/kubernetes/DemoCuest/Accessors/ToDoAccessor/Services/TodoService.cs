using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos;
using ToDoAccessor.Models;
using System.Collections.Concurrent;

namespace ToDoAccessor.Services
{
    public class TodoCosmosDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
    }

    public class TodoService : ITodoService
    {
        private readonly Container _container;

        public TodoService(IOptions<TodoCosmosDbSettings> databaseSettings)
        {
            var settings = databaseSettings.Value;

            var cosmosClient = new CosmosClient(
                settings.ConnectionString,
                new CosmosClientOptions
                {
                    ApplicationName = "ToDoApp"
                });

            var database = cosmosClient.GetDatabase(settings.DatabaseName);
            _container = database.GetContainer(settings.ContainerName);
        }

        public async Task<List<Todo>> GetAllTodosAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<Todo>(query);
            var results = new List<Todo>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.Resource);
            }

            return results;
        }

        public async Task<Todo?> GetTodoByIdAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Todo>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task AddTodoAsync(Todo todo)
        {
            if (string.IsNullOrEmpty(todo.Id))
            {
                todo.Id = Guid.NewGuid().ToString();
            }

            ItemResponse<Todo> response = await _container.CreateItemAsync(
                item: todo,
                partitionKey: new PartitionKey(todo.Id)
            );
        }

        public async Task<bool> UpdateTodoAsync(string id, Todo updatedTodo)
        {
            try
            {
                var response = await _container.ReplaceItemAsync(
                item: updatedTodo,
                    id: id,
                    partitionKey: new PartitionKey(id)
                );

                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTodoAsync(string id)
        {
            try
            {
                var response = await _container.DeleteItemAsync<Todo>(id, new PartitionKey(id));
                return response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
