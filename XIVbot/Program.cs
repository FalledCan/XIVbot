﻿
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
            .AddOption("itemname", ApplicationCommandOptionType.String, "アイテム名を書いてください", isRequired: true)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("dc")
                .WithDescription("データセンター")
                .WithRequired(false)
                .AddChoice("Elemental",1)
                .AddChoice("Gaia", 2)
                .AddChoice("Mana", 3)
                .AddChoice("Meteor", 4)
                .WithType(ApplicationCommandOptionType.Integer))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("elemental_world")
                .WithDescription("Elementalワールド")
                .WithRequired(false)
                .AddChoice("Aegis", 90)
                .AddChoice("Atomos", 68)
                .AddChoice("Carbuncle", 45)
                .AddChoice("Garuda", 58)
                .AddChoice("Gungnir", 94)
                .AddChoice("Kujata", 49)
                .AddChoice("Tonberry", 72)
                .AddChoice("Typhon", 50)
                .WithType(ApplicationCommandOptionType.Integer))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("gaia_world")
                .WithDescription("Gaiaワールド")
                .WithRequired(false)
                .AddChoice("Alexander", 43)
                .AddChoice("Bahamut", 69)
                .AddChoice("Durandal", 92)
                .AddChoice("Fenrir", 46)
                .AddChoice("Ifrit", 59)
                .AddChoice("Ridill", 98)
                .AddChoice("Tiamat", 76)
                .AddChoice("Ultima", 51)
                .WithType(ApplicationCommandOptionType.Integer))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("mana_world")
                .WithDescription("Manaワールド")
                .WithRequired(false)
                .AddChoice("Anima", 44)
                .AddChoice("Asura", 23)
                .AddChoice("Chocobo", 70)
                .AddChoice("Hades", 47)
                .AddChoice("Ixion", 48)
                .AddChoice("Masamune", 96)
                .AddChoice("Pandaemonium", 28)
                .AddChoice("Titan", 61)
                .WithType(ApplicationCommandOptionType.Integer))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("meteor_world")
                .WithDescription("Meteorワールド")
                .WithRequired(false)
                .AddChoice("Belias", 24)
                .AddChoice("Mandragora", 82)
                .AddChoice("Ramuh", 60)
                .AddChoice("Shinryu", 29)
                .AddChoice("Unicorn", 30)
                .AddChoice("Valefor", 52)
                .AddChoice("Yojimbo", 31)
                .AddChoice("Zeromus", 32)
                .WithType(ApplicationCommandOptionType.Integer));
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

        if(command.Data.Options.Count > 2)
        {
            await command.RespondAsync("アイテムの名前以外は１つまで入力可能です。");
            return;
        }

        int world = -1;
        int dc = -1;
        bool world_b = false;
        bool dc_b = false;


        try
        {
            var world_id = command.Data.Options.Where(x => x.Name == "elemental_world").First().Value;
            world = (int)world_id;
            world_b = true;
        }catch (Exception exception) { }
        try
        {
            var world_id = command.Data.Options.Where(x => x.Name == "gaia_world").First().Value;
            world = (int)world_id;
            world_b = true;
        }
        catch (Exception exception) { }
        try
        {
            var world_id = command.Data.Options.Where(x => x.Name == "mana_world").First().Value;
            world = (int)world_id;
            world_b = true;
        }
        catch (Exception exception) { }
        try
        {
            var world_id = command.Data.Options.Where(x => x.Name == "meteor_world").First().Value;
            world = (int)world_id;
            world_b = true;
        }
        catch (Exception exception) { }
        try
        {
            var dc_id = command.Data.Options.Where(x => x.Name == "dc").First().Value;
            dc = (int)dc_id;
            dc_b = true;
        }
        catch (Exception exception){}

        var item = command.Data.Options.First().Value;
        Console.WriteLine(item);

            string str = searchitem((string)item);

        if (str == null)
        {
            await command.RespondAsync("正しいアイテム名を入力して下さい。");
            return;
        }
        string[] strsplit = str.Split(",");
        string[] strsplit2 = market(dc,world,dc_b,world_b).Split(",");

        var list = new List<EmbedFieldBuilder>();

        for(int i = 0; i < 6; i++)
        list.Add(
            new EmbedFieldBuilder()
            {
                //0,4,8,12,16,20//1,5,9,13,17,21//3,7,11,15,19,23
                Name = "Ifrit",
                Value = "単価: " + strsplit2[i == 0 ? 0:i == 1 ? 4:i == 2 ? 8:i == 3 ? 12:i == 4? 16:20] 
                + " 個数: " + strsplit2[i == 0 ? 1 : i == 1 ? 5 : i == 2 ? 9 : i == 3 ? 13 : i == 4 ? 17 : 21] 
                + "\n 合計: " + strsplit2[i == 0 ? 3 : i == 1 ? 7 : i == 2 ? 11 : i == 3 ? 15 : i == 4 ? 19 : 23]
            }
            );

        var embedBuilder = new EmbedBuilder()
        {
            Author = new EmbedAuthorBuilder() { Name = strsplit[2] + " #" + strsplit[0] },
            ThumbnailUrl = "https://xivapi.com/" + strsplit[1],
            Color = Color.DarkOrange,
            Title = "マーケット一覧"
        }
            .WithFields(list)
            .WithCurrentTimestamp();


        await command.RespondAsync(embed: embedBuilder.Build());
        
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

    string market(int dc, int world, bool dc_b, bool world_b)
    {

        string url = "https://universalis.app/api/v2/59/36047?listings=10&hq=true";
        WebRequest request = WebRequest.Create(url);
        Stream response_stream = request.GetResponse().GetResponseStream();
        StreamReader reader = new StreamReader(response_stream);
        var xiv_json = JObject.Parse(reader.ReadToEnd());
        string list = null;
        for (int i = 0; i < 6; i++) {
            var j_pricePerUnit = xiv_json["listings"][0]["pricePerUnit"];   //0,4,8,12,16,20
            var j_quantity = xiv_json["listings"][0]["quantity"];           //1,5,9,13,17,21
            var j_hq = xiv_json["listings"][0]["hq"];                       //2,6,10,14,18,22
            var j_total = xiv_json["listings"][0]["total"];                 //3,7,11,15,19,23

            list = list + j_pricePerUnit + "," + j_quantity + "," + j_hq + "," + j_total + ",";
        }
        return list;
    }

}
