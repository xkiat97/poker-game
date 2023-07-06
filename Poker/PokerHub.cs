using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Poker
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; } = 0;
        public int Chip { get; set; }
        public string CardChoosen { get; set; } = "null";

        public Player(string id, string name)
        {
            Id = id;
            Name = name;
            Chip =5;
        }
    }

    public class Game
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public List<Player> players = new List<Player>();
        public string start = "no";
        public string card1 = "null";
        public string card2 = "null";
        public string bigSmall = "null";
        public int cardA = 0;
        public int cardB = 0;
        public string[] cards;
        public int entryFee = 1;
        
        public void Gamer(Player player)
        {
            players.Add(player);
        }

        public void GenerateRandFee()
        {
            Random rand = new Random();

            int prob = rand.Next(5);

            if (prob == 0)
                prob = 1;

            entryFee = prob;
        }
    }

    public class PokerHub : Hub
    {
        private static List<Game> games = new List<Game>();
        private static List<Player> players = new List<Player>();
        private static string direction = "leave";

        public void InitializeAll()
        {
            string gameId = Context.QueryString["gameId"];
            string name = Context.QueryString["name"];

            Game game = games.Find(g => g.Id == gameId);
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            Player player = game.players.Find(p => p.Name == name);
            if (name == null)
            {
                Clients.Caller.Reject();
                return;
            }

            if (player.Chip == 0)
            {
                Clients.All.Kick(player);
            }

            for (int i = 0; i < game.players.Count; i++)
            {
                game.players[i].CardChoosen = "null";
                game.start = "no";
                game.card1 = "null";
                game.card2 = "null";
                game.bigSmall = "null";
                game.cardA = 0;
                game.cardB = 0;
                game.cards = null;
                game.entryFee = 1;
            }

            Clients.Group(gameId).InitializeAll(game);

        }

        // in list page
        public void InitializeSpecific(string name)
        {
            Player player = players.Find(p => p.Name == name);
            if (name == null)
            {
                Clients.Caller.Reject();
                return;
            }

            player.CardChoosen = "null";
        }


        public void CheckWinLose()
        {
            string gameId = Context.QueryString["gameId"];
            string name = Context.QueryString["name"];
            string winLose = "null";

            Game game = games.Find(g => g.Id == gameId);
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            Player player = game.players.Find(p => p.Name == name);
            if (name == null)
            {
                Clients.Caller.Reject();
                return;
            }

            winLose = "null";
            if (game.bigSmall != "null")
            {
                if (game.bigSmall == "big")
                {
                    if (int.Parse(game.card1) > int.Parse(game.card2))
                    {
                        winLose = game.card1;
                    }
                    else if (int.Parse(game.card1) < int.Parse(game.card2))
                    {
                        winLose = game.card2;
                    }
                }
                else if (game.bigSmall == "small")
                {
                    if (int.Parse(game.card1) > int.Parse(game.card2))
                    {
                        winLose = game.card2;
                    }
                    else if (int.Parse(game.card1) < int.Parse(game.card2))
                    {
                        winLose = game.card1;
                    }
                }

                if (player.CardChoosen == "null")
                {
                    Clients.Client(player.Id).CheckWinLose("You no join the game");
                }
                else if (player.CardChoosen == winLose)
                {
                    player.Chip += (game.entryFee != 1 ? game.entryFee*2: 2);
                    Clients.Client(player.Id).CheckWinLose("You Win! Chip + "+ (game.entryFee != 1 ? game.entryFee * 2 : 2)+". Now still left " + player.Chip);
                    Clients.Client(player.Id).Chip(player.Chip);
                }
                else
                {
                    Clients.Client(player.Id).CheckWinLose("You Lose! Now still left " + player.Chip);
                }
            }
            else
            {
                Clients.Caller.Reject();
                return;
            }

        }

        public void CardChoosen(string selected)
        {
            string gameId = Context.QueryString["gameId"];
            string name = Context.QueryString["name"];

            Game game = games.Find(g => g.Id == gameId);
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            Player player = game.players.Find(p => p.Name == name);
            if (name == null)
            {
                Clients.Caller.Reject();
                return;
            }

            if (player.CardChoosen == "null")
            {
                player.Chip -= game.entryFee;
            }

            player.CardChoosen = selected;

            game.cardA = 0;
            game.cardB = 0;
            for (int i = 0; i < game.players.Count; i++)
            {
                if(game.players[i].CardChoosen == game.card1)
                {
                    game.cardA++;
                }
                else if (game.players[i].CardChoosen == game.card2)
                {
                    game.cardB++;
                }
            }

            Clients.Group(gameId).Count(game);
            Clients.Client(player.Id).Chip(player.Chip);
        }

        public string ChooseBigSmall(Game game)
        {     
            Random rand = new Random();
            
            int prob = rand.Next(100);

            if (prob <= 49)
            {
                game.bigSmall = "small";
                return "smaller";
            }
            else
            {
                game.bigSmall = "big";
                return "bigger";
            }
        }

        public void PickedCard()
        {
            string gameId = Context.QueryString["gameId"];

            Game game = games.Find(g => g.Id == gameId);
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            Clients.Group(gameId).PickedCard(game.cards);
        }


        public void SetCards(string[] cards, int firstTime)
        {
            string gameId = Context.QueryString["gameId"];

            Game game = games.Find(g => g.Id == gameId);
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            game.cards = cards;
            
            Clients.Group(gameId).SetCards(cards, firstTime);
        }

        public void PokerCard(string[] card)
        {
            string gameId = Context.QueryString["gameId"];
            string name = Context.QueryString["name"];

            Game game = games.Find(g => g.Id == gameId);
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            Player player = game.players.Find(p => p.Name == name);
            if (player == null)
            {
                Clients.Caller.Reject();
                return;
            }

            if (game.players[0].Name == name)
            {
                game.card1 = card[0];
                game.card2 = card[1];
                Clients.Group(gameId).PokerCard(game);
            }

        }

        public void Hide()
        {
            string gameId = Context.QueryString["gameId"];

            Game game = games.Find(g => g.Id == gameId);
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            game.start = "yes";
            UpdateGameList();

            Clients.Group(gameId).Hide();
            Start(game);
            
        }

        public void Start(Game game)
        {
            string gameId = Context.QueryString["gameId"];
            string id = Context.ConnectionId;
            string bigSmall = "null";
            string name = Context.QueryString["name"];

            Player player = game.players.Find(p => p.Name == name);
            if (player == null)
            {
                Clients.Caller.Reject();
                return;
            }

            game.GenerateRandFee();
            bigSmall = ChooseBigSmall(game);

            Clients.Group(gameId).TotalNum(game);
            Clients.Group(gameId).Start(bigSmall,game.entryFee);
        }

        public void OpenPublic()
        {
            string gameId = Context.QueryString["gameId"];
            string name = Context.QueryString["name"];

            Game game = games.Find(g => g.Id == gameId);
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            game.start = "no";
            UpdateGameList();
        }

        public string Create()
        {
            Game game = new Game();
            games.Add(game);

            return game.Id;
        }

        // To consider whether need to remove user or not from the player list
        public void Direction(string h)
        {
            if (h == "game")
                direction = "game";
            else
                direction = "leave";
        }

        // Return the game rooms details
        private void UpdateGameList(string id = null)
        {
            var list = games.FindAll(g => g.players.FirstOrDefault() != null);

            if (id == null)
                Clients.All.UpdateGameList(list);
            else
                Clients.Client(id).UpdateGameList(list);
        }

        public override Task OnConnected()
        {
            //Clients.All.Test("aaa");
            string page = Context.QueryString["page"];
            switch (page)
            {
                case "list": ListConnected(); break;
                case "game": GameConnected(); break;
            }

            return base.OnConnected();
        }

        // TODO: When connected to list page
        private void ListConnected()
        {
            string id = Context.ConnectionId;
            string name = Context.QueryString["name"];

            Player player = players.Find(p => p.Name == name);
            if (player == null)
            {
                Player newPlayer = new Player(id, name);
                players.Add(newPlayer);
                Clients.Client(id).Chip(newPlayer.Chip);
            }
            else
            {
                player.Id = id;
                Clients.Client(id).Chip(player.Chip);
            }
            InitializeSpecific(name);

            Player py = players.Find(ply => ply.Name == name);
            if (py.Chip == 0)
            {
                Clients.All.Kick(py);
            }
            //countPlayers++;
            Clients.All.Status(players.Count);
            UpdateGameList(id);
        }

        // TODO: When connected to game page
        private async void GameConnected()
        {
            string gameId = Context.QueryString["gameId"];
            string id = Context.ConnectionId;
            string name = Context.QueryString["name"];

            // (1) Checking
            Game game = games.Find(g => g.Id == gameId);
            
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            Player player = players.Find(p => p.Name == name);
            

            if (player == null)
            {
                Clients.Caller.Reject();
                return;
            }
            else if(player.Chip == 0)
            {
                Clients.All.Kick(player);
            }
            else
            {
                int playerCount = game.players.Count;
                // (2) Game exists
                if (game.players == null)
                {
                    player.Id = id;
                    game.Gamer(player);
                    await Groups.Add(player.Id, gameId);
                    Clients.Group(gameId);
                    UpdateGameList();
                }
                else if (playerCount <= 6)
                {
                    player.Id = id;
                    game.Gamer(player);
                    await Groups.Add(player.Id, gameId);
                    Clients.Group(gameId);
                    UpdateGameList();
                }
                else
                {
                    // Game full --> reject
                    Clients.Caller.Reject();
                }

                if (game.players.Count(p => p.Name == name) >= 2)
                {
                    Clients.Group(gameId).Duplicated(name);
                }
            }

            Clients.Client(id).Chip(player.Chip);
            Clients.Group(gameId).TotalNum(game);
            // Clients.Group(gameId).ShowCard(game.);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string page = Context.QueryString["page"];

            switch (page)
            {
                case "list": ListDisconnected(); break;
                case "game": GameDisconnected(); break;
            }

            return base.OnDisconnected(stopCalled);
        }

        // When disconnected to list page
        private void ListDisconnected()
        {
            string name = Context.QueryString["name"];

            Player player = players.Find(p => p.Name == name);
            if (player != null)
            {
                if (direction == "leave")
                {
                    Clients.All.Kick(player);
                    players.Remove(player);
                }

                //countPlayers--;
                Clients.All.Status(players.Count);
            }
        }

        // When disconnected to game page
        private void GameDisconnected()
        {
            string gameId = Context.QueryString["gameId"];
            string name = Context.QueryString["name"];

            // (1) Checking
            Game game = games.Find(g => g.Id == gameId);
            if (game == null)
            {
                Clients.Caller.Reject();
                return;
            }

            Player player = game.players.Find(g => g.Name == name);
            if (player == null)
            {
                Clients.Caller.Reject();
                return;
            }
            else
            { 
                if (game.players.FirstOrDefault().Name == name)
                {

                    if (direction == "leave")
                    {
                        Clients.All.Kick(player);
                        players.Remove(player);
                    }
                    else
                    {
                        Clients.Group(gameId).Back(name);
                    }

                    game.players = null;
                    Clients.Group(gameId).Info();
                    games.Remove(game);
                    UpdateGameList();
                }
                else
                {
                    if (direction == "leave")
                    {
                        Clients.All.Kick(player);
                        players.Remove(player);
                    }
                    else
                    {
                        Clients.Group(gameId).Back(name);
                    }

                    game.players.Remove(player);
                    UpdateGameList();
                }
            }

            Clients.Group(gameId).TotalNum(game);

        }
    }
}