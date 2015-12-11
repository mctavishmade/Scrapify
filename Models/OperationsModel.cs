using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PageScraper.Models
{
    public class OperationsModel
    {
        public class IndexViewModel
        {
            public UrlToSubmitModel Utsm { get; set; }

            public IEnumerable<GalleryItem> Lstimages { get; set; }

            public IEnumerable<TextItem> LstText { get; set; }
        }
        
        public class UrlToSubmitModel
        {
            [Required(ErrorMessage = "Required Field")]
            [UrlValidation(UrlValidationType.AbsoluteUrlValidation, "Absolute Url's only")]
           [UrlValidation(UrlValidationType.SiteContentValidation, "Site has no content to scrape")]
            public virtual string UrlToSubmit { get; set; }

        }

        public class GalleryItem
        {
            public string RequestDomain { get; set; }
            public string ImagePath { get; set; }
            public string ImageDesc { get; set; }
        }

        public class TextItem
        {
            public string Scrapedtext { get; set; }
            public string Scrubbedtext { get; set; }
            public int OccurenceCount { get; set; }
            
        }


       
    }
}