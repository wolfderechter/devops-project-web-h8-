﻿using EuropArt.Domain.Artists;
using EuropArt.Domain.Artworks;
using EuropArt.Persistence.Data;
using EuropArt.Shared.Artworks;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EuropArt.Shared.Common;
using EuropArt.Domain.Common;
using EuropArt.Services.Infrastructure;

namespace EuropArt.Services.Artworks
{
    public class ArtworkService : IArtworkService
    {
        private readonly HooopDbContext dbContext;
        private readonly DbSet<Artwork> artworks;
        private readonly DbSet<Artist> artists;
        private readonly IStorageService storageService;
        public ArtworkService(HooopDbContext dbContext, IStorageService storageService)
        {
            this.dbContext = dbContext;
            artworks = dbContext.Artworks;
            artists = dbContext.Artists;
            this.storageService = storageService;

        }

        private IQueryable<Artwork> GetArtworkById(int id) => artworks
            .AsNoTracking()
            .Where(p => p.Id == id);
        public async Task<ArtworkResponse.Create> CreateAsync(ArtworkRequest.Create request)
        {
            await Task.Delay(100);
            ArtworkResponse.Create response = new();

            var model = request.Artwork;
            var imageExtension = model.ImagePath.Substring(model.ImagePath.LastIndexOf('.'));
            var imageFileName = Guid.NewGuid().ToString() + request.Artwork.ImagePath + imageExtension;
            var imagePath = $"{storageService.StorageBaseUri}{imageFileName}";

            var artwork = artworks.Add(new Artwork(model.Name, model.Price, model.Description, artists.First(), model.DateCreated)
            {
                //Id = artworks.Max(x => x.Id) + 1,
                //Fake data opvullen
                ImagePath = imagePath,
            });

            await dbContext.SaveChangesAsync();
            response.ArtworkId = artwork.Entity.Id;

            var uploadUri = storageService.CreateUploadUri(imageFileName);
            response.UploadUri = uploadUri;

            return response;
        }

        public async Task DeleteAsync(ArtworkRequest.Delete request)
        {
            artworks.RemoveIf(x => x.Id == request.ArtworkId);
            await dbContext.SaveChangesAsync();
        }

        public async Task<ArtworkResponse.Edit> EditAsync(ArtworkRequest.Edit request)
        {
            ArtworkResponse.Edit response = new();
            //artwork exists?
            var artwork = await artworks
            .Where(p => p.Id == request.ArtworkId).SingleOrDefaultAsync();

            if(artwork is not null)
            {
                var model = request.Artwork;
                //artwork aanpassen
                artwork.Name = model.Name;
                artwork.Description = model.Description;
                artwork.Price = new Money(model.Price);
                //artwork.ImagePath = request.Artwork.ImagePath;

                //returnen
                dbContext.Entry(artwork).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
                response.ArtworkId = artwork.Id;
            }

            return response;
        }

        public async Task<ArtworkResponse.GetDetail> GetDetailAsync(ArtworkRequest.GetDetail request)
        {
            ArtworkResponse.GetDetail response = new();

            response.Artwork = await GetArtworkById(request.ArtworkId).Select(x => new ArtworkDto.Detail
            {
                Id = x.Id,
                Name = x.Name,
                ArtistId = x.Artist.Id,
                ArtistName = x.Artist.Name,
                Description = x.Description,
                Price = x.Price.Value,
                DateCreated = x.DateCreated,
                ImagePath = x.ImagePath,
            })
            .SingleOrDefaultAsync(x => x.Id == request.ArtworkId);

            return response;
        }

        public async Task<ArtworkResponse.GetIndex> GetIndexAsync(ArtworkRequest.GetIndex request)
        {
            await Task.Delay(100);
            ArtworkResponse.GetIndex response = new();

            //Query om te filteren
            var query = artworks.Include(x => x.Artist).AsQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Searchterm))
                query = query.Where(x => x.Name.Contains(request.Searchterm, StringComparison.OrdinalIgnoreCase) ||
                            x.Artist.Name.ToString().Contains(request.Searchterm, StringComparison.OrdinalIgnoreCase));

            if (request.MinimumPrice is not null)
                query = query.Where(x => x.Price.Value >= request.MinimumPrice);

            if (request.MaximumPrice is not null)
                query = query.Where(x => x.Price.Value <= request.MaximumPrice);

            if (request.Style is not null)
            {
                query = query.Where(x => x.Style.Equals(request.Style, StringComparison.OrdinalIgnoreCase));
            }
            if (request.Category is not null)
            {
                query = query.Where(x => x.Category.Equals(request.Category, StringComparison.OrdinalIgnoreCase));
            }

            //auctions includen TODO
            var query2 = new List<Artwork>();
            if (request.OrderBy is not null)
            {
                switch (request.OrderBy.Value)
                {
                    case OrderByArtwork.OrderByPriceAscending:
                        query2 = query.OrderBy(x => x.Price.Value).ToList();
                        break;

                    case OrderByArtwork.OrderByPriceDescending:
                        query2 = query.OrderByDescending(x => x.Price.Value).ToList();
                        break;

                    case OrderByArtwork.OrderByName:
                        query2 = query.OrderBy(x => x.Name).ToList();
                        break;

                    //case OrderBy.OrderByOldest:
                    //    query2 = query.OrderBy(x => x.).ToList();
                    //    break;

                    //case OrderBy.OrderByNewest:
                    //     query2 = query.OrderBy(x => x.).ToList();
                    //    break;
                    default:
                        query2 = query.ToList();
                        break;
                }
            }
            //wanneer request komt van buiten artwork page -> snel resultaten terugeven zonder ordening (vb homepage)
            //else binnenkomen orderby niet opgevuld is dus request komt niet van artist index page
            else
            {
                response.Artworks = query.Select(x => new ArtworkDto.Index
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImagePath = x.ImagePath,
                    Price = x.Price,
                    DateCreated = x.DateCreated,
                    ArtistId = x.Artist.Id,
                    ArtistName = x.Artist.Name
                }).ToList();

                return response;
            }

            response.TotalAmount = query2.Count();
            query2 = query2.Skip(request.Amount * request.Page).ToList();
            query2 = query2.Take(request.Amount).ToList();

            response.Artworks = query2.Select(x => new ArtworkDto.Index
            {
                Id = x.Id,
                Name = x.Name,
                ImagePath = x.ImagePath,
                Price = x.Price.Value,
                DateCreated = x.DateCreated,
                ArtistId = x.Artist.Id,
                ArtistName = x.Artist.Name
            }).ToList();

            return response;
        }
    }
}
