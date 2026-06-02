using System.ComponentModel.DataAnnotations;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class Table : BaseEntity<int>
    {
        public int Number { get; set; }

        public int AreaId { get; set; }
        public Area? Area { get; set; }

        public int Capacity { get; set; }
    }
}
