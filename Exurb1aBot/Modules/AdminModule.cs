﻿using Discord;
using Discord.Commands;
using Exurb1aBot.Util.EmbedBuilders;
using System;
using System.Threading.Tasks;

namespace Exurb1aBot.Modules { 
    [Name("Admin Commands")]
   public class AdminModule : ModuleBase<SocketCommandContext> {

        [Name("")]
        [Command("shutdown"), RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task ShutDown() {
            await Context.Channel.SendMessageAsync("Shutting down...");
            Environment.Exit(0);
        }

        #region Stream Command
        [Command("stream"), RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Mention(string name, string url = null) {
            await Context.Client.SetGameAsync(name, url, (url != null ? ActivityType.Streaming : ActivityType.Playing));
        }

        [Command("stream"), RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Mention() {
            await EmbedBuilderFunctions.GiveErrorSyntax("stream", new string[] { "**name**(required)", "**url**(optional,needs to be from twitch)" },
                new string[] { $"{Program.prefix}stream \"existential despair\"",
                    $"{Program.prefix}stream \"existential despair\" \"https://www.twitch.tv/directory/game/Depression%20Quest\"" }, Context);
        }

        [Command("stream"), RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task Mention([Remainder]string s) {
            await Mention();
        } 
        #endregion
    }
}
