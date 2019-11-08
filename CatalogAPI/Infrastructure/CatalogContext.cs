using CatalogAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace CatalogAPI.Infrastructure
{
    public class CatalogContext
    {
        private IConfiguration configuration;
        private IMongoDatabase mongoDatabase; 
        public CatalogContext(IConfiguration configuration)
        {
            this.configuration = configuration;
            var conStr = configuration.GetValue<string>("MongoSettings:ConnectionStrings");

            //MongoClientSettings settings  =  MongoClientSettings.FromConnectionString(conStr);
   
            MongoClientSettings settings = MongoClientSettings.FromUrl(
              new MongoUrl(conStr)
            );
            settings.SslSettings =
              new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
          
            MongoClient mongoClient = new MongoClient(settings);

            if(mongoClient != null)
            {
                this.mongoDatabase = mongoClient.GetDatabase(configuration.GetValue<string>("MongoSettings:Database"));
            }
                            //data
        }

        public IMongoCollection<CatalogItem> Catalog
        {
            get
            {
                return this.mongoDatabase.GetCollection<CatalogItem>("products");
            }

        }
    }
}
