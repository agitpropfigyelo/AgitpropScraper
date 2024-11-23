namespace Agitprop.Consumer.Consumers
{
    using MassTransit;
    using Microsoft.Extensions.Configuration;


    public class ScrapingJobConsumerDefinition : ConsumerDefinition<ScrapingJobConsumer>
    {
        public ScrapingJobConsumerDefinition(IConfiguration configuration)
        {
            var concurLimit = configuration.GetValue<int?>("ConcurrencyLimit");
            if (concurLimit.HasValue) ConcurrentMessageLimit = concurLimit;
        }
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<ScrapingJobConsumer> consumerConfigurator, IRegistrationContext context)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));

            endpointConfigurator.UseInMemoryOutbox(context);
        }
    }
}