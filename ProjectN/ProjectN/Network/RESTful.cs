using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace ProjectN
{
    class RESTful : NetworkBase
    {
        private string baseUri;

        public RESTful()
        {
            baseUri = "http://" + ServerAddr + ":" + ServerPort + "/";
        }

        public IRestResponse RESTfulSingleParmRequest(string Uri, Method transferMethod, string parmKey, string parmVal)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;

            var Request = new RestRequest(Uri, transferMethod);
            Request.AddParameter(parmKey, parmVal);

            IRestResponse<JsonDeserializer> rj = Client.Execute<JsonDeserializer>(Request);

            return rj;
        }

        public IRestResponse<_REST_StylesetHashInfo> RESTUploadUserLook(long lookId, long matchUserLookId, string filePath1, string filePath2, string filePath3, string MembershipId)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;
            MembershipId = "2011003539244269";
            var Request = new RestRequest("/api/userLook", Method.POST);
            Request.AddParameter("membershipId", MembershipId);
            Request.AddParameter("lookId", lookId);
            Request.AddParameter("matchUserLookId", matchUserLookId);
            Request.AddFile("front", filePath1);
            Request.AddFile("noface", filePath2);
            Request.AddFile("back", filePath3);

            IRestResponse<_REST_StylesetHashInfo> rj = Client.Execute<_REST_StylesetHashInfo>(Request);

            return rj;
        }

        public IRestResponse<_REST_ProductInfo> RESTgetProductById(long id)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;

            var Request = new RestRequest("/api/looks/{id}", Method.GET);

            Request.AddUrlSegment("id", id.ToString());

            IRestResponse<_REST_ProductInfo> p = Client.Execute<_REST_ProductInfo>(Request);

            return p;
        }

        public IRestResponse<_REST_ProductInfo> RESTgetProductByBarCode(string barcode)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;

            var Request = new RestRequest("/api/looksByBarcode/{barcode}", Method.GET);

            Request.AddUrlSegment("barcode", barcode);

            IRestResponse<_REST_ProductInfo> p = Client.Execute<_REST_ProductInfo>(Request);
            
            return p;
        }

        public IRestResponse<_REST_StyleSetInfo> RESTgetStyleSetById(long Id)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;

            var Request = new RestRequest("/api/looks/{id}/userLooks", Method.GET);

            Request.AddUrlSegment("id", Id.ToString());

            IRestResponse<_REST_StyleSetInfo> p = Client.Execute<_REST_StyleSetInfo>(Request);

            return p;
        }

        public IRestResponse<_REST_StyleSetListInfo> RESTgetStyleSetListById(long Id)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;

            var Request = new RestRequest("/api/looks/{id}/userLooks", Method.GET);

            Request.AddUrlSegment("id", Id.ToString());

            IRestResponse<_REST_StyleSetListInfo> p = Client.Execute<_REST_StyleSetListInfo>(Request);

            return p;
        }

        public IRestResponse RESTgetCupon(string stylesetId)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;

            var Request = new RestRequest("/api/looks/{id}/userLooks", Method.GET);

            Request.AddUrlSegment("id", stylesetId);

            IRestResponse<_REST_StyleSetInfo> p = Client.Execute<_REST_StyleSetInfo>(Request);

            return p;
        }

        public IRestResponse<_REST_MembershipInfo> RESTgetMember(long membershipId)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;

            var Request = new RestRequest("/api/user/{membershipId}", Method.GET);

            Request.AddUrlSegment("membershipId", membershipId.ToString());

            IRestResponse<_REST_MembershipInfo> p = Client.Execute<_REST_MembershipInfo>(Request);

            return p;
        }

        public IRestResponse<_REST_StyleSetListInfo> RESTgetStyleSetListByUserId(long userlookId)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;

            var Request = new RestRequest("/api/userLooksOfUserByUserLookId/{userLookId}", Method.GET);

            Request.AddUrlSegment("userLookId", userlookId.ToString());

            IRestResponse<_REST_StyleSetListInfo> p = Client.Execute<_REST_StyleSetListInfo>(Request);
            
            if (p.Data[0].id == 0)
                p.Data = null;

            return p;
            
        }

        public IRestResponse RESTsetLikeStyleset(long userlookId)
        {
            var Client = new RestClient();
            Client.BaseUrl = baseUri;

            var Request = new RestRequest("/api/like/{userLookId}", Method.POST);

            Request.AddUrlSegment("userLookId", userlookId.ToString());

            IRestResponse p = Client.Execute(Request);

            return p;

        }
    }
}
