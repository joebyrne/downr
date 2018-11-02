using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using downr.Models;
using downr.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace downr
{
    public class CategoriesModel : PageModel
    {
        [BindProperty(SupportsGet=true)]
        public string Category { get; set; }
        public IYamlIndexer Indexer { get; private set; }
        public Post[] Posts { get; private set; }

        public CategoriesModel(IYamlIndexer indexer)
        {
            this.Indexer = indexer;
        }

        public void OnGet()
        {
            Posts = this.Indexer.Posts.Where(x => x.Categories.Contains(this.Category)).ToArray();
        }
    }
}