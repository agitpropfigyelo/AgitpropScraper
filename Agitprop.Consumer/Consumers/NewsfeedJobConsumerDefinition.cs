namespace Agitprop.Consumer.Consumers
{
    using MassTransit;

    using Microsoft.Extensions.Configuration;


    public class NewsfeedJobConsumerDefinition : ConsumerDefinition<NewsfeedJobConsumer>
    {
        public NewsfeedJobConsumerDefinition(IConfiguration configuration)
        {
            var concurLimit = configuration.GetValue<int?>("Infrastructure:ConcurrencyLimit");
            if (concurLimit.HasValue) ConcurrentMessageLimit = concurLimit;
        }
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<NewsfeedJobConsumer> consumerConfigurator, IRegistrationContext context)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));

            endpointConfigurator.UseInMemoryOutbox(context);
        }
    }
}
