namespace Project6w6wH.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Follows
    {
        public int Id { get; set; }

        public int FollowUserId { get; set; }

        public int UserId { get; set; }

        public DateTime? CreateTime { get; set; }

        public virtual Members Members { get; set; }
    }
}
