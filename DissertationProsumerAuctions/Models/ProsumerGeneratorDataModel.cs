using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions.Models
{
    public class ProsumerGeneratorDataModel
    {
        [PrimaryKey, AutoIncrement]
        public int ProsumerId { get; set; }
        public string Time { get; set; }
        public double GenerationRate { get; set; }
        public override string ToString()
        {
            return $"ProsumerId: {ProsumerId}, Time: {Time}, GenerationRate: {GenerationRate}";
        }
    }
}
