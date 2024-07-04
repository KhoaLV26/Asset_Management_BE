using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Constants
{
    public static class SortConstants
    {
        public static class Asset
        {
            public const string SORT_BY_ASSET_CODE = "assetcode";
            public const string SORT_BY_ASSET_NAME = "assetname";
            public const string SORT_BY_CATEGORY = "category";
            public const string SORT_BY_STATE = "state";
        }

        public static class Report
        {
            public const string SORT_BY_CATEGORY = "category";
            public const string SORT_BY_TOTAL = "total";
            public const string SORT_BY_ASSIGNED = "assigned";
            public const string SORT_BY_AVAILABLE = "available";
            public const string SORT_BY_NOT_AVAILABLE = "notavailable";
            public const string SORT_BY_WAITING_FOR_RECYCLING = "waitingforrecycling";
            public const string SORT_BY_RECYCLED = "recycled";
        }

        public static class Assignment
        {
            public const string SORT_BY_ASSET_CODE = "assetcode";
            public const string SORT_BY_ASSET_NAME = "assetname";
            public const string SORT_BY_ASSIGNED_TO = "assignedto";
            public const string SORT_BY_ASSIGNED_BY = "assignedby";
            public const string SORT_BY_ASSIGNED_DATE = "assigneddate";
            public const string SORT_BY_STATE = "state";
        }

        public static class RequestReturn
        {
            public const string SORT_BY_ASSET_CODE = "AssetCode";
            public const string SORT_BY_ASSET_NAME = "AssetName";
            public const string SORT_BY_REQUESTED_BY = "RequestedBy";
            public const string SORT_BY_ASSIGNED_DATE = "AssignedDate";
            public const string SORT_BY_ACCEPTED_BY = "AcceptedBy";
            public const string SORT_BY_RETURNED_DATE = "ReturnedDate";
            public const string SORT_BY_STATE = "State";
        }

        public static class User
        {
            public const string SORT_BY_STAFF_CODE = "StaffCode";
            public const string SORT_BY_JOINED_DATE = "JoinedDate";
            public const string SORT_BY_ROLE = "Role";
            public const string SORT_BY_USERNAME = "Username";
        }
    }
}
