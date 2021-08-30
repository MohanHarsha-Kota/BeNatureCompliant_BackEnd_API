using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NCAppWebApi.Models;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;

namespace NCAppWebApi.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        private static SecretClientOptions options;
        private static SecretClient client;
        private static KeyVaultSecret secret;
        private static string secretValue;
        private static string encryptKey;
        private static string endpointuri;
        private static CosmosClient cosmosClient;
        private static Database Ncdatabase;

        public ValuesController()
        {
            options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };

            client = new SecretClient(new Uri("https://nc-poc-keyvault.vault.azure.net/"), new DefaultAzureCredential(), options);
            secret = client.GetSecret("nc-cosmos-access-key");
            secretValue = secret.Value;
            secret = client.GetSecret("nc-cosmos-encrypt-key");
            encryptKey = secret.Value;
            endpointuri = "https://nc-poc-cosmosdb.documents.azure.com:443/";
            cosmosClient = new CosmosClient(endpointuri, secretValue);
            Ncdatabase = cosmosClient.GetDatabase("NCMasterDB");
        }

        /* To view all the products available for recycling
         * Last Modified: 24-06-2021
         */
        [Route("api/getproducts")]
        [HttpGet]
        public async Task<List<ProductModel>> GetProducts()
        {
            Container NcProductsCon = Ncdatabase.GetContainer("Products");
            var sqlQueryText = "SELECT * FROM Products";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<ProductModel> queryResultSetIterator = NcProductsCon.GetItemQueryIterator<ProductModel>(queryDefinition);

            List<ProductModel> ProductList = new List<ProductModel>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<ProductModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (ProductModel prObj in currentResultSet)
                {
                    ProductList.Add(prObj);
                }
            }

            return ProductList;
        }

        /*Enables the Compliance Engineer to Confirm the pickup
        * Last Modified: 22-06-2021
        */
        [Route("api/getscore")]
        [HttpPost]
        public async Task<int> EcoScore([FromBody] string emailId)
        {
            Container NCServiceHistoryCon = Ncdatabase.GetContainer("ServiceHistory");
            string sqlQueryText = string.Format("SELECT SUM(ServiceHistory.eco_score) AS score, ServiceHistory.customer_email" +
                " FROM ServiceHistory where ServiceHistory.customer_email='{0}' group by ServiceHistory.customer_email", emailId);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<ScoreModel> queryResultSetIterator = NCServiceHistoryCon.GetItemQueryIterator<ScoreModel>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<ScoreModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (ScoreModel p in currentResultSet)
                {
                    return p.Score;   
                }
            }
            //If we do not get any response, return a default value
            return 0;
        }

        /*Enables the Compliance Engineer to Confirm the pickup
         * Last Modified: 22-06-2021
         */
        [Route("api/confirmpickup")]
        [HttpPost]
        public async Task<bool> ConfirmPickup([FromBody] ServicedOrderModel somObject)
        {
            Container NcServiceHistoryCon = Ncdatabase.GetContainer("ServiceHistory");
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<ServicedOrderModel> serviceOrderResponse = await NcServiceHistoryCon.ReadItemAsync<ServicedOrderModel>(somObject.ServicePickupId, new PartitionKey(somObject.CustomerEmailId));
                //return "Item in database with id: " + serviceOrderResponse.Resource.ServicePickupId + "already exists";
                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                //If the item does not exist, we will insert it into the container
                ItemResponse<ServicedOrderModel> serviceOrderResponse = await NcServiceHistoryCon.CreateItemAsync<ServicedOrderModel>(somObject, new PartitionKey(somObject.CustomerEmailId));
                return true;
            }
        }


        /*Enables the customer to view all the of orders scheduled by them till date in descending order with latest scheduled pickup on top.
         * Last Modified: 24-08-2021
         */
        [Route("api/schedulehistory")]
        [HttpPost]
        public async Task<List<ScheduleHistoryModel>> ScheduleHistory([FromBody] string emailId)
        {
            Container NcScheduleHistoryCon = Ncdatabase.GetContainer("ScheduleHistory");
            string sqlQueryText = string.Format("SELECT * FROM ScheduleHistory where ScheduleHistory.email = '{0}' order by ScheduleHistory._ts desc", emailId);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<ScheduleHistoryModel> queryResultSetIterator = NcScheduleHistoryCon.GetItemQueryIterator<ScheduleHistoryModel>(queryDefinition);

            List<ScheduleHistoryModel> ScheduleOrderList = new List<ScheduleHistoryModel>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<ScheduleHistoryModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (ScheduleHistoryModel pscObjc in currentResultSet)
                {
                    ScheduleOrderList.Add(pscObjc);
                }
            }
            //returning the list of pickup orders scheduled by the corresponding customer
            return ScheduleOrderList;
        }

        /*Enables the Compliance Engineer to view all the active orders to be serviced
         * Last Modified: 22-06-21
         */
        [Route("api/activeorders")]
        [HttpGet]
        public async Task<List<ScheduleHistoryModel>> ActiveOrders()
        {
            string today_date = DateTime.Now.ToString("dd-MM-yyyy");
            Container NcScheduleHistory = Ncdatabase.GetContainer("ScheduleHistory");

            string sqlQueryText = string.Format("SELECT * FROM ScheduleHistory where ScheduleHistory.scheduled_date = '{0}' AND ScheduleHistory.status='Active'", today_date);
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<ScheduleHistoryModel> queryResultSetIterator = NcScheduleHistory.GetItemQueryIterator<ScheduleHistoryModel>(queryDefinition);

            List<ScheduleHistoryModel> ActiveOrders = new List<ScheduleHistoryModel>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<ScheduleHistoryModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (ScheduleHistoryModel actObjc in currentResultSet)
                {
                    ActiveOrders.Add(actObjc);
                }
            }

            return ActiveOrders;
        }

        /*Enables the customer to schedule a pickup
         *Last Modified: 22-06-2021 
         */
        [Route("api/schedule")]
        [HttpPost]
        public async Task<string> Schedule([FromBody] ScheduleHistoryModel schObject)
        {
            Container NcScheduleHistCon = Ncdatabase.GetContainer("ScheduleHistory");

            try
            {
                // Checking if the item already exists.  
                ItemResponse<ScheduleHistoryModel> ScheduleOrderResponse = await NcScheduleHistCon.ReadItemAsync<ScheduleHistoryModel>(schObject.OrderScheduleId, new PartitionKey(schObject.EmailId));
                return "A slot with Order Schedule Id: " + ScheduleOrderResponse.Resource.OrderScheduleId + "already exists. Please Contact customer care";
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                //Creating the order document in the container and returning the order ID back to the application.
                ItemResponse<ScheduleHistoryModel> ScheduleOrderResponse = await NcScheduleHistCon.CreateItemAsync<ScheduleHistoryModel>(schObject, new PartitionKey(schObject.EmailId));
                return ScheduleOrderResponse.Resource.OrderScheduleId;
            }
        }

        /*Enables the customer to LogIn to the application
         *Last Modified: 30-08-2021 
         */
        [Route("api/login")]
        [HttpPost]
        public async Task<LoginUserModel> Login([FromBody] LoginModel lObject)
        {
           
            Container NcUsersCon = Ncdatabase.GetContainer("Users");

            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(lObject.Password);
            //AesCryptoServiceProvider to make sure we store the data in encrypted form in addition to encryption at rest provided by Microsoft
            AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
            aesProvider.Key = UTF8Encoding.UTF8.GetBytes(encryptKey);
            aesProvider.Mode = CipherMode.ECB;
            aesProvider.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = aesProvider.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            aesProvider.Clear();
            string secPass = Convert.ToBase64String(resultArray, 0, resultArray.Length);

            string sqlQueryText = string.Format("SELECT * FROM Users where Users.id='{0}' AND Users.password='{1}'", lObject.Username, secPass);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<UserModel> queryResultSetIterator = NcUsersCon.GetItemQueryIterator<UserModel>(queryDefinition);

            LoginUserModel resObj = new LoginUserModel();

            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<UserModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                
                if(currentResultSet.Count != 0)
                {
                    UserModel usrObj = currentResultSet.First();
                    resObj.EmailId = usrObj.EmailId;
                    resObj.Address = usrObj.Addresses;
                    resObj.Id = usrObj.Id;
                    resObj.RoleId = usrObj.RoleId;
                    resObj.FirstName = usrObj.FirstName;
                    return resObj;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /*Enables the customer to register
         *Last Modified: 30-08-2021 
         */
        [Route("api/register")]
        [HttpPost]
        public async Task<string> Register([FromBody] UserModel uObject)
        {
            Container NcUsersCon = Ncdatabase.GetContainer("Users");
            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(uObject.Password);
            //AesCryptoServiceProvider to make sure we store the data in encrypted form in addition to encryption at rest provided by Microsoft
            AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
            aesProvider.Key = UTF8Encoding.UTF8.GetBytes(encryptKey);
            aesProvider.Mode = CipherMode.ECB;
            aesProvider.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = aesProvider.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            aesProvider.Clear();
            string secPass = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            uObject.Password = secPass;
            try
            {
                // Read the customer already registered
                ItemResponse<UserModel> RegisterObjResponse = await NcUsersCon.ReadItemAsync<UserModel>(uObject.Id, new PartitionKey(uObject.RoleId));
                //return "Item in database with id: "+ RegisterObjResponse.Resource.Id+ "already exists";
                return "Error";
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                ItemResponse<UserModel> RegisterObjResponse = await NcUsersCon.CreateItemAsync<UserModel>(uObject, new PartitionKey(uObject.RoleId));
                //return "Created item in database with id: " + RegisterObjResponse.Resource.Id;
                return RegisterObjResponse.Resource.Id;
            }
        }
    }
}
