namespace Project6w6wH.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ReplyLikes
    {
        public int Id { get; set; }

        public int ReplyId { get; set; }

        public int LikeUserId { get; set; }

        public DateTime? CreateTime { get; set; }

        public int? Member_Id { get; set; }

        public virtual Members Members { get; set; }

        public virtual Replies Replies { get; set; }
    }
}
