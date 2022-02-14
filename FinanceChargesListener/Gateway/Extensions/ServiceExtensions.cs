using Amazon.S3;
using FinanceChargesListener.Gateway.Services;
using FinanceChargesListener.Gateway.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceChargesListener.Gateway.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddAmazonS3(this IServiceCollection services,
           IConfiguration configuration)
        {
            services.AddScoped<IAwsS3FileService, AwsS3FileService>();

            services.AddAWSService<IAmazonS3>();
        }
    }
}
