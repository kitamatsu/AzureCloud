using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.ServiceBus.Messaging;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private ManualResetEvent completedEvent = new ManualResetEvent(false);
        private QueueClient client;

        public override void Run()
        {
            var options = new OnMessageOptions();
            options.AutoComplete = true;        // 自分でCompleteを呼ばない
            options.MaxConcurrentCalls = 10;    // 同時に処理するメッセージ数
            options.ExceptionReceived += options_ExceptionReceived;

            this.client.OnMessageAsync(
                 async (msg) =>
                 {
                     // 繰り返し処理しても完了できなかったら、メッセージを削除
                     if (msg.DeliveryCount > 10)
                     {
                         await msg.DeadLetterAsync();
                         Trace.TraceWarning("Maximum Message Count Exceeded: {0} for MessageID: {1} ", 10, msg.MessageId);
                         return;
                     }

                     // メッセージを受信して、何かしらの処理を実行
                     await Task.Delay(TimeSpan.FromSeconds(2));
                     Trace.TraceInformation("Recived Message ID : {0} Body : {1}", msg.MessageId, msg.GetBody<string>());
                 },
                 options);

            this.completedEvent.WaitOne();

        }

        void options_ExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            var exceptionMessage = "null";
            if (e != null && e.Exception != null)
            {
                exceptionMessage = e.Exception.Message;
                Trace.TraceError("Exception in QueueClient.ExceptionReceived: {0}", exceptionMessage);
            }
        }


        public override bool OnStart()
        {
            // 同時接続の最大数を設定します
            ServicePointManager.DefaultConnectionLimit = 12;

            var connectionString = CloudConfigurationManager.GetSetting("ServiceBusConnectionString");
            var queueName = CloudConfigurationManager.GetSetting("QueueName");
            this.client = QueueClient.CreateFromConnectionString(connectionString, queueName);

            return base.OnStart();

        }

        public override void OnStop()
        {
            this.client.Close();
            this.completedEvent.Set();
            base.OnStop();

        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: 次のロジックを自分で作成したロジックに置き換えてください。
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
