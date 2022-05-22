using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MongoDB.Driver;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Category.Functions.Models;
using Category.Functions.Entities;
using MongoDB.Driver.Linq;
using System.Collections.Generic;

namespace Category.Functions.Functions
{
    public class CategoryFunction
    {

        private readonly ILogger<CategoryFunction> _logger;
        private readonly IMongoDatabase _db;

        public CategoryFunction(ILogger<CategoryFunction> logger)
        {
            var mongoClient = new MongoClient(System.Environment.GetEnvironmentVariable("MongoDBAtlasConnectionString"));
            _db = mongoClient.GetDatabase(System.Environment.GetEnvironmentVariable("MongoDBDatabaseName"));
            _logger = logger;
        }

        [FunctionName("CreateCategory")]
        [OpenApiOperation(operationId: "CreateCategory", tags: new[] { "Categories" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateCategoryDto))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CategoryTable), Description = "The OK response")]
        public async Task<IActionResult> CreateCategory(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "categories")] CreateCategoryDto request,
            ILogger log)
        {
            try
            {
                IMongoCollection<CategoryTable> category = _db.GetCollection<CategoryTable>("categories");

                var categoryObj = new CategoryTable
                {
                    Name = request.Name,
                    Description = request.Description
                };

                await category.InsertOneAsync(categoryObj);

                return new OkObjectResult(categoryObj);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error fetching data" + ex.Message);
            }
        }

        [FunctionName("AllCategories")]
        [OpenApiOperation(operationId: "AllCategories", tags: new[] { "Categories" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<CategoryVm>), Description = "The OK response")]
        public async Task<IActionResult> AllCompanies(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "categries")] HttpRequest req)
        {
            try
            {
                IMongoCollection<CategoryTable> category = _db.GetCollection<CategoryTable>("categories");

                var categories = await category.AsQueryable<CategoryTable>().Select(c => new
                {
                    c.Id,
                    c.Name
                }).ToListAsync();

                return new OkObjectResult(categories);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error fetching data" + ex.Message);
            }
        }
    }
}
