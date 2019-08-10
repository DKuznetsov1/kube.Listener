﻿using Kube.Infrastructure.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kube.Infrastructure.RabbitMQAgent
{
    public class RabbitMQAgent : IMQAgent
    {
        private readonly string queue = "kube.audit.queue";
        private readonly string exchangeName = "kube.audit.exchange";

        public void Subscribe()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Port = 5674 };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeName, type: "topic");

                var queueName = channel.QueueDeclare(this.queue, true, false, false, null).QueueName;
                channel.QueueBind(queue: queueName, exchange: this.exchangeName, routingKey: "*");

                Console.WriteLine(" [*] Waiting for logs.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] {0}", message);
                };
                channel.BasicConsume(queue: "kube.audit.queue", autoAck: true, consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
