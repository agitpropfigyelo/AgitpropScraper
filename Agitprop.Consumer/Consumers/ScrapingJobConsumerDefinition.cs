namespace Agitprop.Consumer.Consumers
{
    using MassTransit;

    public class ScrapingJobConsumerDefinition :
        ConsumerDefinition<ScrapingJobConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<ScrapingJobConsumer> consumerConfigurator, IRegistrationContext context)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));

            endpointConfigurator.UseInMemoryOutbox(context);
        }
    }
}