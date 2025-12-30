using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using SmartTicketSystem.Infrastructure.Lucene;
using SmartTicketSystem.Infrastructure.Persistence;

using Version = Lucene.Net.Util.LuceneVersion;

namespace SmartTicketSystem.Infrastructure.Services;

public class LuceneIndexService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public LuceneIndexService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public void BuildCategoryIndex()
    {
        var directory = LuceneDirectoryProvider.GetIndexDirectory(_config);
        var analyzer = new StandardAnalyzer(Version.LUCENE_48);
        var indexConfig = new IndexWriterConfig(Version.LUCENE_48, analyzer);

        using var writer = new IndexWriter(directory, indexConfig);
        writer.DeleteAll();

        var jsonFile = LuceneDirectoryProvider.GetKeywordsFilePath(_config);
        var jsonData = File.ReadAllText(jsonFile);
        var keywordsMap = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonData);

        var categories = _context.TicketCategories.ToList();

        foreach (var cat in categories)
        {
            var keywords = keywordsMap.ContainsKey(cat.Name) ? string.Join(" ", keywordsMap[cat.Name]) : "";

            var doc = new Document
            {
                new StringField("CategoryId", cat.CategoryId.ToString(), Field.Store.YES),
                new TextField("Keywords", keywords, Field.Store.YES)
            };

            writer.AddDocument(doc);
        }

        writer.Commit();
    }
}