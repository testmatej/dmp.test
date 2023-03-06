using rssreader.Models;

namespace rssreader.Data
{
    public static class DbInitializer
    {
        public static void Initialize(DataContext context)
        {
            //Look for any feeds
            if (context.Feeds.Any())
            {
                return;
            }

            var feeds = new Feed[]
                {
                    new Feed{URL = "https://servis.idnes.cz/rss.aspx?c=zpravodaj", Name="Zprávy iDNES.cz", Description="Nejrychlejší zpravodajství na českém internetu, události z domova i celého světa"},
                    new Feed{URL = "https://servis.idnes.cz/rss.aspx?c=sport", Name="Sport iDNES.cz", Description="Nejlepší sport na českém internetu. Původní zpravodajství, on-line reportáže, rozhovory, analýzy, diskuse, ostatní sporty."},
                    new Feed{URL = "https://games.tiscali.cz/rss2.xml", Name="Tiscali Games", Description=""},
                    new Feed{URL = "https://www.zive.cz/rss/sc-47/", Name="Živě.cz", Description=""}
                };

            context.Feeds.AddRange(feeds);
            context.SaveChanges();
        }
    }
}
