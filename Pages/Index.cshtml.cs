using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using downr.Models;
using downr.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace downrv2
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet=true)]
        public string Slug { get; set; }
        public IYamlIndexer YamlIndexer { get; private set; }
        public Post Post { get; private set; }
        public Dictionary<string, int> Categories { get; private set; }
        public string Next { get; private set; }
        public string NextTitle { get; private set; }
        public string Previous { get; private set; }
        public string PreviousTitle { get; private set; }

        public IndexModel(IYamlIndexer yamlIndexer)
        {
            this.YamlIndexer = yamlIndexer;   
        }
        
        public void OnGet()
        {
            // if no slug was passed our way, grab the first one
            if (!YamlIndexer.Posts.Any(x => x.Slug == Slug))
                this.Slug = YamlIndexer.Posts.First().Slug;

            this.Post = YamlIndexer.Posts.First(x => x.Slug == Slug);
            this.Categories = YamlIndexer.GetCategories();

            GetNextAndPrevious();
        }

        private void GetNextAndPrevious()
        {
            // last post?
            int index = YamlIndexer.Posts.FindIndex(x => x.Slug == Slug);
            if (index != 0)
            {
                Next = YamlIndexer.Posts.ElementAt(index - 1).Slug;
                NextTitle = YamlIndexer.Posts.ElementAt(index - 1).Title;
            }

            // first post?
            if (index != YamlIndexer.Posts.Count - 1)
            {
                Previous = YamlIndexer.Posts.ElementAt(index + 1).Slug;
                PreviousTitle = YamlIndexer.Posts.ElementAt(index + 1).Title;
            }
        }
    }
}