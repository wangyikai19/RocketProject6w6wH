using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Project6w6wH.Models
{
    public partial class Model : DbContext
    {
        public Model()
            : base("name=Models")
        {
        }

        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<Advertises> Advertise { get; set; }
        public virtual DbSet<Cities> City { get; set; }
        public virtual DbSet<CollectStores> CollectStore { get; set; }
        public virtual DbSet<CommentLikes> CommentLike { get; set; }
        public virtual DbSet<CommentPictures> CommentPictures { get; set; }
        public virtual DbSet<CommnetReports> CommnetReport { get; set; }
        public virtual DbSet<Configs> Configs { get; set; }
        public virtual DbSet<Follows> Follow { get; set; }
        public virtual DbSet<Members> Member { get; set; }
        public virtual DbSet<Notifies> Notify { get; set; }
        public virtual DbSet<Replies> Reply { get; set; }
        public virtual DbSet<ReplyLikes> ReplyLike { get; set; }
        public virtual DbSet<SearchConditions> SearchCondition { get; set; }
        public virtual DbSet<SearchRecords> SearchRecord { get; set; }
        public virtual DbSet<StoreComments> StoreComments { get; set; }
        public virtual DbSet<StorePictures> StorePictures { get; set; }
        public virtual DbSet<Stores> Stores { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stores>()
       .Property(e => e.XLocation)
       .HasPrecision(18, 8);

            modelBuilder.Entity<Stores>()
                .Property(e => e.YLocation)
                .HasPrecision(18, 8);

            modelBuilder.Entity<StoreComments>()
                .HasRequired(sc => sc.Members)
                .WithMany(m => m.StoreComments)
                .HasForeignKey(sc => sc.MemberId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
