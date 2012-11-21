using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectN
{
    class RESTfulStructure
    {

    }

    public class _REST_ProductInfo
    {
        public long id { get; set; }
        public string name { get; set; }
        public int shotCount { get; set; }
        public int year { get; set; }
        public int season { get; set; }
        public int lookType { get; set; }
        public int price { get; set; }
        public string Description { get; set; }
        public string imageFileName { get; set; }
        public string barCode { get; set; }
        public string imageUrl { get; set; }
    }

    public class _REST_StyleSetInfo
    {
        public long id { get; set; }
        public _REST_ProductInfo look { get; set; }
        public int size { get; set; }
        public int likeCount { get; set; }
        public string imageFileName { get; set; }
        public string date { get; set; }
        public long matchUserLookId { get; set; }
        public string imageUrl { get; set; }
    }

    public class _REST_StyleSetListInfo : List<_REST_StyleSetInfo> {}

    public class _REST_CuponInfo
    {
        public long id { get; set; }
        public string userLookHash { get; set; }
        public string price { get; set; }
        public string expireDate { get; set; }
        public string used { get; set; }
    }

    public class _REST_StylesetHashInfo
    {
        public string hash { get; set; }
    }

    public class _REST_MembershipInfo
    {
        public string membershipId { get; set; }
        public string name { get; set; }
    }
}
