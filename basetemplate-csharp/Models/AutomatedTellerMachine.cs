using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace basetemplate_csharp.Models
{
    [Table("automated_teller_machines")]
    public class AutomatedTellerMachine
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required, MaxLength(16)]
        [Column("atmidentifier")]
        public string? AtmIdentifier { get; set; }

        [MaxLength(70)]
        [Column("atmaddress_streetname")]
        public string? AtmAddressStreetName { get; set; }

        [MaxLength(16)]
        [Column("atmaddress_buildingnumber")]
        public string? AtmAddressBuildingNumber { get; set; }

        [MaxLength(35)]
        [Column("atmtownname")]
        public string? AtmTownName { get; set; }

        [MaxLength(35)]
        [Column("atmdistrictname")]
        public string? AtmDistrictName { get; set; }

        [MaxLength(35)]
        [Column("atmcountrysubdivisionmajorname")]
        public string? AtmCountrySubdivisionMajorName { get; set; }

        [Required]
        [Column("atmfromdatetime")]
        public DateTime AtmFromDatetime { get; set; }

        [Required]
        [Column("atmtodatetime")]
        public DateTime AtmToDatetime { get; set; }

        [MaxLength(35)]
        [Column("atmtimetype")]
        public string? AtmTimeType { get; set; }

        [MaxLength(50)]
        [Column("atmattentionhour")]
        public string? AtmAttentionHour { get; set; }

        [Required, MaxLength(4)]
        [Column("atmservicetype")]
        public string? AtmServiceType { get; set; }

        [Required, MaxLength(4)]
        [Column("atmaccesstype")]
        public string? AtmAccessType { get; set; }

    }
}

