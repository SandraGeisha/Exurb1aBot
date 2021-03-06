﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Exurb1aBot.Model.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exurb1aBot.Util.Extensions;

namespace Exurb1aBot.Util.EmbedBuilders {
    public static class EmbedBuilderFunctions {
        private static EmbedFooterBuilder embf = new EmbedFooterBuilder();

        public static async Task GiveAllCommands(CommandService _commands, ICommandContext context, string ErrorReason = null) {
            IEnumerable<ModuleInfo> modInfo = _commands.Modules;
            EmbedBuilder ebm = new EmbedBuilder();

            if (ErrorReason == null) {
                ebm.WithTitle("Command list");
                ebm.WithColor(Color.Teal);
            }else {
                ebm.WithTitle(ErrorReason);
                ebm.WithColor(Color.Red);
            }

            ebm.WithDescription("List of all available commands:");

            foreach (ModuleInfo mi in modInfo) {
                ebm.AddField(mi.Name, String.Join(", ", mi.Commands.Select(mx => mx.Name).Where(name=>name.Trim().Length!=0).Distinct().ToArray()));
            }

            ebm.WithFooter(AddFooter(context).Result);

            await context.Channel.SendMessageAsync(embed: ebm.Build());
        }

        public async static Task<EmbedBuilder> MakeHelp(string title,string description,
            string url,string commandName,string[] parameters, string[] Examples,ICommandContext context) {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithColor(Color.Teal);
            eb.WithTitle(title);

            eb.WithDescription(description);
            eb.WithThumbnailUrl(url);

            eb.AddField("command name", commandName, true);
            eb.AddField("Parameters", string.Join(",",parameters), true);
            eb.AddField("Examples", string.Join("\r\n",Examples));

            eb.WithFooter(await AddFooter(context));

            return eb;
        }

        public async static Task<EmbedFooterBuilder> AddFooter(ICommandContext context) {
            IGuildUser user = await context.Guild.GetUserAsync((ulong)401452008957280257);
            embf.WithIconUrl(user.GetAvatarUrl());
            embf.Text = $"Made by {user.Nickname??user.Username}";
            return embf;
        }

        public async static Task GiveErrorSyntax(string command, string[] parameters, string[] examples, ICommandContext context) {
            EmbedBuilder ebm = new EmbedBuilder() {
                Color = Color.Red
            };

            ebm.WithTitle("Command Error Syntax");
            ebm.WithDescription($"Error in running the command {command}");

            ebm.AddField("Parameters", string.Join(" ", parameters));

            ebm.AddField("Examples", string.Join("\r\n", examples));

            ebm.WithFooter(await AddFooter(context));

            await context.Channel.SendMessageAsync(embed: ebm.Build());
        }

        public async static Task DisplayQuote(Quote q, IGuildUser[] users, ICommandContext context) {
            EmbedBuilder ebm = new EmbedBuilder() {
                Color = Color.Blue
            };

            IGuildUser quotee = users[0];
            ebm.WithTitle($"Quote #{q.Id} by {(quotee == null ? q.Qoutee.Username : (quotee.Nickname??quotee.Username))}");

            if (quotee != null) 
                ebm.WithThumbnailUrl(quotee.GetAvatarUrl());
            if (ebm.ThumbnailUrl == null)
                ebm.WithThumbnailUrl("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png");

            ebm.AddField("Quote", $"```\r\n{q.QuoteText.RemoveAbuseCharacters()}\r\n```");

            EmbedFooterBuilder efb = new EmbedFooterBuilder();
            IGuildUser creator = users[1];

            if (creator != null) 
                efb.WithIconUrl(creator.GetAvatarUrl());
            if (efb.IconUrl == null)
                efb.WithIconUrl("https://discordapp.com/assets/dd4dbc0016779df1378e7812eabaa04d.png");

            efb.WithText($"Quoted by {(creator == null ? q.Creator.Username : (creator.Nickname??creator.Username))} on {q.Time.ToShortDateString()}");
            ebm.WithFooter(efb);

            await context.Channel.SendMessageAsync(embed: ebm.Build());
        }


        
        public async static Task UnhandledException(string ex,ISocketMessageChannel channel) {
            EmbedBuilder builder = new EmbedBuilder {
                ImageUrl = "https://37.media.tumblr.com/40bc69a6ae90bfe6a90fbdce4fb7516b/tumblr_n6k977lF4T1rna0heo1_1280.gif",
                Color = Color.Red,
                Description = "Error: " + ex
            };
            await channel.SendMessageAsync(embed: builder.Build());
        }
    }
}
