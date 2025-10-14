using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Lijan { 

    public partial class Vatrate
    {
       public long VatRateId { get; set; }

       public string CountryCode { get; set; } = null!;

      public decimal VatPercent { get; set; }
    }

}