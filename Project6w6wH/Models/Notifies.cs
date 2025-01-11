namespace Project6w6wH.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Notifies
    {
        public int Id { get; set; }

        public int ReplyId { get; set; }

        public int ReplyUserId { get; set; }

        public int CommentId { get; set; }

        public int CommentUserId { get; set; }

        public int Check { get; set; }

        public DateTime? CreateTime { get; set; }

        public virtual Replies Replies { get; set; }
    }
}
