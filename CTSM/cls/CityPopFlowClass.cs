using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSM.cls
{
    public class CityPopFlowClass
    {
        private string o_city_id;
        private string d_city_id;
        private int flow_pop;

        public CityPopFlowClass(string o_city_id, string d_city_id, int flow_pop)
        {
            this.O_city_id = o_city_id;
            this.D_city_id = d_city_id;
            this.Flow_pop = flow_pop;
        }

        public string O_city_id { get => o_city_id; set => o_city_id = value; }
        public string D_city_id { get => d_city_id; set => d_city_id = value; }
        public int Flow_pop { get => flow_pop; set => flow_pop = value; }
    }
    public class OutPutFlowClass
    {
        private string Culture;
    }
}
