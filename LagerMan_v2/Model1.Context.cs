﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LagerMan_v2
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class inventoryBaseEntities : DbContext
    {
        public inventoryBaseEntities()
            : base("name=inventoryBaseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<activeInventory> activeInventory { get; set; }
        public DbSet<application> application { get; set; }
        public DbSet<inverters> inverters { get; set; }
        public DbSet<panels> panels { get; set; }
        public DbSet<passiveInventory> passiveInventory { get; set; }
        public DbSet<postCodes> postCodes { get; set; }
        public DbSet<productCatalog> productCatalog { get; set; }
        public DbSet<statistics> statistics { get; set; }
        public DbSet<suppliers> suppliers { get; set; }
        public DbSet<technical> technical { get; set; }
        public DbSet<users> users { get; set; }
    }
}
