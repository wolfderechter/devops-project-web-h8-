﻿using EuropArt.Domain.Artworks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuropArt.Persistence.Data.Configuration
{
    public class ArtworkEntityTypeConfiguration : IEntityTypeConfiguration<Artwork>
    {
        public void Configure(EntityTypeBuilder<Artwork> builder)
        {
            
            builder.Property(p => p.Name).IsRequired();
            builder.Property(p => p.ImagePath).IsRequired();
            
            builder.HasOne(p => p.Artist).WithMany(p => p.Artworks);
            builder.OwnsOne(p => p.Price, money =>
            {
                money.Property(m => m.Value)
                    .HasColumnName("Price")
                    .IsRequired();
            }).Navigation(p => p.Price).IsRequired();
            builder.Property(p => p.DateCreated);
        }
    }
}
