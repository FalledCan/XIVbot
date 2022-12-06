
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;


namespace XIVbot;
class Program
{
    private DiscordSocketClient _client;
    public static CommandService _commands;
    public static IServiceProvider _services;


    //keyとToken
    string key_Token(String kt)
    {
        if (kt == "token")
            return "MTA0OTQ5MjQ2OTMzMjU5MDY1Mw.GibMpb.o8EBqK-hYdDPpm1LY_RrMJ1jK5H0EeZ0tGMxuk";
        else if (kt == "key")
            return "&private_key=63a4cc3c61404bddb8b35acc18b7694f14f0728f42014909b79d74fab5fd0e4a";
        return kt;
    }

    static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    //Log
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    //Discordbotメイン
    public async Task MainAsync()
    {
        _client = new DiscordSocketClient();
        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        var token = key_Token("token");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await Task.Delay(-1);
    }


    //コマンド登録
    public async Task Client_Ready()
    {
        var globalCommand = new SlashCommandBuilder()
            .WithName("search")
            .WithDescription("FFXIV内のアイテム検索")
            .AddOption("itemname", ApplicationCommandOptionType.String, "アイテム名を書いてください",isRequired: true);
        try
        { 
            await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
        }
        catch (ApplicationCommandException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
    //コマンド受信
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "search":
                await SearchCommand(command);
                break;
        }
    }

    private async Task SearchCommand(SocketSlashCommand command)
    {

        var item = command.Data.Options.First().Value;
        Console.WriteLine(item);

        string str = searchitem((string)item);

        if (str == null)
        {
            await command.RespondAsync("正しいアイテム名を入力して下さい。");
        }
        else
        {

            string[] strsplit = str.Split(",");
            var embedBuilder = new EmbedBuilder()
                .WithAuthor(strsplit[2], "https://xivapi.com/" + strsplit[1])
                .WithColor(Color.Green)
                .WithTitle("マーケット一覧")
                .WithCurrentTimestamp();
            await command.RespondAsync(embed: embedBuilder.Build());
        }
    }

    string searchitem(string item) 
    {
        string url = "https://xivapi.com/search?indexes=item&string=" + item + "&fuzzy=1&language=ja" + key_Token("key");
        WebRequest request = WebRequest.Create(url);
        Stream response_stream = request.GetResponse().GetResponseStream();
        StreamReader reader = new StreamReader(response_stream);
        var xiv_json = JObject.Parse(reader.ReadToEnd());

        if ((int)xiv_json["Pagination"]["Results"] == 1)
        {
            var j_id = xiv_json["Results"][0]["ID"];
            var j_icon = xiv_json["Results"][0]["Icon"];
            var j_name = xiv_json["Results"][0]["Name"];
            var j_url = xiv_json["Results"][0]["Url"];
            Console.WriteLine(j_name);

            return j_id + "," + j_icon + "," + j_name + "," + j_url;
        }

        return null;


    }

}
