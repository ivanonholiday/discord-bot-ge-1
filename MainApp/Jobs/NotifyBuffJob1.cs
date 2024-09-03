using Discord;
using Discord.Webhook;
using Quartz;

namespace MainApp.Jobs;

public class NotifyBuffJob1 : IJob
{
    public async Task Execute(IJobExecutionContext? context)
    {
        var urlString = Environment.GetEnvironmentVariable("POST_URLS");
        if (string.IsNullOrWhiteSpace(urlString)) return;
        var urls = urlString.Split(",").Select(p => p.Trim()).ToList();

        var embed = new EmbedBuilder
        {
            Title = "領地摸球 開跑！",
            Color = Color.Gold,
        }.Build();

        foreach (var url in urls)
        {
            using var client = new DiscordWebhookClient(url);

            await client.SendMessageAsync(
                // text: "text here",
                embeds: new[] { embed }
            );
        }
    }
}