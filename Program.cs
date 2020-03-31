using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HungerGamesTelegram.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;


namespace HungerGamesTelegram
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
            //WriteLine("Hunger Games");

            //var telegramGameHost = new TelegramGameHost();
            //telegramGameHost.Start();

            //Thread.Sleep(int.MaxValue);

            //Game game = new Game(new ConsoleNotificator());
            //while (true)
            //{
            //    game.Players.Add(new ConsolePlayer(game));
            //    await game.StartGame();
            //}
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .ConfigureServices(collection =>
                {
                    collection.AddScoped<ITelegramBotClient>(provider =>
                    {
                        var key = File.ReadAllText("botkey.key");
                        return new TelegramBotClient(key);
                    });
                    collection.AddHostedService<HungerGamesGameService>();
                    collection.AddSingleton<TelegramListener>();
                    collection.AddScoped<TelegramSender>();
                })
                .UseConsoleLifetime();
        }
    }

    internal class TelegramSender
    {
        private readonly ILogger<TelegramSender> _logger;
        private readonly ITelegramBotClient _botClient;

        public TelegramSender(ILogger<TelegramSender> logger, ITelegramBotClient botClient)
        {
            _logger = logger;
            _botClient = botClient;
        }

        public void SendMessage(int id, string message) => SendMessage(new TelegramMessage(id, message));

        public Task SendMessage(TelegramMessage message)
        {
            _logger.LogDebug($"Sending message: {message.Id}: {message.Text}");
            return _botClient.SendTextMessageAsync(message.Id, message.Text, ParseMode.MarkdownV2, replyMarkup: message.Keyboard);
        }
    }

    internal class TelegramListener
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramListener> _logger;

        public TelegramListener(ILogger<TelegramListener> logger, ITelegramBotClient botClient)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public void StartListening(CancellationToken token)
        {
            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving(new []
            {
                UpdateType.Message,
            }, token);
        }

        List<Game> _gamesInProgress = new List<Game>();

        private void Bot_OnMessage(object? sender, MessageEventArgs e)
        {
            _logger.LogDebug("Message received");
            try
            {
                OnMessageReceived(new TelegramMessage(e.Message));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error processing telegram message");
            }
        }

        public event Action<TelegramMessage> MessageReceived;
        protected virtual void OnMessageReceived(TelegramMessage message) => MessageReceived?.Invoke(message);

        
    }

    internal class TelegramMessage
    {
        public TelegramMessage(Message message)
        {
            Id = message.From.Id;
            Text = message.Text;
            Keyboard = new ReplyKeyboardRemove();
        }

        public TelegramMessage(int id, string text) : this(id, text, new ReplyKeyboardRemove()) { }

        public TelegramMessage(int id, string text, IReplyMarkup keyboard)
        {
            Id = id;
            Text = text;
            Keyboard = keyboard ?? new ReplyKeyboardRemove();
        }

        public int Id { get; }
        public string Text { get; }
        public IReplyMarkup Keyboard { get; }
    }

    internal class GameRules
    {
        public TimeSpan RoundTime { get; set; } = TimeSpan.FromMinutes(1);
        public int Dimensions { get; set; } = 12;
    }

    internal class HungerGamesGameService : BackgroundService
    {
        private readonly ILogger<HungerGamesGameService> _logger;
        private readonly TelegramListener _telegramListener;
        private readonly TelegramSender _telegramSender;
        private readonly IServiceProvider _serviceProvider;

        public HungerGamesGameService(ILogger<HungerGamesGameService> logger, TelegramListener telegramListener, TelegramSender telegramSender, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _telegramListener = telegramListener;
            _telegramSender = telegramSender;
            _serviceProvider = serviceProvider;
        }

        private List<Game> Games { get; } = new List<Game>();

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting game server");
            _telegramListener.MessageReceived += OnMessage;

            _telegramListener.StartListening(stoppingToken);

            CreateNewGame();

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _telegramListener.MessageReceived -= OnMessage;
            return base.StopAsync(cancellationToken);
        }

        private void OnMessage(TelegramMessage message)
        {
            // Handle stateless messages
            switch (message.Text)
            {
                case "/start":
                    _telegramSender.SendMessage(message.Id, "start");
                    return;
                case "/regler":
                    _telegramSender.SendMessage(message.Id, "regler");
                    return;
            }

            // Handle admin messages
            if (IsAdmin(message.Id))
            {
                switch (message.Text)
                {
                    case "/newGame":
                    {
                        CreateNewGame();
                        return;
                    }
                    case "/startGame":
                    {
                        if (GetFirstGameNotStarted(out var gameToStart))
                        {
                            //gameToStart.Start();
                        }
                        else
                        {
                            // No Game to start
                        }
                        return;
                    }
                }
            }

            // Check if player is in a game
            if (GetGame(message.Id, out var game))
            {
                //if (IsAdmin(message.Id))
                //{
                //    // Handle in-game admin messages
                //    switch (message.Text)
                //    {
                //        case "/endGame":
                //    }
                //}

                // Handle in-game messages
                //if (game.HandleMessage(message))
                //    return;
            }
            else
            {
                // Handle out-of-game messages
                switch (message.Text)
                {
                    case "/join":
                    {
                        if (GetFirstGameNotStarted(out var gameToJoin))
                        {
                            //gameToJoin.AddPlayer(message.Id);
                        }
                        else
                        {
                            // Send /notify description
                        }
                        return;
                    }
                    case "/notify":
                    {
                        if (GetFirstGameNotStarted(out var existingGame))
                        {
                            // There is a game ready to join
                        }
                        else
                        {
                            // Add player to list of players to notify when a new game is starting
                        }
                        return;
                    }
                }
            }

            // Reply with I did not understand what you meant - message
        }

        private void CreateNewGame()
        {
            var game = _serviceProvider.CreateScope().ServiceProvider.GetService<Game>();
            Games.Add(game);
            // Notify players in wait list
        }

        private bool GetFirstGameNotStarted(out object game)
        {
            game = Games.FirstOrDefault(x => !x.Started);
            return game != null;
        }

        private bool GetGame(in int id, out object game)
        {
            game = new object();
            return false;
        }

        private bool IsAdmin(in int id)
        {
            return id == 0;
        }
    }
}
