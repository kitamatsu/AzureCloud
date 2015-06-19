using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            //キューの名称を設定
            string queueName = "nortificationqueue";

            //キューの最大サイズ(1GB)とタイムアウト時間(1時間)を設定
            QueueDescription qd = new QueueDescription(queueName);
            qd.MaxSizeInMegabytes = 1024;
            qd.DefaultMessageTimeToLive = new TimeSpan(1, 0, 0);

            //App.Configからサービスバスへの接続情報を読み込む
            string connectionString =
                CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            //サービスバスを管理するためのNamespaceManagerを作成する
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            //キューがなければ作成する
            if (!namespaceManager.QueueExists(queueName))
            {
                namespaceManager.CreateQueue(qd);
            }

            //キューのクライアントを取得
            QueueClient client = QueueClient.CreateFromConnectionString(connectionString, queueName);
            //送信するメッセージの本体を生成
            BrokeredMessage message = new BrokeredMessage("Test Message.");
            //メッセージを送信
            client.Send(message);
            Console.WriteLine("Message(s) sent.");
            Console.WriteLine("Done, press a key contirue.");
            Console.ReadKey();
        }
    }
}
