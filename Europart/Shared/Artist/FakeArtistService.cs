﻿using Bogus;
using EuropArt.Shared.Artist;
using EuropArt.Shared.Artworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuropArt.Shared.Artist
{
    public class FakeArtistService : IArtistService
    {
        private static List<ArtistDto.Detail> artists = new();

        static FakeArtistService()
        {
            var artistIds = 0;

            var artworkFaker = new FakeArtworkService();
            var artworks = artworkFaker.GetArtworks();


            var faker = new Faker<ArtistDto.Detail>("en")
                .RuleFor(x => x.Id, _ => artistIds++)
                .RuleFor(x => x.ImagePath, f => f.Internet.Avatar())
                .RuleFor(x => x.City, f => f.Address.City())
                //.RuleFor(x => x.Artworks, f => artworks.Where(a => a.Artist.Id == artistIds))
                .RuleFor(x => x.Website, f => f.Person.Website)
                .RuleFor(x => x.Biography, f => f.Lorem.Sentence(5))
                .RuleFor(x => x.Name, f => f.Name.FullName());

            artists.AddRange(faker.Generate(5));
            artists.ForEach(a => a.Artworks = artworks.Where(aw => aw.Artist.Id == a.Id));
        }

        public async Task<ArtistDto.Detail> GetDetailAsync(int id)
        {
            await Task.Delay(100);
            return artists.SingleOrDefault(x => x.Id == id);
        }

        public async Task<IEnumerable<ArtistDto.Index>> GetIndexAsync()
        {
            await Task.Delay(100);
            return artists.AsEnumerable();
        }

        public IEnumerable<ArtistDto.Detail> GetArtists()
        {
            return artists;
        }

        public async Task<IEnumerable<ArtistDto.Index>> GetIndexAsync(string searchterm)
        {
            await Task.Delay(100);
            return artists.AsEnumerable().Where(a => a.Name.ToLower().Contains(searchterm) || a.Biography.ToLower().Contains(searchterm));
        }

        public Task UpdateArtistAsync(ArtistDto.Edit model, int id)
        {
            var a = artists.SingleOrDefault(x => x.Id == id);

            a.Name = model.Name;
            a.City = model.City;
            a.Website = model.Website;
            a.Biography = model.Biography;

            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var a = artists.SingleOrDefault(x => x.Id == id);
            artists.Remove(a);
            return Task.CompletedTask;
        }
    }
}
