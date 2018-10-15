using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using downr.Models;
using YamlDotNet.Serialization;
using Microsoft.Extensions.Logging;

namespace downr.Services
{
    public interface IYamlIndexer
    {
        List<Post> Posts { get; set; }
        void IndexContentFiles(string contentPath);
        Dictionary<string, int> GetCategories();
    }

    public class DefaultYamlIndexer : IYamlIndexer
    {
        IMarkdownContentLoader _markdownLoader;
        ILogger _logger;

        public DefaultYamlIndexer(IMarkdownContentLoader markdownLoader,
            ILoggerFactory loggerFactory)
        {
            Posts = new List<Post>();
            _markdownLoader = markdownLoader;
            _logger = loggerFactory.CreateLogger<DefaultYamlIndexer>();
        }

        public List<Post> Posts { get; set; }

        public void IndexContentFiles(string contentPath)
        {
            var subDirectories = Directory.GetDirectories(contentPath);
            //_logger.LogInformation(message: $"{subDirectories.Length} directories found in posts folder.");

            foreach (var subDirectory in subDirectories)
            {
                using (var rdr = File.OpenText(
                        Path.Combine(subDirectory, "index.md")
                    ))
                {
                    // make sure the file has the header at the first line
                    var line = rdr.ReadLine();
                    if (line == "---")
                    {
                        line = rdr.ReadLine();

                        var stringBuilder = new StringBuilder();

                        // keep going until we reach the end of the header
                        while (line != "---")
                        {
                            stringBuilder.Append(line);
                            stringBuilder.Append("\n");
                            line = rdr.ReadLine();
                        }

                        var yaml = stringBuilder.ToString();
                        var de = new Deserializer();
                        var result = de.Deserialize<Dictionary<string, string>>(new StringReader(yaml));

                        // convert the dictionary into a model
                        var metadata = new Post
                        {
                            Slug = result[Strings.MetadataNames.Slug],
                            Title = result[Strings.MetadataNames.Title],
                            Author = result[Strings.MetadataNames.Author],
                            PublicationDate = DateTime.Parse(result[Strings.MetadataNames.PublicationDate]),
                            LastModified = DateTime.Parse(result[Strings.MetadataNames.LastModified]),
                            Categories = result[Strings.MetadataNames.Categories].Split(','),
                            Content = _markdownLoader.GetContentToRender(result[Strings.MetadataNames.Slug])
                        };

                        Posts.Add(metadata);
                        //_logger.LogInformation(message: $"Added post '{metadata.Title}' to the list of posts.");
                    }
                }
            }

            Posts = Posts.OrderByDescending(x => x.PublicationDate).ToList();
        }

        public Dictionary<string, int> GetCategories()
        {
            Dictionary<string, int> categories = new Dictionary<string, int>();
            
            Posts.Select(x => x.Categories).ToList().ForEach(c =>
            {
                c.ToList().ForEach(category =>
                {
                    if (!categories.ContainsKey(category.Trim()))
                        categories.Add(category.Trim(), 0);
                    categories[category.Trim()] += 1;
                });
            });

            return categories;
        }
    }
}