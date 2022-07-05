using System;

namespace NotificaitonSender.NotificationService
{
    public class AWSConfigs
    {
        public string AwsRegion { get; set; }
        public string AwsUsSecretAccessKey { get; set; }
        public string AwsSqsQueueUrl { get; set; }
        public string AwsUsAccessKeyId { get; set; }
        public int SqsWaitTimeSec { get; set; }
        public int SqsMaxMessages { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(AwsRegion))
            {
                throw new Exception("AWS Region is empty");
            }
            if (string.IsNullOrEmpty(AwsUsSecretAccessKey))
            {
                throw new Exception("AWS Us Secret Access Key is empty");
            }
            if (string.IsNullOrEmpty(AwsSqsQueueUrl))
            {
                throw new Exception("AWS SQS QueueUrl is empty");
            }
            if (string.IsNullOrEmpty(AwsUsAccessKeyId))
            {
                throw new Exception("AWS Us Access Key Id is empty");
            }
        }
    }
}
