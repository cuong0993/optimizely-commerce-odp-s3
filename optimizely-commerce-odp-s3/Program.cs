using System.Runtime.CompilerServices;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using EPiServer.Commerce.ODP;

var fileName = "optimizely-commerce-odp-s3.deps.json";
var marketKey = new MarketKey();
marketKey.TrackingId = "";
var s3Options = new S3Options();
s3Options.BucketName = "zaius-incoming.eu1";
s3Options.AccessKeyId = "";
s3Options.SecretAccessKey = "";
s3Options.Region = "eu-west-1";
marketKey.S3Options = s3Options;


var trackingId = marketKey.TrackingId;
var bucketName = marketKey.S3Options.BucketName;
var accessKeyId = marketKey.S3Options.AccessKeyId;
var secretAccessKey = marketKey.S3Options.SecretAccessKey;
var systemName = string.IsNullOrWhiteSpace(marketKey.S3Options.Region)
    ? "us-east-1"
    : marketKey.S3Options.Region;
var num = 3;
var amazonS3Config1 = new AmazonS3Config();
amazonS3Config1.RegionEndpoint = RegionEndpoint.GetBySystemName(systemName);
var awsSecretAccessKey = secretAccessKey;
using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
{
    using (var s3Client = new AmazonS3Client(accessKeyId, awsSecretAccessKey, amazonS3Config1))
    {
        using (var transferUtility = new TransferUtility(s3Client))
        {
            var request = new TransferUtilityUploadRequest
            {
                BucketName = bucketName,
                Key = trackingId + "/" + fileName,
                InputStream = fs,
                AutoCloseStream = false
            };
            request.UploadProgressEvent += UploadRequest_UploadProgressEvent;
            while (num > 0)
                try
                {
                    transferUtility.UploadAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
                    var interpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 3);
                    interpolatedStringHandler.AppendLiteral("File successfully uploaded to S3 bucket: ");
                    interpolatedStringHandler.AppendFormatted(bucketName);
                    interpolatedStringHandler.AppendLiteral("/");
                    interpolatedStringHandler.AppendFormatted(trackingId);
                    interpolatedStringHandler.AppendLiteral("/");
                    interpolatedStringHandler.AppendFormatted(fileName);
                    var stringAndClear = interpolatedStringHandler.ToStringAndClear();
                    var objArray = Array.Empty<object>();
                    break;
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case AmazonS3Exception _:
                        case HttpRequestException _:
                            throw;
                        default:
                            --num;
                            continue;
                    }
                }
        }
    }
}

void UploadRequest_UploadProgressEvent(object sender, UploadProgressArgs e)
{
    var interpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
    interpolatedStringHandler.AppendLiteral("Sending file... ");
    interpolatedStringHandler.AppendFormatted(e.PercentDone);
    interpolatedStringHandler.AppendLiteral("%");
    var stringAndClear = interpolatedStringHandler.ToStringAndClear();
}