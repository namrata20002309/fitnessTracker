using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Business.Services.Interfaces;
using UserService.Data.Entities;

namespace UserService.Business.Services
{
    public class UserActionQueueService : IUserActionQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly ILogger<UserActionQueueService> _logger;

        public UserActionQueueService(IConfiguration configuration, ILogger<UserActionQueueService> logger)
        {
            string connectionString = configuration["AzureQueueSettings:ConnectionString"];
            string queueName = configuration["AzureQueueSettings:UserActionsQueueName"];

            _queueClient = new QueueClient(connectionString, queueName);
            _logger = logger;
        }

        public async Task EnsureQueueExistsAsync()
        {
            try
            {
                await _queueClient.CreateIfNotExistsAsync();
                _logger.LogInformation($"Queue {_queueClient.Name} is ready");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ensuring queue exists: {ex.Message}");
                throw;
            }
        }

        public async Task SendMessageAsync(UserActionMessage message)
        { 
            try
            {
                // Make sure the queue exists
                await EnsureQueueExistsAsync();

                // Serialize the message to JSON
                string jsonMessage = JsonSerializer.Serialize(message);

                // Base64 encode the message content
                string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonMessage));

                // Send the message to the queue
                await _queueClient.SendMessageAsync(base64Message);

                _logger.LogInformation($"Sent {message.Action} message for user {message.UserId} to queue");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message to queue: {ex.Message}");
                throw;
            }
        }

    }

}



