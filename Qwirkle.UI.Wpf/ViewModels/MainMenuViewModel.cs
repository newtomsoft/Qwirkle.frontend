﻿using Microsoft.Extensions.Configuration;
using Newtomsoft.EntityFramework.Configuration;
using Newtonsoft.Json;
using Qwirkle.Core.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Qwirkle.UI.Wpf.ViewModels
{
    public class MainMenuViewModel : NotifyPropertyChangedBase
    {
        private GameViewModel _gameViewModel;
        private readonly IConfigurationRoot _configuration;
        private readonly HttpClient _httpClient;
        private readonly Action<NotifyPropertyChangedBase> _changeViewModel;

        public ICommand NewGameCommand => new RelayCommand(NewGame);

        public MainMenuViewModel(Action<NotifyPropertyChangedBase> changeViewModel)
        {
            _configuration = NewtomsoftConfiguration.GetConfiguration();
            _httpClient = GetHttpClient();
            _changeViewModel = changeViewModel;
        }

        private void NewGame(object _ = null)
        {
            var playerIds = new List<int> { 10 }; //todo
            var response = CreateGameAsync(playerIds).Result;
            var resultCreateGame = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode) return;
            response.Dispose();
            var players = JsonConvert.DeserializeObject<List<Player>>(resultCreateGame);
            _gameViewModel = new GameViewModel(_configuration, players, _httpClient);
            _changeViewModel(_gameViewModel);
        }

        private HttpClient GetHttpClient()
        {
            string uriWebApi = _configuration.GetValue<string>("UriWebApi");
            var httpClient = new HttpClient { BaseAddress = new Uri(uriWebApi) };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            return httpClient;
        }

        private async Task<HttpResponseMessage> CreateGameAsync(List<int> playerIds) => await _httpClient.PostAsJsonAsync("Games", playerIds).ConfigureAwait(false);
    }
}
