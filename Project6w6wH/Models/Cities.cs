namespace Project6w6wH.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Cities
    {
        public int Id { get; set; }

        public string Area { get; set; }

        public string CountryName { get; set; }

        public string Country { get; set; }
    }
}
