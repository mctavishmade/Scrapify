using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using PageScraper.Models;

namespace PageScraper.Utility
{
    public class Scraper
    {
        public class ImageFinder
        {
            public static List<OperationsModel.GalleryItem> Find(string url)
            {
                var list = new List<OperationsModel.GalleryItem>();

                Uri myUri = new Uri(url);
                string host = myUri.Host;

                var htmldocument = new HtmlDocument();
                var htmlwebobj = new HtmlWeb();
                htmldocument = htmlwebobj.Load(url);
                
                //find meta img refs
                IEnumerable<HtmlNode> metalinks = htmldocument.DocumentNode.Descendants("meta").Where(n => n.GetAttributeValue("property", "").Equals("og:image"));

                foreach (var link in metalinks)
                {
                    var scrapedmetaimg = new OperationsModel.GalleryItem {RequestDomain = host, ImageDesc = "meta"};
                    var iPath = link.GetAttributeValue("content", "");
                    scrapedmetaimg.ImagePath = CheckImagePath(iPath, host);
                   
                    list.Add(scrapedmetaimg);
                }

                //find img refs
                IEnumerable<HtmlNode> imglinks = htmldocument.DocumentNode.Descendants("img").Where(n => n.Attributes.Contains("src"));

                foreach (var link in imglinks)
                {
                    var scrapedimgimg = new OperationsModel.GalleryItem { RequestDomain = host, ImageDesc = "img" };
                    var iPath = link.GetAttributeValue("src", "");
                    scrapedimgimg.ImagePath = CheckImagePath(iPath, host);

                    list.Add(scrapedimgimg);
                   
                }

                //find link img refs
                IEnumerable<HtmlNode> linklinks = htmldocument.DocumentNode.Descendants("link").Where(n => n.GetAttributeValue("rel", "").Equals("image_src"));

                foreach (var link in linklinks)
                {
                    var scrapedimgimg = new OperationsModel.GalleryItem { RequestDomain = host, ImageDesc = "link" };
                    var iPath = link.GetAttributeValue("href", "");
                    scrapedimgimg.ImagePath = CheckImagePath(iPath, host);

                    list.Add(scrapedimgimg);
               
                }
               
                return list;
            }
        }

        public static string CheckImagePath(string path, string rhost)
        {
            var strreturn = "";
           
            if (path.Trim().StartsWith("http://") || path.Trim().StartsWith("https://") || path.Trim().StartsWith("data:image"))
            {
                strreturn = path;
                return strreturn;
            }
            
            if (path.Trim().StartsWith("//"))
            {
                strreturn = "http:" + path;
                return strreturn;
            }


            if (path.Trim().StartsWith("/~") || path.Trim().StartsWith("/"))
            {
                //for a couple sitecore sites 
                strreturn = "http://" + rhost + path;
                return strreturn;
            }

            strreturn = "~/Content/images/ina.png";
            return strreturn;

        }

        public class TextFinder
        {
            public static string ScrapedWordBlock { get; set; }

            public static string ScrubbedWordBlock { get; set; }

            public static List<OperationsModel.TextItem> Find(string url)
            {
                var list = new List<OperationsModel.TextItem>();
                var sb = new StringBuilder();

                var htmldocument = new HtmlDocument();
                var htmlwebobj = new HtmlWeb();

                try
                {
                    htmldocument = htmlwebobj.Load(url);

                    //execute primary filter sans script and style tags
                    IEnumerable<HtmlNode> textndes = htmldocument.DocumentNode.DescendantsAndSelf().Where(n =>
                        n.NodeType == HtmlNodeType.Text &&
                        n.ParentNode.Name != "script" &&
                        n.ParentNode.Name != "style");

                    //enumerate nodes to build a new string, cleaning textnodes along the way 
                    foreach (var txtsnip in textndes)
                    {
                        var txtsnipitem = new OperationsModel.TextItem { Scrapedtext = txtsnip.InnerText };

                        txtsnipitem.Scrubbedtext = TextCleanup(txtsnipitem.Scrapedtext);

                        if (txtsnipitem.Scrubbedtext != "")
                        {
                            //add spaces along the way just to make sure..these will be removed later
                            sb.Append(" ");
                            sb.Append(txtsnipitem.Scrubbedtext);
                            sb.Append(" ");
                        }

                    }

                    //take semi-cleaned string, add it to scraped word block
                    ScrapedWordBlock = sb.ToString();

                    //cleanup runs of whtespace from ScrapedWordBlock
                    ScrubbedWordBlock = Regex.Replace(ScrapedWordBlock, @"\s+", " ");

                    //define separators for primary enum
                    string[] separators = { ",", ".", "!", "\'", " ", "\'s" };

                    //define where to split, listing any weird chars i missed, return words to array..this array used for matchquery
                    string[] source = ScrubbedWordBlock.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    //use hash simply for the contains, which checks off whether I've added a value or not
                    var hshcheck = new Hashtable();

                    //enum my words, while searching each word in my matchquery to get a count of its presence in the scrubbedwordblock
                    foreach (string word in ScrubbedWordBlock.Split(separators, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var worditem = new OperationsModel.TextItem();

                        //only allow entrance if my word has not already been added.. for example, as i enum my words I'll hit them multiple times, but i only want to execute my match once because it will return all occurences from one call. So If my word has already been run through, I'll skip it.
                        if (!hshcheck.ContainsValue(word.ToLowerInvariant()))
                        {
                            hshcheck.Add(word, word.ToLowerInvariant());

                            //load my item prop
                            worditem.Scrubbedtext = word;

                            // this query finds all occurrences of the word in the wordblock just by running the word once. 
                            var matchQuery = from wrd in source
                                             where wrd.ToLowerInvariant().Trim() == worditem.Scrubbedtext.ToLowerInvariant().Trim()
                                             select wrd;

                            // execute the query, spit out the matches.
                            int wordCount = matchQuery.Count();

                            //load another item prop
                            worditem.OccurenceCount = wordCount;

                            //add item to my return list
                            list.Add(worditem);
                        }
                    }
                }
                catch(Exception ex)
                {
                    
                }
                
                
               
                //sort and return occurence count DESC
                var sortedList = list.OrderByDescending(o => o.OccurenceCount).Take(10).ToList();

                return sortedList;
            }

            private static string TextCleanup(string txtstr)
            {

                var retstr = txtstr;
                var myWriter = new StringWriter();
                // Decode the encoded string.
                HttpUtility.HtmlDecode(retstr, myWriter);
                retstr = myWriter.ToString();
                
                //cleanup extra chars
                retstr = retstr.Replace("&", String.Empty)
                    .Replace(".", String.Empty)
                    .Replace("-", String.Empty)
                    .Replace(",", String.Empty)
                    .Replace("?", String.Empty)
                    .Replace("!", String.Empty)
                    .Replace("*", String.Empty)
                    .Replace("<", String.Empty)
                    .Replace(">", String.Empty)
                    .Replace("[", String.Empty)
                    .Replace("'", String.Empty)
                    .Replace("]", String.Empty)
                    .Replace("(", String.Empty)
                    .Replace(")", String.Empty)
                    .Replace("$", String.Empty)
                    .Replace(":", String.Empty)
                    .Replace(";", String.Empty)
                    .Replace("'", String.Empty)
                    .Replace("|", String.Empty)
                    .Replace("</form>", String.Empty);
                
                retstr = retstr.Trim();
                return retstr;

            }
        }
    }
}






