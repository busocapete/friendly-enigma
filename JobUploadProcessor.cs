using System;
using BuSoCa.Data.Config;
using Microsoft.Extensions.Options;
using Azure.Messaging.ServiceBus;
using BuSoCa.Model.CreateDtos;
using System.Diagnostics;
using BuSoCa.Data.Entities;
using System.Text.Json;
using BuSoCa.Model.Dtos;
using BuSoCa.Data.Interfaces;
using BuSoCa.Model.AzureDtos;
using System.Web;
using BuSoCa.Data.Helpers;

namespace BuSoCa.Data.ServiceBus
{
    public class JobUploadProcessor : IJobUploadProcessor
	{  
	    private static ServiceBusClient _client;
	    private readonly IOptions<AzureServiceBusConfig> _options;
	    public static string errorMessage;
	
	    public JobUploadProcessor(IOptions<AzureServiceBusConfig> options)
		{
		    _options = options ?? 
			throw new ArgumentNullException(nameof(options));
			try
			{
				  _client = new ServiceBusClient(
					_options.Value.ConnectionString);
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message.ToString();
				Console.WriteLine
					($"Error initializing jobUploadProcessor. " +
					$"Error:- {errorMessage}");
			}
		}

        public async Task JobsListReceived(IEnumerable<GreenhouseJob> jobs,
	    BusinessForJobUploadProcessorDto business,
            IEnumerable<ProfileTag> businessTags,
            IEnumerable<AzureTag> businessTagsForAzure,
            string ownerId)
        {
            var sender = _client.CreateSender(_options.Value.GHJobsQueueName);

            var watch = Stopwatch.StartNew();

            foreach (var job in jobs)
            {
                var jobAndBusiness = new
                {
                    Job = job,
                    Business = business,
                    BusinessTags = businessTags,
                    TagsForAzure = businessTagsForAzure,
                    OwnerId = ownerId,
                };

                var jobAndBusinessAsJson = JsonSerializer.Serialize(jobAndBusiness);

                var message = new ServiceBusMessage(jobAndBusinessAsJson)
                {
                    Subject = "GHJob" + " " + business.Id,
                    ContentType = "application/json",
                };

                await sender.SendMessageAsync(message); 
            }

            Console.WriteLine($"Sent {jobs.ToList().Count} jobs. - Time: {watch.ElapsedMilliseconds} m/s");
            Console.WriteLine($"That's {jobs.ToList().Count / watch.Elapsed.TotalSeconds} messages per second");
            Console.WriteLine();

            await sender.CloseAsync();

        }
    }
}    
