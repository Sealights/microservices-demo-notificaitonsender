using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace NotificaitonSender.NotificationService
{
    public class NotificationService : BackgroundService
    {
        private IConfiguration _configuration;

        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                AWSConfigs awsConfigs = new AWSConfigs()
                {
                    AwsRegion = _configuration["AWS_REGION"],
                    AwsSqsQueueUrl = _configuration["AWS_SQS_QUEUE_URL"],
                    AwsUsAccessKeyId = _configuration["AWS_US_ACCESS_KEY_ID"],
                    AwsUsSecretAccessKey = _configuration["AWS_US_SECRET_ACCESS_KEY"],
                    SqsWaitTimeSec = !string.IsNullOrEmpty(_configuration["SQS_WAIT_TIME_SEC"]) ? Int32.Parse(_configuration["SQS_WAIT_TIME_SEC"]) : 2,
                    SqsMaxMessages = !string.IsNullOrEmpty(_configuration["SQS_MAX_MESSAGES"]) ? Int32.Parse(_configuration["SQS_MAX_MESSAGES"]) : 1
                };

                try
                {
                    awsConfigs.Validate();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Missing env variable: {e.Message}");
                    return;
                }

                BasicAWSCredentials awsCredentials = new BasicAWSCredentials(awsConfigs.AwsUsAccessKeyId, awsConfigs.AwsUsSecretAccessKey);

                AmazonSQSClient sqsClient;

                try
                {
                    sqsClient = new AmazonSQSClient(awsCredentials, Amazon.RegionEndpoint.GetBySystemName(awsConfigs.AwsRegion));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"AWS SQL client creating error: {e}");
                    return;
                }
                

                Console.WriteLine($"Reading messages from queue\n  {awsConfigs.AwsSqsQueueUrl}");

                try
                {
                    do
                    {
                        var msg = await GetMessage(sqsClient, awsConfigs.AwsSqsQueueUrl, awsConfigs.SqsMaxMessages, awsConfigs.SqsWaitTimeSec);
                        if (msg.Messages.Count != 0)
                        {
                            if (ProcessMessage(msg.Messages[0]))
                                await DeleteMessage(sqsClient, msg.Messages[0], awsConfigs.AwsSqsQueueUrl);
                        }
                    } while (true);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error during processing message: {e}");
                    return;
                }

            }
        }

        private async Task<ReceiveMessageResponse> GetMessage(IAmazonSQS sqsClient, string qUrl, int maxMessages, int waitTime = 0)
        {
            return await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = qUrl,
                MaxNumberOfMessages = maxMessages,
                WaitTimeSeconds = waitTime
                // (Could also request attributes, set visibility timeout, etc.)
            });
        }

        private bool ProcessMessage(Message message)
        {
            Console.WriteLine($"\nMessage body of {message.MessageId}:");
            Console.WriteLine($"{message.Body}");
            return true;
        }


        private async Task DeleteMessage(IAmazonSQS sqsClient, Message message, string qUrl)
        {
            Console.WriteLine($"\nDeleting message {message.MessageId} from queue...");
            await sqsClient.DeleteMessageAsync(qUrl, message.ReceiptHandle);
        }
    }}
