using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MonasteryRetreat.Domain.Entities;

public class Governorate
{
    public long Id { get; set; }

    public string NameAr { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;


}
