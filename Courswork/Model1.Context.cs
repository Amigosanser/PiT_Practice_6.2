﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Courswork
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MPID4855073Entities : DbContext
    {
        public MPID4855073Entities()
            : base("name=MPID4855073Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Achievements> Achievements { get; set; }
        public virtual DbSet<Attendance> Attendance { get; set; }
        public virtual DbSet<Children> Children { get; set; }
        public virtual DbSet<Feedback> Feedback { get; set; }
        public virtual DbSet<GameCategories> GameCategories { get; set; }
        public virtual DbSet<Games> Games { get; set; }
        public virtual DbSet<Locations> Locations { get; set; }
        public virtual DbSet<Methodists> Methodists { get; set; }
        public virtual DbSet<Parents> Parents { get; set; }
        public virtual DbSet<Resources> Resources { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Schedule> Schedule { get; set; }
        public virtual DbSet<Users> Users { get; set; }
    }
}
