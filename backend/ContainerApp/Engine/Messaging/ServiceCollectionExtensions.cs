using Azure.Messaging.ServiceBus;

namespace Engine.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueue<T, THandler>(this IServiceCollection services, string queueName)
            where THandler : class, IQueueHandler<T>
        {
            services.AddScoped<IQueueHandler<T>, THandler>();
            services.AddSingleton<IQueueListener<T>>(sp =>
                new AzureServiceBusQueueListener<T>(
                    sp.GetRequiredService<ServiceBusClient>(),
                    queueName));
            services.AddHostedService<QueueProcessor<T>>();
            return services;
        }
    }
}
