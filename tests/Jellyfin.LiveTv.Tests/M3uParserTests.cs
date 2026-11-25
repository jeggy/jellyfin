using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests;

public class M3uParserTests
{
    [Theory]
    [InlineData("#EXTINF:-1 radio=\"true\",Station", ChannelType.Radio)]
    [InlineData("#EXTINF:-1 radio=\"TRUE\",Station", ChannelType.Radio)]
    [InlineData("#EXTINF:-1 radio=\"false\",Station", ChannelType.TV)]
    [InlineData("#EXTINF:-1 radio=\"yes\",Station", ChannelType.TV)]
    [InlineData("#EXTINF:-1,Station", ChannelType.TV)]
    public async Task Parse_RadioAttribute_SetsChannelType(string extInf, ChannelType expected)
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, $"#EXTM3U\n{extInf}\nhttp://example.com/stream\n", TestContext.Current.CancellationToken);
            var parser = new M3uParser(NullLogger.Instance, Mock.Of<IHttpClientFactory>());

            var channels = await parser.Parse(new TunerHostInfo { Url = path }, "m3u_", TestContext.Current.CancellationToken);

            Assert.Equal(expected, Assert.Single(channels).ChannelType);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
