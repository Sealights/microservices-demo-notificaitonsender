using NotificaitonSender.NotificationService;

namespace NotificationSenderTest
{
    public class NotificationSenderTest
    {
        [Fact]
        public void AWSConfigs_Validate_MissingValue_ErrorReturned()
        {
            AWSConfigs awsConfigs = new AWSConfigs() { 
                AwsRegion = "",
                AwsSqsQueueUrl = "AwsSqsQueueUrl",
                AwsUsAccessKeyId = "AwsUsAccessKeyId",
                AwsUsSecretAccessKey = "AwsUsSecretAccessKey",
            };

            Action act = () => awsConfigs.Validate();

            Exception exception = Assert.Throws<Exception>(act);

            Assert.Equal("AWS Region is empty", exception.Message);
        }
    }
}