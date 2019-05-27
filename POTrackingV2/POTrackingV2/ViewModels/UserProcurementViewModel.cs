using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using POTrackingV2.Models;

namespace POTrackingV2.ViewModels
{
    public class UserProcurementViewModelCreate
    {
        public SelectList SuperiorUsernames { get; set; }
        public SelectList InferiorUsernames { get; set; }
    }

    public class UserProcurementViewModelDetails
    {
        public UserProcurementSuperior UserProcurementSuperior { get; set; }
        public List<UserProcurementSuperior> UserProcurementInferiors { get; set; }
    }
}