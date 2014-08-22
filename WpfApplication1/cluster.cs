using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockSharp.BusinessEntities;

namespace VolumeAnalyzer
{
    public class Cluster
    {
        // свойство - максимальная цена кластера
        public decimal maxPrice { get; set; }

        // свойство - минимальная цена кластера
        public decimal minPrice { get; set; }

        // свойство - список сделок кластера
        public List<Trade> sdelkiClustera { get; set;}

        // свойство - объем на покупку кластера
        public int buyVolume { get; set; }

        // свойство - объем на продажу кластера
        public int sellVolume { get; set; }

        // свойство - дельта кластера
        public int delta { get; set; }

        // свойство - дельта кластера
        public double clusterPercent { get; set; }

        // свойство - объем кластера
        public int clusterVolume { get; set; }

        // конструктор кластера
        public Cluster()
        {
            // создаем переменую список сделок кластера
            sdelkiClustera = new List<Trade>();
        }


    }
}
