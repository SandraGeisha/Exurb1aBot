﻿using Discord;
using Discord.Commands;
using Exurb1aBot.Util.Extensions;
using Exurb1aBot.Model.Domain;
using Exurb1aBot.Model.Exceptions.QuoteExceptions;
using Exurb1aBot.Model.ViewModel;
using Exurb1aBot.Util.EmbedBuilders;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Exurb1aBot.Modules {
    [Name("Quoting Commands")]
    [Group("quote"),Summary("The quote command, accepted methods add,get,delete,user,help")]
    public class QuoteModule:ModuleBase{
        #region Fields
        private readonly IUserRepository _userRepo;
        private IQouteRepository _qouteRepo; 
        #endregion

        #region Constructor
        public QuoteModule(IQouteRepository quoteRepo, IUserRepository userRepository) {
            _qouteRepo = quoteRepo;
            _userRepo = userRepository;
        } 
        #endregion

        #region No input
        [Command("")]
        public async Task NoInput() {
            await GetRandomQuote();
        } 

        [Command("")]
        public async Task NoInput(IGuildUser user) {
            await GetRandomQuoteUser(user);
        }

        [Command("")]
        public async Task NoInput(int id) {
            await GetQuote(id.ToString());
        }
        #endregion

        #region help
        [Command("help")]
        public async Task DisplayHelp() {
            EmbedBuilder eb = await EmbedBuilderFunctions.MakeHelp("Quote command help", 
                "A command made for the purposes of quoting",
               "https://static.thenounproject.com/png/81720-200.png", "quote", new string[] { "help", "get",
                   "random", "user", "remove" }, new string[] { "quote help", "quote add \"I wanna be called margret\" @27#2727"
                ,"quote remove 123456789","quote random","quote get 23","quote user @Margret#0062"}, Context);

            await Context.Channel.SendMessageAsync(embed: eb.Build());
        } 

        [Command("help")]
        public async Task DisplayHelp([Remainder] string s) {
            await DisplayHelp();
        }
        #endregion

        #region Add
        [Command("add")]
        public async Task AddQuote([Remainder] string s) {
            await AddQuote();
        }

        [Command("add")]
        public async Task AddQuote() {
            await EmbedBuilderFunctions.GiveErrorSyntax("quote add",
                new string[] { "**quote**(required must be between \"\") ", "**user**(required must be @mention)" },
                new string[] { $"{Program.prefix}quote add \"Why is the milk gone\" @exurb1a" }, Context);
        }

        [Command("add")]
        public async Task AddQuote(string quote, IGuildUser user) {
            IGuildUser cr = Context.Message.Author as IGuildUser;

            EntityUser creator = new EntityUser(Context.Message.Author as IGuildUser);
            EntityUser quotee = new EntityUser(user);
            Quote q = new Quote(quote.Replace("`","'"), creator, quotee, DateTime.Now,Context.Guild);

            _qouteRepo.AddQuote(q);
            _qouteRepo.SaveChanges();

            int id = _qouteRepo.GetId(q);
            await Context.Channel.SendMessageAsync($"added quote **{q.QuoteText.RemoveAbuseCharacters()}** from" +
                $" **{user.Nickname ??user.Username}**  quoted by **{cr.Nickname ?? cr.Username}** with id {id}");

        }

        public static async Task BotAddQuote(IQouteRepository _quoteRepo,IMessageChannel channel,string quote,ulong msgId, IGuildUser creator, IGuildUser quotee,DateTime time) {
            if (!_quoteRepo.MessageExists(quote,quotee,time)) {
                EntityUser cr = new EntityUser(creator);
                EntityUser quotee2 = new EntityUser(quotee);

                Quote q = new Quote(quote, cr, quotee2, time,creator.Guild) {
                    msgId = msgId
                };

                _quoteRepo.AddQuote(q);
                _quoteRepo.SaveChanges();

                int id = _quoteRepo.GetId(q);
                await channel.SendMessageAsync($"added quote **{q.QuoteText.RemoveAbuseCharacters()}**" +
                    $" from **{quotee.Nickname ?? quotee.Username}** quoted by **{creator.Nickname ?? creator.Username}** " +
                    $"with id {id}");
            }
        }
        #endregion

        #region Random
        [Command("random")]
        public async Task GetRandomQuote() {
            Quote q = _qouteRepo.GetRandom();
            IGuildUser[] users = await GetGuildUsers(q);
            await EmbedBuilderFunctions.DisplayQuote(q, users, Context);
        }


        [Command("random")]
        public async Task GetRandomQuote([Remainder] string x) {
            await EmbedBuilderFunctions.GiveErrorSyntax("quote random",
                 new string[] { "**user**(optional,must be an @mention)" },
                 new string[] { $"{Program.prefix}quote random", $"{Program.prefix}quote random @exurb1a" },
                 Context);
        }

        [Command("random")]
        public async Task GetRandomQuote(IGuildUser user) {
            await GetRandomQuoteUser(user);
        }
        #endregion

        #region User
        [Command("user")]
        public async Task GetRandomQuoteUser(IGuildUser user) {
            EntityUser user2 = await GetUser(user.Id);
            Quote q = _qouteRepo.GetRandomByUser(user2.Id);
            await EmbedBuilderFunctions.DisplayQuote(q, GetGuildUsers(q).Result, Context);
        }

        [Command("user")]
        public async Task GetRandomQuoteUser() {
            await EmbedBuilderFunctions.GiveErrorSyntax("quote user",
                new string[] { "**user**(required must be @mention)" },
                new string[] { $"{Program.prefix}quote user @exurb1a" }, Context);
        }

        [Command("user")]
        public async Task GetRandomQuoteUser([Remainder] string s) {
            await GetRandomQuoteUser();
        }
        #endregion

        #region Get
        [Command("get")]
        public async Task GetQuote(string qId) {
            string qfid = qId.Replace("#", "");
            if (Int32.TryParse(qfid, out int j)) {
                Quote q = _qouteRepo.GetQuoteById(j);
                await EmbedBuilderFunctions.DisplayQuote(q, GetGuildUsers(q).Result, Context);
            }
            else
                await GetQuote();
        }

        [Command("get")]
        public async Task GetQuote() {
            await EmbedBuilderFunctions.GiveErrorSyntax("quote get",
               new string[] { "(#-optional)**quoteId**(required must be a number) " },
               new string[] { $"{Program.prefix}quote get 7", $"{Program.prefix}quote get #7" }, Context);
        }

        [Command("get")]
        public async Task GetQuote(string x, [Remainder] string s) {
            await GetQuote();
        }
        #endregion

        #region Remove
        [Command("remove")]
        public async Task RemoveQuote(int id) {
            var Author = Context.Message.Author as IGuildUser;
            Quote q = _qouteRepo.GetQuoteById(id);

            if (q == null)
                throw new QouteNotFound();

            if (Author.GuildPermissions.ManageNicknames || q.Creator.Id == Author.Id || q.Qoutee.Id == Author.Id) {

                var users = await GetGuildUsers(q);
                IGuildUser quotee = users[0];

                _qouteRepo.RemoveQuote(id);
                _qouteRepo.SaveChanges();

                if(quotee!=null)
                    await Context.Channel.SendMessageAsync($"Quote {q.Id} \"{q.QuoteText.RemoveAbuseCharacters()}\" by {quotee.Nickname ?? quotee.Username} deleted");
                else
                    await Context.Channel.SendMessageAsync($"Quote {q.Id} \"{q.QuoteText.RemoveAbuseCharacters()}\" by {q.Qoutee.Username} deleted");
            }
            else
                await Context.Channel.SendMessageAsync("You can't delete someone elses quotes");
        }

        [Command("remove")]
        public async Task RemoveQuote() {
            await EmbedBuilderFunctions.GiveErrorSyntax("quote remove",
                new string[] { "**quoteId**(required) " },
                new string[] { $"{Program.prefix}quote remove 5 " }, Context);
        }

        [Command("remove")]
        public async Task RemoveQuote([Remainder]string s) {
            await RemoveQuote();
        } 
        #endregion

        #region Helping functions
        private async Task<IGuildUser[]> GetGuildUsers(Quote q) {
            IGuildUser quotee = await Context.Guild.GetUserAsync(q.Qoutee.Id);
            IGuildUser creator = await Context.Guild.GetUserAsync(q.Creator.Id);
            return new IGuildUser[] { quotee, creator };
        }

        private async Task<EntityUser> GetUser(ulong id) {
            IGuildUser gu = await Context.Guild.GetUserAsync(id);

            if (gu == null)
                throw new UserNotFoundException();

            return new EntityUser(gu);
        } 
        #endregion

    }
}
