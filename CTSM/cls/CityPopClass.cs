using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSM.cls
{
    public class CityPopClass
    {
        private string city_id;
        private int city_pop;
        private string culture_type;
        private int removed_pop;

        public CityPopClass(string city_id, int city_pop, string culture_type, int removed_pop)
        {
            this.City_id = city_id;
            this.City_pop = city_pop;
            this.Culture_type = culture_type;
            this.Removed_pop = removed_pop;
        }

        public string City_id { get => city_id; set => city_id = value; }
        public int City_pop { get => city_pop; set => city_pop = value; }
        public string Culture_type { get => culture_type; set => culture_type = value; }
        public int Removed_pop { get => removed_pop; set => removed_pop = value; }
    }
}
