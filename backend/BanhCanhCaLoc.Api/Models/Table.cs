using System.ComponentModel.DataAnnotations;

namespace BanhCanhCaLoc.Api.Models
{
    public class Table
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public int AreaId { get; set; }
        public Area? Area { get; set; }

        public int Capacity { get; set; }
    }
}
