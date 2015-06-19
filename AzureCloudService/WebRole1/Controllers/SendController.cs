using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace WebRole1
{
    public class SendController : ApiController
    {
        private QueueClient client;

        public SendController()
        {
            var connectionString = "Endpoint=sb://notificationbas.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=du41aFGjHMQTu3Vjn+TOHzHNfyvYFTCbn63BJ8kmRVs=";
            var queueName = "notificationqueue";
            this.client = QueueClient.CreateFromConnectionString(connectionString, queueName);
        }

        public async Task<string> Get()
        {
            var message = new BrokeredMessage("Gooner");
            message.MessageId = Guid.NewGuid().ToString();
            await this.client.SendAsync(message);
            return "OK";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.client.Close();
            }
            base.Dispose(disposing);
        }
    }
}