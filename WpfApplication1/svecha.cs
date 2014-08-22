using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.IO;

// using S#
using Ecng.Common;
using Ecng.ComponentModel;
using Ecng.Xaml;
using Ecng.Serialization;
using Ecng.Collections;
using StockSharp.Algo;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Candles.Compression;
using StockSharp.Algo.Strategies;
using StockSharp.Algo.Storages;
using StockSharp.BusinessEntities;
using StockSharp.Quik;
using StockSharp.Logging;

using VolumeAnalyzer;

namespace VolumeAnalyzer
{
    public class Svecha
    {
        // свойство - начальная дата свечи
        public DateTime startCandleTime { get; set; }

        // свойство - конечная дата свечи
        public DateTime endCandleTime { get; set; }

        // свойство - объем на покупку свечи
        public int buyVolumeSvechi { get; set; }

        // свойство - объем на продажу свечи
        public int sellVolumeSvechi { get; set; }

        // свойство - дельта свечи
        public int deltaSvechi { get; set; }

        // свойство - дельта свечи
        public double percent { get; set; }

        // свойство - объем свечи
        public int volumeSvechi { get; set; }

        // свойство - список сделок свечи
        public List<Trade> sdelkiSvechi { get; set; }

        // свойство - список кластеров свечи
        public List<Cluster> clusteruSvechi { get; set; }


        // конструктор класса
        public Svecha()
        {
            // создаем переменную список сделок свечи
            sdelkiSvechi = new List<Trade>();

            // создаем переменную список кластеров свечи
            clusteruSvechi = new List<Cluster>();
        }

        // метод построения свечи
        public void postroitSvechu(Trade _trade, decimal _step)
        {
            // переменная "существует ли кластер"
            bool clusterExists=false;

            // для каждого кластера в кластерах свечи
            foreach (Cluster cl in clusteruSvechi)
            {
                // если цена сделки больше или равна минимальной цене кластера и цена сделки меньше максимальной цены кластера
                if (_trade.Price >= cl.minPrice && _trade.Price < cl.maxPrice)
                    // значит кластер существует - переменная флаг вкл.
                    clusterExists = true;
            }

            // условие если кластер не существует
            if (clusterExists == false)
            {
                // создаем новый кластер
                Cluster newCluster = new Cluster();

                // добавляем в сделки кластера сделку
                newCluster.sdelkiClustera.Add(_trade);

                // выставляем минимальную цену кластера
                newCluster.minPrice = 0;

                // выставляем максимальную цену кластера (минимальная цена + шаг инструмента)
                newCluster.maxPrice = 0 + _step;

                // выполняем даный цикл пока НЕ (цена сделки больше или равно минимальной цене кластера и цена сделки меньше максимальной цены кластера)
                while ( !(_trade.Price >= newCluster.minPrice && _trade.Price < newCluster.maxPrice))
                {
                    // добавляем к минимальной и максимальной цене кластера шаг инструмента
                    newCluster.minPrice += _step;
                    newCluster.maxPrice += _step;
                }

                // если направление сделки на покупку
                if (_trade.OrderDirection == OrderDirections.Buy)
                {
                    // добавляем объем сделки к объему на покупку кластера
                    newCluster.buyVolume += (int)_trade.Volume;

                    // добавляем объем сделки к объему на покупку кластера
                    newCluster.delta += (int)_trade.Volume;

                    // добавляем объем сделки к объему на покупку свечи
                    buyVolumeSvechi += (int)_trade.Volume;

                    // добавляем объем сделки к дельте свечи
                    deltaSvechi += (int)_trade.Volume;
                }

                // если направление сделки на продажу
                if (_trade.OrderDirection == OrderDirections.Sell)
                {
                    // добавляем объем сделки к объему на продажу кластера
                    newCluster.sellVolume += (int)_trade.Volume;

                    // добавляем объем сделки к объему на продажу кластера
                    newCluster.delta -= (int)_trade.Volume;

                    // добавляем объем сделки к объему на продажу свечи
                    sellVolumeSvechi += (int)_trade.Volume;

                    // добавляем объем сделки к дельте свечи
                    deltaSvechi -= (int)_trade.Volume;
                }

                // добавляем объем сделки к общему объему кластера
                newCluster.clusterVolume += (int)_trade.Volume;

                // пересчитываем баланс кластера
                newCluster.clusterPercent = ((double)newCluster.delta / (double)newCluster.clusterVolume) * (double)100;

                newCluster.clusterPercent = Math.Round(newCluster.clusterPercent, 2);

                // добавляем объем сделки к общему объему свечи
                volumeSvechi += (int)_trade.Volume;

                // добавляем новый кластер в свечу
                clusteruSvechi.Add(newCluster);
            }

            // если кластер существует
            if (clusterExists==true)
            {
                // для каждого кластера в списке кластеров свечи
                foreach (Cluster cl in clusteruSvechi)
                {
                    // если цена сделки больше или равна минимальной цене кластера и цена сделки меньше или равна максимальной цене кластера
                    if (_trade.Price >= cl.minPrice && _trade.Price < cl.maxPrice)
                    {
                        // добавляем сделку в список сделк кластера 
                        cl.sdelkiClustera.Add(_trade);

                        // если сделка на покупку
                        if (_trade.OrderDirection == OrderDirections.Buy)
                        {
                            // добавляем объем сделки к объему на покупку кластера
                            cl.buyVolume += (int)_trade.Volume;

                            // добавляем объем сделки к общей дельте кластера
                            cl.delta += (int)_trade.Volume;

                            // добавляем объем сделки к общему объему на покупку свечи
                            buyVolumeSvechi += (int)_trade.Volume;

                            // добавляем объем сделки к общей дельте свечи
                            deltaSvechi += (int)_trade.Volume;
                        }

                        // если сделка на продажу
                        if (_trade.OrderDirection == OrderDirections.Sell)
                        {
                            // добавляем объем сделки к общему объему на продажу
                            cl.sellVolume += (int)_trade.Volume;

                            // добавляем объем сделки к общей дельте кластера
                            cl.delta -= (int)_trade.Volume;

                            // добавляем объем сделки к общему объему на продажу свечи
                            sellVolumeSvechi += (int)_trade.Volume;

                            // добавляем объем сделки к общей дельте свечи
                            deltaSvechi -= (int)_trade.Volume;
                        }

                        // добавляем объем сделки к общему объему кластера
                        cl.clusterVolume += (int)_trade.Volume;

                        // пересчитываем баланс кластера
                        cl.clusterPercent = ((double)cl.delta / (double)cl.clusterVolume) * (double)100;

                        cl.clusterPercent = Math.Round(cl.clusterPercent, 2);

                        // добавляем объем сделки к общему объему свечи
                        volumeSvechi += (int)_trade.Volume;

                    }
                }
            }

            percent = ((double)deltaSvechi / (double)volumeSvechi) * (double)100;

            percent = Math.Round(percent, 2);

            //List<Cluster> SortedList = clusteruSvechi.OrderByDescending(cluster => cluster.minPrice).ToList();
            clusteruSvechi = clusteruSvechi.OrderByDescending(cluster => cluster.minPrice).ToList();

        }


    }
}
