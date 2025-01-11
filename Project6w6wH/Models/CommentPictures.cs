namespace Project6w6wH.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CommentPictures
    {
        public int Id { get; set; }

        public int CommentId { get; set; }

        public string PictureUrl { get; set; }

        public DateTime? CreateTime { get; set; }

        public virtual StoreComments StoreComments { get; set; }
    }
}
