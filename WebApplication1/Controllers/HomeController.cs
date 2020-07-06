using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;


// Inseridos
using System.Security.Cryptography; // para utilização do MD%
using System.Text; // Para conversão de texto 

using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Personagem
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string UrlImagem { get; set; }
    public string UrlWiki { get; set; }
}

public class Comic
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string UrlImagem { get; set; }
}

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public string publicKey = "001ac6c73378bbfff488a36141458af2";
        //public string privateKey = "34822d85476cb25458a16c0b061e9e4662a826e8";
        public string baseURL = "https://gateway.marvel.com/v1/public/characters";
        public string hash = "72e5ed53d1398abb831c3ceec263f18b";
        public string ts = "thesoer";
       
        
        
        // converte as strings dadas para o formato md5(ts+privateKey+publicKey), essencial para utilizar a API da marvel comics
        /*
        private string GerarHash(string ts, string publicKey, string privateKey)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(ts + privateKey + publicKey);
            var gerador = MD5.Create();
            byte[] bytesHash = gerador.ComputeHash(bytes);
            return BitConverter.ToString(bytesHash).ToLower().Replace("-", String.Empty);
        }
        */


        public IActionResult Index(int pageNumber=1, int PageSize=20)
        {
            Personagem[] personagem;
            int AccessOffset = (pageNumber - 1) * PageSize;


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //string ts = DateTime.Now.Ticks.ToString();

                //string hash = GerarHash(ts, publicKey, privateKey);

                string fullURL = baseURL + $"?ts={ts}&apikey={publicKey}&hash={hash}&offset={AccessOffset}&limit={PageSize}";  
                                 
                HttpResponseMessage response = client.GetAsync(fullURL).Result;

                response.EnsureSuccessStatusCode();
                string conteudo = response.Content.ReadAsStringAsync().Result;
                
                dynamic resultado =  JsonConvert.DeserializeObject(conteudo);
                personagem = new Personagem[resultado.data.count];

                for (int i = 0; i < personagem.Length; i++)
                {
                    personagem[i] = new Personagem();
                    personagem[i].Id = resultado.data.results[i].id;
                    personagem[i].Nome = resultado.data.results[i].name;
                    personagem[i].Descricao = resultado.data.results[i].description;
                    personagem[i].UrlImagem = resultado.data.results[i].thumbnail.path + "." +
                                              resultado.data.results[i].thumbnail.extension;
                    personagem[i].UrlWiki = resultado.data.results[i].urls[1].url;


                   


                }
            }

            return View(personagem);
        }


        public IActionResult CharacterDetails(int id)
        {
            Comic[] comics;
            
            using (var client = new HttpClient())
            {
                string comicsURL = baseURL + "/" + id.ToString() + "/comics" + $"?ts={ts}&apikey={publicKey}&hash={hash}";

                HttpResponseMessage response = client.GetAsync(comicsURL).Result;
                string conteudoComic = response.Content.ReadAsStringAsync().Result;
                dynamic resultadoComic = JsonConvert.DeserializeObject(conteudoComic);
                comics = new Comic[resultadoComic.data.count];

                for(int i=0; i < comics.Length; i++)
                {
                    comics[i] = new Comic();
                    comics[i].Id = resultadoComic.data.results[i].id;
                    comics[i].Descricao = resultadoComic.data.results[i].description;
                    comics[i].UrlImagem = resultadoComic.data.results[i].thumbnail.path + "." +
                                          resultadoComic.data.results[i].thumbnail.extension;
                    comics[i].Titulo = resultadoComic.data.results[i].title;
                }
            }

            return View(comics);

        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
