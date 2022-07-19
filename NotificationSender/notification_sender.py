import os

from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import (BatchSpanProcessor)
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter

import boto3
from opentelemetry.instrumentation.boto3sqs import Boto3SQSInstrumentor

Boto3SQSInstrumentor().instrument()

import init_tracing
from logger import getJSONLogger
logger = getJSONLogger('notificationsender')


init_tracing.init_tracer_provider()

tracer_provider = TracerProvider()
trace.set_tracer_provider(tracer_provider)
tracer_provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter()))

region_name = os.environ.get('AWS_REGION', "us-east-2")
aws_access_key_id = os.environ.get('AWS_US_ACCESS_KEY_ID', "")
aws_secret_access_key = os.environ.get('AWS_US_SECRET_ACCESS_KEY', "")
endpoint_url = os.environ.get('AWS_SQS_QUEUE_URL', "") 
sqs_wait_time_sec = os.environ.get('SQS_WAIT_TIME_SEC', "20") 
sqs_max_messages = os.environ.get('SQS_MAX_MESSAGES', "5") 
endpoint_notification_url = os.environ.get('AWS_NOTIFICATION_SQS_QUEUE_URL', "") 

sqs = boto3.client('sqs',
      region_name=region_name,
      aws_access_key_id=aws_access_key_id,
      aws_secret_access_key=aws_secret_access_key)  

def delete_message(message):
  receipt_handle = message['ReceiptHandle'] 
  sqs.delete_message(
      QueueUrl=endpoint_url,
      ReceiptHandle=receipt_handle
  )

def receive_message():
  return sqs.receive_message(
      QueueUrl=endpoint_url,
      MaxNumberOfMessages=int(sqs_max_messages),
      AttributeNames=[
        'SentTimestamp'
      ],
      VisibilityTimeout=1,
      WaitTimeSeconds=int(sqs_wait_time_sec),
      MessageAttributeNames=[
        'All'
      ],
  )

def send_notification_message(message):
  if endpoint_notification_url == "":
    return

  return sqs.send_message(
        QueueUrl=endpoint_notification_url,
        DelaySeconds=10,
        MessageAttributes={
            'ReceiptHandle': {
                'DataType': 'String',
                'StringValue': message['ReceiptHandle'] 
            },         
        },
        MessageBody=message['Body'] 
    )  

while True:
  response = receive_message()

  if "Messages" in response:   
    for message in response["Messages"]:
        logger.info(f"Recived message: {message}")
        try:
          response = send_notification_message(message)
          logger.info('SQS send message, response {} .'.format(response['MessageId']))
        except Exception as e: 
          logger.error('Error during sending message to notification sqs {} .'.format(e))
        delete_message(message) 