using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.LuceneVersion;

namespace SmartTicketSystem.Infrastructure.Services;

public class LuceneCategoryMatcher
{
    private readonly IConfiguration _config;

    public LuceneCategoryMatcher(IConfiguration config)
    {
        _config = config;
    }

    public int DetectCategory(string content)
    {
        var directory = FSDirectory.Open(_config["Lucene:IndexDirectory"]);

        using var analyzer = new StandardAnalyzer(Version.LUCENE_48);
        using var reader = DirectoryReader.Open(directory);
        var searcher = new IndexSearcher(reader);

        var parser = new QueryParser(Version.LUCENE_48, "Keywords", analyzer);
        var query = parser.Parse(content);

        var result = searcher.Search(query, 1);
        if (result.TotalHits == 0)
            return 0;

        var doc = searcher.Doc(result.ScoreDocs[0].Doc);
        return int.Parse(doc.Get("CategoryId"));
    }
}