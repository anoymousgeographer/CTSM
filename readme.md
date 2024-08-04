### Spatiotemporal process simulation model for cultural transmission and acculturation

#### 1. Preparing the Running Environment
Here's a program written in C# to simulate the process of cultural transmission and acculturation, use a C# compiler like Visual Studio to run the CTSMv2.sln program. 

#### 2. Preparing the Data
Before running the program, users need to prepare the relevant data, which should be stored in the path ``CTSM\bin\Debug\data\source_data\country``.In this path, there are four table files stored: ``city_culture_pop.csv``, ``city_pop_flow.csv``, ``city_pop.csv``, and ``cultural strength.csv``. 
It is worth noting that if the user does not want to pre-construct the cultural population structure of the area and prefers the simplest condition (i.e., each area only has the regional culture population and removed), they **do not need** to construct the ``city_culture_pop.csv`` table or check the "**Read the initial cultural population data**" option on the program page.
The four tables' meanings are as follows:
- ``city_culture_pop.csv``(optional): The table stores the number of each type of cultural population in the smallest geographical units within the study area. The "types of culture" include acculturation levels and cultural types. For example, in the sample data, "N_L_0" represents the native culture L, and "A_B_D" represents an acculturation culture combining culture B and D. In this program, each integration/acculturation culture is represented as "acculturation level_culture1_culture2". The acculturation levels include I (integration) and A (acculturation), while culture1 and culture2 are letters representing each regional culture. For example, the integration culture of culture C and culture D is denoted as "I_B_D", and the acculturation culture is denoted as "A_B_D". For a single regional culture, the position of culture2 will be a placeholder 0. The values for acculturation level include N (native) and H (heterogeneous). For example, native culture C is denoted as "N_C_0," and heterogeneous culture D is denoted as "H_D_0". And "R_0_0" represents the removed. The first column of this table contains the identification numbers for each city.
<br>
- ``city_pop.csv``contains four columns, which are, in order: the city's ID, the city's total population, the city's regional cultural type, and the number of removed individuals in the city.
<br>
- ``city_pop_flow.csv``is a table that stores the number of population movements between cities. The first column is the ID of the origin city, the second column is the ID of the destination city, and the last column is the number of moving individuals.
<br>
- ``cultural strength.csv``stores the cultural antibodies (CA) between each pair of cultures.

#### 3. Run the program
Run the program and click the "Get default parameters" button on the panel. The program will automatically fill in the default parameters, which may not be suitable and might need to be adjusted by the user. Decide whether to check the "Read the initial cultural population data" option based on the prepared data. Adjust the "Iterations" option according to your needs to set the number of iterations. After adjusting the parameters, click "Start" to begin the simulation.<br>
The output data will be stored in the path CTSM\bin\Debug\data\output_data, and it will include:
- A time series table of cultural populations by type for each city.
- A time series table of cultural population movements by type between cities.
