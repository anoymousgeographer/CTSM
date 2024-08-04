using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSM.cls
{
    public class CityPopCulturalStructureClass
    {
        private string city_id;
        private Dictionary<string, PersonCount> crowd_cultural_structure_dic = new Dictionary<string, PersonCount>();

        private Dictionary<int, string> gene_order_codeName_dic = new Dictionary<int, string>();

        public string City_id { get => city_id; set => city_id = value; }
        public Dictionary<string, PersonCount> Crowd_cultural_structure_dic { get => crowd_cultural_structure_dic; set => crowd_cultural_structure_dic = value; }
        public Dictionary<int, string> Gene_order_codeName_dic { get => gene_order_codeName_dic; set => gene_order_codeName_dic = value; }

        public double[] cultural_structure_rateList()
        {
            double[] genes_rate = new double[crowd_cultural_structure_dic.Count];

            double genes_pop_total = crowd_cultural_structure_dic.Sum(x => x.Value.persons);

            int gene_index = 0;
            foreach (KeyValuePair<string, PersonCount> kvl in crowd_cultural_structure_dic)
            {
                string gene_codeName = kvl.Key;
                int gene_pop_count = kvl.Value.persons;

                genes_rate[gene_index] = gene_pop_count / genes_pop_total;

                Gene_order_codeName_dic[gene_index] = gene_codeName;

                gene_index++;
            }

            return genes_rate;
        }


    
    }

    public class PersonCount
    {
        public int persons;

        public PersonCount(int persons)
        {
            this.persons = persons;
        }
    }
}
