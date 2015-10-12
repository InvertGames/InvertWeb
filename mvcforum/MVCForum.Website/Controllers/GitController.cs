using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Octokit;

namespace MVCForum.Website.Controllers
{
    public class GitController : Controller
    {
        // GET: Git
        public ActionResult Index()
        {
   
            
            return View();
        }
    }
}