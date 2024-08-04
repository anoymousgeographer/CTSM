using CTSM.cls;
using Facet.Combinatorics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTSM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = Convert.ToInt32(textBox2.Text) + 6;
            
            
            /// city_id,city_pop,culture_type,removed_po;Cx,500000,A,100000
            string city_pop_path = string.Format($"{Application.StartupPath}\\data\\source_data\\country\\{"city_pop.csv"}");
            ///o_city_id,d_city_id,flow_pop;Cx,C1,20000
            string city_pop_flow_path = string.Format($"{Application.StartupPath}\\data\\source_data\\country\\{"city_pop_flow.csv"}");
            ///Culture1,Culture2,Cultural Strength
            string cultural_strength_path = string.Format($"{Application.StartupPath}\\data\\source_data\\country\\{"cultural strength.csv"}");

            string city_culture_path=string.Format($"{Application.StartupPath}\\data\\source_data\\country\\{"city_culture_pop.csv"}");
            textBox1.Text = "读取城市人口数据";
            //读取城市人口数据
            StreamReader sr = new StreamReader(city_pop_path);
            string line = sr.ReadLine();

            Dictionary<string, CityPopClass> cityPopDic = new Dictionary<string, CityPopClass>();
            CityPopClass cityPopClass = null;
            string[] strs = null;

            while ((line = sr.ReadLine()) != null)
            {
                strs = line.Split(',');

                cityPopClass = new CityPopClass(strs[0], Convert.ToInt32(strs[1]), strs[2], Convert.ToInt32(strs[3]));

                cityPopDic[strs[0]] = cityPopClass;
            }

            progressBar1.Value = 1;
            progressBar1.Update();
            progressBar1.Show();

            //读取城市人口流动数据
            sr = new StreamReader(city_pop_flow_path);
            line = sr.ReadLine();
            textBox1.Text = "读取城市人口流动数据";
            Dictionary<string, CityPopFlowClass> cityPopFlowDic = new Dictionary<string, CityPopFlowClass>();
            CityPopFlowClass cityPopFlowClass = null;
            strs = null;

            while ((line = sr.ReadLine()) != null)
            {
                strs = line.Split(',');

                cityPopFlowClass = new CityPopFlowClass(strs[0], strs[1], Convert.ToInt32(strs[2]));

                string keyID = string.Format($"{strs[0]}_{strs[1]}");
                cityPopFlowDic[keyID] = cityPopFlowClass;
            }

            progressBar1.Value ++;
            progressBar1.Update();
            

            //读取文化涵化力参数,X_B代表X对B的涵化力，故NB0-IXB需要乘上系数X_B
            sr = new StreamReader(cultural_strength_path);
            line = sr.ReadLine();

            Dictionary<string, double> CulturalStrengthDic = new Dictionary<string, double>();
            while((line=sr.ReadLine())!=null)
            {
                strs = line.Split(',');
                string CKey = string.Format($"{strs[0]}_{strs[1]}");
                CulturalStrengthDic[CKey] = Convert.ToDouble(strs[2]);
            }
            textBox1.Text = "读取参数数据";
            progressBar1.Value = 3;
            progressBar1.Update();

            //构建文化基因结构
            HashSet<string> geneHashSet = new HashSet<string>();
            foreach (KeyValuePair<string, CityPopClass> kv in cityPopDic)
            {
                geneHashSet.Add(kv.Value.Culture_type);
            }

            Dictionary<string, int> geneDic = new Dictionary<string, int>();
            foreach (var item in geneHashSet)
            {
                string geneKey = string.Format($"N_{item}_0");
                geneDic[geneKey] = 0;

                geneKey = string.Format($"H_{item}_0");
                geneDic[geneKey] = 0;

                geneKey = string.Format($"R_0_0");
                geneDic[geneKey] = 0;

            }
            textBox1.Text = "构建文化基因结构";
            progressBar1.Value = 4;
            progressBar1.Update();
            progressBar1.Show();

            //构建一个集合，基因两两组合
            //迭代每一个，并添加到I和A开头的染色体中
            {
                List<string> strList = geneHashSet.ToList();
                Combinations<string> variations = new Combinations<string>(strList, 2);
             
                foreach (List<string> v in variations)
                {
                    // Console.WriteLine(String.Format("{{{0} {1}}}", v[0], v[1]));

                    string geneKey = string.Format($"I_{v[0]}_{v[1]}");
                    geneDic[geneKey] = 0;

                    geneKey = string.Format($"A_{v[0]}_{v[1]}");
                    geneDic[geneKey] = 0;

                }
            }

            progressBar1.Value = 5;
            progressBar1.Update();

            //存放一个城市的数据，以文化类型为主键
            Dictionary<string,int> CityData= new Dictionary<string, int>();
            //存放所有城市的数据
            Dictionary<string, Dictionary<string, int>> AllCityData = new Dictionary<string, Dictionary<string, int>>();

            //读取所有城市的数据
            sr = new StreamReader(city_culture_path);
            line = sr.ReadLine();
            string[] strs0 = line.Split(',');

            while ((line = sr.ReadLine()) != null)
            {
                strs = line.Split(',');
                string CKey = strs[0];
                for(int Ci=1;Ci<strs0.Length;Ci++)
                {
                    CityData[strs0[Ci]] = Convert.ToInt32(strs[Ci]);
                }
                Dictionary<string, int> CityDataCopy = new Dictionary<string, int>();
                foreach(KeyValuePair<string, int> keyValuePair in CityData)
                {
                    CityDataCopy[keyValuePair.Key] = keyValuePair.Value;
                }
                AllCityData[CKey] = CityDataCopy;
                
            }

            //构造<城市ID,基因字典类>的字典，并初始化
            Dictionary<string, CityPopCulturalStructureClass> city_gene_pop_dic = new Dictionary<string, CityPopCulturalStructureClass>();

            foreach (KeyValuePair<string, CityPopClass> kv in cityPopDic)
            {
                string currentCityKey = kv.Key;

                city_gene_pop_dic[currentCityKey] = new CityPopCulturalStructureClass();

                city_gene_pop_dic[currentCityKey].City_id = currentCityKey;

                //为每个城市初始化染色体
                foreach (KeyValuePair<string, int> kv1 in geneDic)
                {
                    city_gene_pop_dic[currentCityKey].Crowd_cultural_structure_dic[kv1.Key] = new PersonCount(kv1.Value);
                    city_gene_pop_dic[currentCityKey].Crowd_cultural_structure_dic[kv1.Key].persons = AllCityData[currentCityKey][kv1.Key];
                }

            }
            string number = textBox2.Text;
            int iterations_set = Convert.ToInt32(number);
            //str:存放数据

            progressBar1.Value = 6;
            progressBar1.Update();
            textBox1.Text = "构造城市基因结构";
            progressBar1.Show();

            ///str：每个元素代表一个城市，即一行数据（城市名+各文化类型的数量）
            string[] str = new string[(city_gene_pop_dic.Count+1)*iterations_set + 1];
            string[] str_flow = new string[(city_gene_pop_dic.Count() * (city_gene_pop_dic.Count() - 1)+1) * iterations_set + 1];


            //存放每次流动的数据，每次流动后，更新并写入str_flow
            Dictionary<string, Dictionary<string, int>> flow_output = new Dictionary<string, Dictionary<string, int>>();
            foreach(KeyValuePair<string,CityPopClass> kvl in cityPopDic)
            {
                foreach(KeyValuePair<string,CityPopClass> kvl2 in cityPopDic)
                {
                    if(kvl.Value!=kvl2.Value)
                    {
                        string OD = string.Format($"{kvl.Key}_{kvl2.Key}");
                        flow_output[OD] = new Dictionary<string, int>();
                        foreach (KeyValuePair<string, PersonCount> kvl1 in city_gene_pop_dic.First().Value.Crowd_cultural_structure_dic)
                        {
                            flow_output[OD][kvl1.Key] =new int();
                            flow_output[OD][kvl1.Key] = 0;
                        }
                    }
                }
            }

            textBox1.Text = "迭代文化涵化";
            //迭代文化涵化
            for (int iterations=0;iterations< iterations_set; iterations++)
            {
                //人口流动

                //MessageBox.Show(iterations.ToString());
                ///构造每个城市的总流出人口数据字典  {'南京':100, '上海': 200}
                Dictionary<string, int> flowOut_totalPop_eachCity_dic = new Dictionary<string, int>();
                ///从当前城市流出的目的城市的列表  {'南京':{'南京_北京'， ‘南京_上海', ...}}
                Dictionary<string, List<string>> citys_out_from_currentCity_dic = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, CityPopFlowClass> kvl in cityPopFlowDic)
                {
                    string currentKey = kvl.Key.Split('_')[0];

                    if (flowOut_totalPop_eachCity_dic.ContainsKey(currentKey))
                    {
                        flowOut_totalPop_eachCity_dic[currentKey] += kvl.Value.Flow_pop;

                        citys_out_from_currentCity_dic[currentKey].Add(kvl.Key);
                    }
                    else
                    {
                        flowOut_totalPop_eachCity_dic[currentKey] = kvl.Value.Flow_pop;

                        citys_out_from_currentCity_dic[currentKey] = new List<string> { kvl.Key };
                    }
                }

                //人口流动数据存档
                /*
                int count_str_flow_citydic = 0;
                foreach(KeyValuePair<string, List<string>> kvl in citys_out_from_currentCity_dic )
                {
                    string Ocity = kvl.Key;
                    foreach(string kvl1 in kvl.Value)
                    {
                        string Dcity = kvl1.Split('_')[1];
                        str_flow[count_str_flow_citydic] = string.Format($"{ Ocity},{Dcity}");
                        count_str_flow_citydic += 1;
                    }
                }
                */

                //人口流动中的轮盘赌参数
                Dictionary<string, double[]> out_city_rate_dic = new Dictionary<string, double[]>();
                Dictionary<string, Dictionary<int, string>> out_city_pair_order_dic = new Dictionary<string, Dictionary<int, string>>();

                foreach (KeyValuePair<string, int> kvl in flowOut_totalPop_eachCity_dic)
                {
                    string current_city_key = kvl.Key;

                    List<string> od_city_list = citys_out_from_currentCity_dic[current_city_key];

                    double[] rate_arrays = new double[od_city_list.Count];
                    Dictionary<int, string> order_name_mapping_dic = new Dictionary<int, string>();

                    for (int i = 0; i < od_city_list.Count; i++)
                    {
                        double denominator_outPop = kvl.Value * 1.0; //分母
                        double numerator_outPop = cityPopFlowDic[od_city_list[i]].Flow_pop * 1.0; //分子

                        double rate = numerator_outPop / denominator_outPop;

                        rate_arrays[i] = rate;

                        order_name_mapping_dic[i] = od_city_list[i];
                    }

                    out_city_rate_dic[current_city_key] = rate_arrays;
                    out_city_pair_order_dic[current_city_key] = order_name_mapping_dic;

                }




                ///选出每个城市将要流出的带有文化结构标签的人群个体

                /// 当前城市要流出的带有文化结构的人群个体的集合
                List<string> out_pop_gene_codeName_list = null;
                //int count_str_flow_citydic = 0;
                foreach (KeyValuePair<string, CityPopCulturalStructureClass> kvl in city_gene_pop_dic)
                {


                    out_pop_gene_codeName_list = new List<string>();

                    ///拎出当前城市要流出人口个体的文化标签
                    string currentCityKey = kvl.Key;
                    CityPopCulturalStructureClass current_pop_culturalStructure_obj = kvl.Value;

                    if(!flowOut_totalPop_eachCity_dic.ContainsKey(currentCityKey))
                    {
                        MessageBox.Show(currentCityKey.ToString());
                    }
                    int flow_out_pop_count = flowOut_totalPop_eachCity_dic[currentCityKey];

                    double[] genes_rate_list = current_pop_culturalStructure_obj.cultural_structure_rateList();

                    for (int i = 0; i < flow_out_pop_count; i++)
                    {
                        int gene_index = RandomModelClass.nextDiscrete(genes_rate_list);
                        out_pop_gene_codeName_list.Add(current_pop_culturalStructure_obj.Gene_order_codeName_dic[gene_index]);
                    }

                    double[] pop_rate_list = out_city_rate_dic[currentCityKey];
                    foreach (var item in out_pop_gene_codeName_list)
                    {
                        int city_pair_index = RandomModelClass.nextDiscrete(pop_rate_list);

                        string city_pair = out_city_pair_order_dic[currentCityKey][city_pair_index];

                        string o_city = city_pair.Split('_')[0];
                        string d_city = city_pair.Split('_')[1];

                        city_gene_pop_dic[o_city].Crowd_cultural_structure_dic[item].persons -= 1;

                        city_gene_pop_dic[d_city].Crowd_cultural_structure_dic[item].persons += 1;

                        flow_output[city_pair][item] += 1;
                    }
                }

                //打印流动数据

                int count0 = 1;
                foreach (KeyValuePair<string, Dictionary<string, int>> kvl in flow_output)
                {
                    string OD = string.Format($"{kvl.Key}");
                    string[] culturlpop = new string[kvl.Value.Count + 1];
                    culturlpop[0] = OD;
                    int countODC = 1;
                    foreach (KeyValuePair<string, int> kvl2 in kvl.Value)
                    {
                        culturlpop[countODC] = kvl2.Value.ToString();
                        countODC++;
                    }
                    //str_flow[(city_gene_pop_dic.Count() * (city_gene_pop_dic.Count() - 1) + 1) * iterations + count0] = string.Join(",", culturlpop);
                    str_flow[(city_gene_pop_dic.Count() * (city_gene_pop_dic.Count() - 1) + 1)  + count0] = string.Join(",", culturlpop);
                    count0 += 1;
                }
                str_flow[(city_gene_pop_dic.Count() * (city_gene_pop_dic.Count() - 1) + 1) * iterations + count0] = "";

                ///更新每个城市中文化结构字典中的值 city_gene_pop_dic
                foreach (KeyValuePair<string, CityPopCulturalStructureClass> kvl in city_gene_pop_dic)
                {
                    string currentCity = kvl.Key;

                    string currentCity_cultureType = cityPopDic[currentCity].Culture_type;

                    foreach (KeyValuePair<string, PersonCount> kvl1 in kvl.Value.Crowd_cultural_structure_dic)
                    {
                        string native_culture_type = string.Format($"N_{currentCity_cultureType}_0");
                        string native_h_culture_type = string.Format($"H_{currentCity_cultureType}_0");


                        // N_C_0
                        if (kvl1.Key != native_culture_type && kvl1.Key.StartsWith("N"))
                        {
                            string[] culturalTypeArrays = kvl1.Key.Split('_');
                            culturalTypeArrays[0] = "H";
                            string culturalTypeName = string.Format($"{culturalTypeArrays[0]}_{culturalTypeArrays[1]}_{culturalTypeArrays[2]}");
                            kvl.Value.Crowd_cultural_structure_dic[culturalTypeName].persons += kvl.Value.Crowd_cultural_structure_dic[kvl1.Key].persons;
                            kvl.Value.Crowd_cultural_structure_dic[kvl1.Key].persons = 0;
                        }

                        if (kvl1.Key == native_h_culture_type)
                        {
                            kvl.Value.Crowd_cultural_structure_dic[native_culture_type].persons += kvl.Value.Crowd_cultural_structure_dic[native_h_culture_type].persons;
                            kvl.Value.Crowd_cultural_structure_dic[native_h_culture_type].persons = 0;
                        }
                    }
                }

                ///区域内部文化传播与涵化

                //double transform_rate_H = 0.3;
                double transform_rate_H = Convert.ToDouble(textBox3.Text);
                //double transform_rate_I = 0.2
                double transform_rate_I= Convert.ToDouble(textBox5.Text);
                //double transform_rate_A = 0.15;
                double transform_rate_A = Convert.ToDouble(textBox6.Text);
                //double contact_rate = 0.2;
                double contact_rate = Convert.ToDouble(textBox4.Text);

                /// 文化融合过程计算

                foreach (KeyValuePair<string, CityPopCulturalStructureClass> kvl in city_gene_pop_dic)
                {
                    string currentCity = kvl.Key;

                    string currentCity_cultureName = string.Format($"N_{cityPopDic[currentCity].Culture_type}_0");

                    string currentCity_singleCulturalName = cityPopDic[currentCity].Culture_type;

                    int Native_cultural_pop = kvl.Value.Crowd_cultural_structure_dic[currentCity_cultureName].persons; //当前城市本土文化总人口
                    int Current_city_pop_count = kvl.Value.Crowd_cultural_structure_dic.Sum(v => v.Value.persons); ; //当前城市总人口

                    Dictionary<string, double> Heterogeneous_cultural_pop_dic = new Dictionary<string, double>();
                    /*
                    foreach (KeyValuePair<string, PersonCount> kvh in kvl.Value.Crowd_cultural_structure_dic)
                    {
                        Heterogeneous_cultural_pop_dic[kvh.Key] = new double();
                        Heterogeneous_cultural_pop_dic[kvh.Key] = 0;
                    }
                    */

                    Dictionary<string, double> Integration_cultural_pop_dic = new Dictionary<string, double>();
                    /*
                    foreach (KeyValuePair<string, PersonCount> kvh in kvl.Value.Crowd_cultural_structure_dic)
                    {
                        Integration_cultural_pop_dic[kvh.Key] = new double();
                        Integration_cultural_pop_dic[kvh.Key] = 0;
                    }
                    */

                    Dictionary<string, double> Acculturation_cultural_pop_dic = new Dictionary<string, double>();
                    /*
                    foreach (KeyValuePair<string, PersonCount> kvh in kvl.Value.Crowd_cultural_structure_dic)
                    {
                        Acculturation_cultural_pop_dic[kvh.Key] = new double();
                        Acculturation_cultural_pop_dic[kvh.Key] = 0;
                    }
                    */

                    Dictionary<string, double> HeterogeneousToIntegration_cultural_pop_dic = new Dictionary<string, double>();
                    /*
                    foreach (KeyValuePair<string, PersonCount> kvh in kvl.Value.Crowd_cultural_structure_dic)
                    {
                        HeterogeneousToIntegration_cultural_pop_dic[kvh.Key] = new double();
                        HeterogeneousToIntegration_cultural_pop_dic[kvh.Key] = 0;
                    }
                    */

                    Dictionary<string, double> IntegrationToAcculturation_cultural_pop_dic = new Dictionary<string, double>();
                    /*
                    foreach (KeyValuePair<string, PersonCount> kvh in kvl.Value.Crowd_cultural_structure_dic)
                    {
                        IntegrationToAcculturation_cultural_pop_dic[kvh.Key] = new double();
                        IntegrationToAcculturation_cultural_pop_dic[kvh.Key] = 0;
                    }
                    */

                    foreach (KeyValuePair<string, PersonCount> kvl1 in kvl.Value.Crowd_cultural_structure_dic)
                    {


                        /// N和H到I
                        if (kvl1.Key.StartsWith("H"))
                        {

                            string[] CulturalTypeArrays1 = kvl1.Key.Split('_');
                            double CulturalStrength =1;
                            if (CulturalTypeArrays1[1]!= cityPopDic[currentCity].Culture_type)
                            {
                                string CulturalPair = string.Format($"{ CulturalTypeArrays1[1]}_{cityPopDic[currentCity].Culture_type}");
                                CulturalStrength = CulturalStrengthDic[CulturalPair];
                            }
                           
                            int Heterogeneous_cultural_pop = kvl1.Value.persons;
                            
                            double p1 = CulturalStrength*transform_rate_H * contact_rate * Native_cultural_pop * (Heterogeneous_cultural_pop * 1.0 / Current_city_pop_count);
                            
                            string cd_name1 = string.Format($"I_{currentCity_singleCulturalName}_{kvl1.Key.Split('_')[1]}");
                            string cd_name2 = string.Format($"I_{kvl1.Key.Split('_')[1]}_{currentCity_singleCulturalName}");

                            if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(cd_name1))
                            {
                                Heterogeneous_cultural_pop_dic[cd_name1] = p1;
                            }
                            if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(cd_name2))
                            {
                                Heterogeneous_cultural_pop_dic[cd_name2] = p1;
                            }
                            
                        }
                        /// N和I到I
                        if (kvl1.Key.StartsWith("I"))
                        {
                            

                            int Integration_cultural_pop = kvl1.Value.persons;

                            double p2 = transform_rate_I * contact_rate * Native_cultural_pop * (Integration_cultural_pop / Current_city_pop_count* 1.0) ;

                            string single_cd_name1 = kvl1.Key.Split('_')[1];
                            string single_cd_name2 = kvl1.Key.Split('_')[2];

                            string[] single_cd_names = { single_cd_name1, single_cd_name2 };

                            string newKey1 = null;
                            string newKey2 = null;

                            if (currentCity_singleCulturalName == single_cd_name1)
                            {
                                newKey1 = string.Format($"I_{currentCity_singleCulturalName}_{single_cd_name2}");
                                newKey2 = string.Format($"I_{single_cd_name2}_{currentCity_singleCulturalName}");

                                string CulturalPair = string.Format($"{ single_cd_name2}_{cityPopDic[currentCity].Culture_type}");
                                double CulturalStrength = CulturalStrengthDic[CulturalPair];


                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey1))
                                {
                                    Integration_cultural_pop_dic[newKey1] = p2* CulturalStrength;
                                }
                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey2))
                                {
                                    Integration_cultural_pop_dic[newKey2] = p2* CulturalStrength;
                                }
                            }

                            if (currentCity_singleCulturalName == single_cd_name2)
                            {
                                newKey1 = string.Format($"I_{currentCity_singleCulturalName}_{single_cd_name1}");
                                newKey2 = string.Format($"I_{single_cd_name1}_{currentCity_singleCulturalName}");

                                string CulturalPair = string.Format($"{ single_cd_name1}_{cityPopDic[currentCity].Culture_type}");
                                double CulturalStrength = CulturalStrengthDic[CulturalPair];

                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey1))
                                {
                                    Integration_cultural_pop_dic[newKey1] = p2* CulturalStrength;
                                }
                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey2))
                                {
                                    Integration_cultural_pop_dic[newKey2] = p2* CulturalStrength;
                                }
                            }

                            if (currentCity_singleCulturalName != single_cd_name1 && currentCity_singleCulturalName != single_cd_name2)
                            {
                                int selIndex = new Random().Next(0, 2);

                                newKey1 = string.Format($"I_{single_cd_names[selIndex]}_{currentCity_singleCulturalName}");
                                newKey2 = string.Format($"I_{currentCity_singleCulturalName}_{single_cd_names[selIndex]}");

                                string CulturalPair = string.Format($"{ single_cd_names[selIndex]}_{cityPopDic[currentCity].Culture_type}");
                                double CulturalStrength = CulturalStrengthDic[CulturalPair];

                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey1))
                                {
                                    Integration_cultural_pop_dic[newKey1] = p2* CulturalStrength;
                                }
                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey2))
                                {
                                    Integration_cultural_pop_dic[newKey2] = p2* CulturalStrength;
                                }
                            }
                        }
                        /// N和A到I
                        if (kvl1.Key.StartsWith("A"))
                        {
                            int Acculturation_cultural_pop = kvl1.Value.persons;

                            double p2 = transform_rate_A * contact_rate * Native_cultural_pop * (Acculturation_cultural_pop / Current_city_pop_count) * 1.0;

                            string single_cd_name1 = kvl1.Key.Split('_')[1];
                            string single_cd_name2 = kvl1.Key.Split('_')[2];

                            string[] single_cd_names = { single_cd_name1, single_cd_name2 };

                            string newKey1 = null;
                            string newKey2 = null;

                            if (currentCity_singleCulturalName == single_cd_name1)
                            {
                                newKey1 = string.Format($"A_{currentCity_singleCulturalName}_{single_cd_name2}");
                                newKey2 = string.Format($"A_{single_cd_name2}_{currentCity_singleCulturalName}");

                                string CulturalPair = string.Format($"{ single_cd_name2}_{cityPopDic[currentCity].Culture_type}");
                                double CulturalStrength = CulturalStrengthDic[CulturalPair];

                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey1))
                                {
                                    Acculturation_cultural_pop_dic[newKey1] = p2* CulturalStrength;
                                }
                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey2))
                                {
                                    Acculturation_cultural_pop_dic[newKey2] = p2* CulturalStrength;
                                }
                            }

                            if (cityPopDic[currentCity].Culture_type == single_cd_name2)
                            {
                                newKey1 = string.Format($"A_{currentCity_singleCulturalName}_{single_cd_name1}");
                                newKey2 = string.Format($"A_{single_cd_name1}_{currentCity_singleCulturalName}");

                                string CulturalPair = string.Format($"{ single_cd_name1}_{cityPopDic[currentCity].Culture_type}");
                                double CulturalStrength = CulturalStrengthDic[CulturalPair];

                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey1))
                                {
                                    Acculturation_cultural_pop_dic[newKey1] = p2* CulturalStrength;
                                }
                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey2))
                                {
                                    Acculturation_cultural_pop_dic[newKey2] = p2* CulturalStrength;
                                }
                            }

                            if (cityPopDic[currentCity].Culture_type != single_cd_name1 && cityPopDic[currentCity].Culture_type != single_cd_name2)
                            {
                                int selIndex = new Random().Next(0, 2);

                                newKey1 = string.Format($"A_{single_cd_names[selIndex]}_{currentCity_singleCulturalName}");
                                newKey2 = string.Format($"A_{currentCity_singleCulturalName}_{single_cd_names[selIndex]}");

                                string CulturalPair = string.Format($"{ single_cd_names[selIndex]}_{cityPopDic[currentCity].Culture_type}");
                                double CulturalStrength = CulturalStrengthDic[CulturalPair];

                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey1))
                                {
                                    Acculturation_cultural_pop_dic[newKey1] = p2* CulturalStrength;
                                }
                                if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(newKey2))
                                {
                                    Acculturation_cultural_pop_dic[newKey2] = p2* CulturalStrength;
                                }
                            }
                        }
                        /// H到I
                        if (kvl1.Key.StartsWith("H"))
                        {
                            //double HtoI_rate = 0.2;
                            double HtoI_rate = Convert.ToDouble(textBox8.Text);

                            int Heterogeneous_cultural_pop = kvl1.Value.persons;

                            double CulturalStrength = 1;
                            if(cityPopDic[currentCity].Culture_type!= kvl1.Key.Split('_')[1])
                            {
                                string CulturalPair = string.Format($"{cityPopDic[currentCity].Culture_type}_{kvl1.Key.Split('_')[1]}");
                                CulturalStrength = CulturalStrengthDic[CulturalPair];
                            }

                            double Convert_pop = Heterogeneous_cultural_pop * HtoI_rate* CulturalStrength;

                            string cd_name1 = string.Format($"I_{currentCity_singleCulturalName}_{kvl1.Key.Split('_')[1]}");
                            string cd_name2 = string.Format($"I_{kvl1.Key.Split('_')[1]}_{currentCity_singleCulturalName}");

                            

                            if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(cd_name1))
                            {
                                HeterogeneousToIntegration_cultural_pop_dic[cd_name1] = Convert_pop;
                            }
                            if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(cd_name2))
                            {
                                HeterogeneousToIntegration_cultural_pop_dic[cd_name2] = Convert_pop;
                            }
                        }
                        /// I到A
                        if (kvl1.Key.StartsWith("I"))
                        {
                            //double ItoA_rate = 0.15;
                            double ItoA_rate = Convert.ToDouble(textBox7.Text);

                            int Heterogeneous_cultural_pop = kvl1.Value.persons;

                            double Convert_pop = Heterogeneous_cultural_pop * ItoA_rate;

                            string cd_name1 = string.Format($"A_{currentCity_singleCulturalName}_{kvl1.Key.Split('_')[1]}");
                            string cd_name2 = string.Format($"A_{kvl1.Key.Split('_')[1]}_{currentCity_singleCulturalName}");

                            if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(cd_name1))
                            {
                                IntegrationToAcculturation_cultural_pop_dic[cd_name1] = Convert_pop;
                            }
                            if (kvl.Value.Crowd_cultural_structure_dic.ContainsKey(cd_name2))
                            {
                                IntegrationToAcculturation_cultural_pop_dic[cd_name2] = Convert_pop;
                            }
                        }

                    }
                    ///归纳涵化后人数
                    foreach (KeyValuePair<string, double> kvl_sub in Heterogeneous_cultural_pop_dic)
                    {
                        kvl.Value.Crowd_cultural_structure_dic[kvl_sub.Key].persons += Convert.ToInt32(kvl_sub.Value);
                        
                        //textBox2.Text = kvl_sub.Key + ":" + Convert.ToInt32(kvl_sub.Value).ToString();
                        kvl.Value.Crowd_cultural_structure_dic[string.Format($"N_{currentCity_singleCulturalName}_0")].persons -= Convert.ToInt32(kvl_sub.Value);
                    }

                    foreach (KeyValuePair<string, double> kvl_sub in Integration_cultural_pop_dic)
                    {
                        kvl.Value.Crowd_cultural_structure_dic[kvl_sub.Key].persons += Convert.ToInt32(kvl_sub.Value);
                        
                        kvl.Value.Crowd_cultural_structure_dic[string.Format($"N_{currentCity_singleCulturalName}_0")].persons -= Convert.ToInt32(kvl_sub.Value);
                    }

                    foreach (KeyValuePair<string, double> kvl_sub in Acculturation_cultural_pop_dic)
                    {
                        kvl.Value.Crowd_cultural_structure_dic[kvl_sub.Key].persons += Convert.ToInt32(kvl_sub.Value);

                        kvl.Value.Crowd_cultural_structure_dic[string.Format($"N_{currentCity_singleCulturalName}_0")].persons -= Convert.ToInt32(kvl_sub.Value);
                    }

                    foreach (KeyValuePair<string, double> kvl_sub in HeterogeneousToIntegration_cultural_pop_dic)
                    {
                        kvl.Value.Crowd_cultural_structure_dic[kvl_sub.Key].persons += Convert.ToInt32(kvl_sub.Value);

                        string cd_type_1 = kvl_sub.Key.Split('_')[1];
                        string cd_type_2 = kvl_sub.Key.Split('_')[2];
                        string h_cd = null;

                        if (currentCity_singleCulturalName == cd_type_1)
                        {
                            h_cd = cd_type_2;
                        }
                        else
                        {
                            h_cd = cd_type_1;
                        }

                        kvl.Value.Crowd_cultural_structure_dic[string.Format($"H_{h_cd}_0")].persons -= Convert.ToInt32(kvl_sub.Value);
                    }

                    foreach (KeyValuePair<string, double> kvl_sub in IntegrationToAcculturation_cultural_pop_dic)
                    {
                        kvl.Value.Crowd_cultural_structure_dic[kvl_sub.Key].persons += Convert.ToInt32(kvl_sub.Value);

                        string cd_type_1 = kvl_sub.Key.Split('_')[1];
                        string cd_type_2 = kvl_sub.Key.Split('_')[2];
                        string a_cd = string.Format($"I_{cd_type_1}_{cd_type_2}");

                        kvl.Value.Crowd_cultural_structure_dic[a_cd].persons -= Convert.ToInt32(kvl_sub.Value);
                    }


                    cityPopDic[currentCity].City_pop = kvl.Value.Crowd_cultural_structure_dic.Sum(v => v.Value.persons);
                    cityPopDic[currentCity].Removed_pop = kvl.Value.Crowd_cultural_structure_dic["R_0_0"].persons;

                    // 遍历每个城市的人口情况,存入str
                    int j = 1;
                    foreach (KeyValuePair<string, CityPopCulturalStructureClass> kvlwrite in city_gene_pop_dic)
                    {
                        string currentcity = kvlwrite.Key;
                        string[] outputs = new string[kvlwrite.Value.Crowd_cultural_structure_dic.Count + 1];
                        outputs[0] = currentcity;
                        int i = 1;
                        foreach (KeyValuePair<string, PersonCount> kvl1 in kvlwrite.Value.Crowd_cultural_structure_dic)
                        {
                            outputs[i] = kvl1.Value.persons.ToString();
                            i++;
                        }
                        str[j+iterations*(city_gene_pop_dic.Count+1)] = string.Join(",", outputs);
                        j++;
                    }
                }
                textBox1.Text=($"round{iterations}……");
                progressBar1.Value ++;
                progressBar1.Update();
            }

            //显示
            string output = string.Format($"{Application.StartupPath}\\data\\output_data\\{"countryoutput.csv"}");
            string output_popflow = string.Format($"{Application.StartupPath}\\data\\output_data\\{"countryoutput_popflow.csv"}");
           
            ///culture_type:文化类型，每个城市顺序相同
            string[] culture_type = new string[city_gene_pop_dic.First().Value.Crowd_cultural_structure_dic.Count+1];
            culture_type[0] = "CityID";
            int k = 1;
            foreach(KeyValuePair<string,PersonCount> kvl in city_gene_pop_dic.First().Value.Crowd_cultural_structure_dic)
            {
                culture_type[k] = kvl.Key;
                k++;
            }

            ///把文化类型放在输出数字第一个元素，即csv第一行
            str[0] = string.Join(",", culture_type);

            System.IO.File.WriteAllLines(output, str);

            //输出流数据

            string[] culturetype = new string[flow_output.First().Value.Count() + 1];
            int countC = 1;
            foreach (KeyValuePair<string, int> kvl in flow_output.First().Value)
            {
                culturetype[countC] = kvl.Key;
                countC++;
            }
            culturetype[0] = "";
            str_flow[0] = string.Join(",", culturetype);


            System.IO.File.WriteAllLines(output_popflow,str_flow);

            textBox1.Text = "OK";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "10";
            textBox3.Text = "0.3";
            textBox4.Text = "0.002";
            textBox5.Text = "0.2";
            textBox6.Text = "0.15";
            textBox7.Text = "0.15";
            textBox8.Text = "0.2";
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
