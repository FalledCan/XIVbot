
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
                .WithName("hq")
                .WithDescription("HQの場合はtrue")
                .WithRequired(true)
                .AddChoice("true", 1)
                .AddChoice("false", 2)
                .WithType(ApplicationCommandOptionType.Integer))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("dc")
                .WithDescription("データセンター")
                .WithRequired(false)
                .AddChoice("Elemental", 1)
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

        string world = "-1";
        string dc = "-1";
        bool world_b = false;
        bool dc_b = false;
        bool hq = false;


        try
        {
            var world_id = command.Data.Options.Where(x => x.Name == "elemental_world").First().Value;
            world = world_id + "";
            Console.WriteLine(world);
            world_b = true;
        } catch (Exception exception) { Console.WriteLine("skp1" + exception.Message); }
        try
        {
            var world_id = command.Data.Options.Where(x => x.Name == "gaia_world").First().Value;
            world = world_id + "";
            world_b = true;
        }
        catch (Exception exception) { Console.WriteLine("skp2"); }
        try
        {
            var world_id = command.Data.Options.Where(x => x.Name == "mana_world").First().Value;
            world = world_id + "";
            world_b = true;
        }
        catch (Exception exception) { Console.WriteLine("skp3"); }
        try
        {
            var world_id = command.Data.Options.Where(x => x.Name == "meteor_world").First().Value;
            world = world_id + "";
            world_b = true;
        }
        catch (Exception exception) { Console.WriteLine("skp4"); }
        try
        {
            var dc_id = command.Data.Options.Where(x => x.Name == "dc").First().Value;
            dc = dc_id + "";
            dc_b = true;
        }
        catch (Exception exception) { Console.WriteLine("skp5"); }


        var hqb = command.Data.Options.Where(x => x.Name == "hq").First().Value;
        var item = command.Data.Options.First().Value;
        Console.WriteLine(item + ", " + hqb);

            string str = searchitem((string)item);

        if (str == null)
        {
            await command.RespondAsync("正しいアイテム名を入力して下さい。");
            return;
        }
        string[] strsplit = str.Split(",");
        string[] strsplit2 = market(dc,world,dc_b,world_b,strsplit[0],hqb + ""== "1"? true:false).Split(",");
        var list = new List<EmbedFieldBuilder>();

        if (strsplit2.Length != 1) { 
        

            for (int i = 0; i < 6; i++)
                list.Add(
                    new EmbedFieldBuilder()
                    {
                //0,4,8,12,16,20//1,5,9,13,17,21//3,7,11,15,19,23
                Name = "Ifrit",
                        Value = "単価: " + strsplit2[i == 0 ? 0 : i == 1 ? 4 : i == 2 ? 8 : i == 3 ? 12 : i == 4 ? 16 : 20]
                        + " 個数: " + strsplit2[i == 0 ? 1 : i == 1 ? 5 : i == 2 ? 9 : i == 3 ? 13 : i == 4 ? 17 : 21]
                        + "\n 合計: " + strsplit2[i == 0 ? 3 : i == 1 ? 7 : i == 2 ? 11 : i == 3 ? 15 : i == 4 ? 19 : 23]
                    }
                    );
        }
        else
        {
            list.Add(
                    new EmbedFieldBuilder()
                    {
                        Name = "マーケット取引不可",
                        Value= "null"
                    }
                    );
        }

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
        if ((int)xiv_json["Pagination"]["Results"] != null)
        {
            var j_id = xiv_json["Results"][0]["ID"];
            var j_icon = xiv_json["Results"][0]["Icon"];
            var j_name = xiv_json["Results"][0]["Name"];
            var j_url = xiv_json["Results"][0]["Url"];

            return j_id + "," + j_icon + "," + j_name + "," + j_url;
        }

        return null;


    }

    string market(string dc, string world, bool dc_b, bool world_b , string item_id, bool hq)
    {

        string jdw = "Japan";

        if (dc_b) { jdw = dc; }
        if (world_b) { jdw = world; }
        string url;
        if (hq) { url = "https://universalis.app/api/v2/" + jdw + "/" + item_id + "?listings=6&hq=true"; }
         else{ url = "https://universalis.app/api/v2/" + jdw + "/" + item_id + "?listings=6";}
        Console.WriteLine(url);
            WebRequest request = WebRequest.Create(url);
        Stream response_stream = request.GetResponse().GetResponseStream();
        StreamReader reader = new StreamReader(response_stream);
        var xiv_json = JObject.Parse(reader.ReadToEnd())
        if ((string)xiv_json["itemID"] != "0")
        {
            int count = xiv_json["listings"].Count();
            if (count > 0)
            {
                string list1 = null;
                string list2 = null;
                string list3 = null;
                string list4 = null;
                string list5 = null;
                string list6 = null;
                for (int i = 0; i < 6; i++)
                {
                    if (i < count)
                    {
                        var j_pricePerUnit = xiv_json["listings"][i]["pricePerUnit"];   //0,4,8,12,16,20
                        var j_quantity = xiv_json["listings"][i]["quantity"];           //1,5,9,13,17,21
                        var j_hq = xiv_json["listings"][i]["hq"];                       //2,6,10,14,18,22
                        var j_total = xiv_json["listings"][i]["total"];                 //3,7,11,15,19,23

                        
                    }

                }
                return list;
            }
        }
        return "hoge";
    }

}
