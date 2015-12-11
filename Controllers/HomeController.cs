using System.Web.Mvc;
using PageScraper.Models;
using PageScraper.Utility;

namespace PageScraper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var rom = new OperationsModel.IndexViewModel();
            var spm = (OperationsModel.IndexViewModel)TempData["spModel"];

            if (spm != null)
            {
                return View(spm);
            }

            return View(rom);
        }

        /// <summary>
        /// This method adds a redirect 
        /// </summary>
        /// <param name="ivm"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult _UrlToSubmit(OperationsModel.IndexViewModel ivm, FormCollection form)
        {

            if (!ModelState.IsValid)
            {
                return View("Index", ivm);
            }

            var ivmreturn = new OperationsModel.IndexViewModel();
            
            var url = ivm.Utsm.UrlToSubmit;
            try
            {
                ivmreturn.Lstimages = Scraper.ImageFinder.Find(url);
                ivmreturn.LstText = Scraper.TextFinder.Find(url);
            }
            catch
            {
                return RedirectToAction("Index", "Home", null);
            }
           

            // Display results to a webpage
            TempData["spModel"] = ivmreturn;

            return RedirectToAction("Index", "Home", null);
        }
               


      
    }


}