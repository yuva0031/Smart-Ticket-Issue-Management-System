using System.IO;

using Lucene.Net.Store;

using LuceneDirectory = Lucene.Net.Store.Directory;


namespace SmartTicketSystem.Infrastructure.Lucene;

public static class LuceneDirectoryProvider
{
    public static LuceneDirectory GetIndexDirectory(IConfiguration config)
    {
        var indexPath = config["Lucene:IndexDirectory"];
        System.IO.Directory.CreateDirectory(indexPath);
        return FSDirectory.Open(indexPath);
    }

    public static string GetKeywordsFilePath(IConfiguration config)
    {
        return config["Lucene:CategoryKeywordsFile"];
    }
}