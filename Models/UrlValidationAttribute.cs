using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace PageScraper.Models
{
     [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UrlValidationAttribute : ValidationAttribute
    {
        private readonly UrlValidationType _validationType;
        private readonly string _defaultErrorMessage;

        private object _typeId = new object();
        public override object TypeId
        {
            get { return _typeId; }
        }

        public UrlValidationAttribute(UrlValidationType validationType, string message)
        {
            _validationType = validationType;
            switch (validationType)
            {
                case UrlValidationType.AbsoluteUrlValidation:
                {
                    _defaultErrorMessage = message;
                    break;
                }
                case UrlValidationType.SiteContentValidation:
                {
                    _defaultErrorMessage = message;
                    break;
                }
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            switch (_validationType)
            {
                case UrlValidationType.AbsoluteUrlValidation:
                {
                    if (value != null)
                    {
                        //parse value from originalUrl
                        var utsUrl = value.ToString();
                        
                        if (!IsUrlGood(utsUrl))
                        {
                            //setup return message, message defined in SelectionModel
                            string message = string.Format(_defaultErrorMessage, utsUrl);
                            return new ValidationResult(message);
                        }
                    }

                    break;
                }

                case UrlValidationType.SiteContentValidation:
                {
                    if (value != null)
                    {
                        //parse value from originalUrl
                        var utsUrl = value.ToString();

                        if (!RemoteSitehasContent(utsUrl))
                        {
                            //setup return message, message defined in SelectionModel
                            string message = string.Format(_defaultErrorMessage, utsUrl);
                            return new ValidationResult(message);
                        }
                    }

                    break;
                }
            }

            return ValidationResult.Success;
        }

        public static bool IsUrlGood(string str)
        {
            //check if absolute
            if (str.StartsWith("http://") || str.StartsWith("https://"))
            {
                return true;
            }

            return false;
        }

        private static bool RemoteSitehasContent(string url)
        {
            try
            {
                var htmldocument = new HtmlDocument();
               
                htmldocument.LoadHtml(new WebClient().DownloadString(url));
                var root = htmldocument.DocumentNode;

                return root.Descendants().Any();
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }



    }
    
    public enum UrlValidationType
    {
        AbsoluteUrlValidation,
        SiteContentValidation
    }
}