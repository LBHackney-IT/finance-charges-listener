using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using FinanceChargesListener.Domain;
using FinanceChargesListener.Factories;
using FinanceChargesListener.Gateway.Interfaces;
using FinanceChargesListener.Infrastructure;
using FinanceChargesListener.Infrastructure.Entities;
using Hackney.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway
{
    //public class ChargesDbGateway : IChargesGateway
    //{
    //    private readonly IAmazonDynamoDB _amazonDynamoDb;
    //    private readonly IDynamoDBContext _dynamoDbContext;

    //    public ChargesDbGateway(IAmazonDynamoDB amazonDynamoDb,
    //        IDynamoDBContext dynamoDbContext)
    //    {
    //        _amazonDynamoDb = amazonDynamoDb;
    //        _dynamoDbContext = dynamoDbContext;
    //    }


    //}
}
