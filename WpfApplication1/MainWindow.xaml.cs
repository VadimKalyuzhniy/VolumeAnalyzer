using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.DataVisualization;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.IO;
using System.Drawing;

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net.Sockets;

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



namespace VolumeAnalyzer
{

    public partial class MainWindow : Window
    {

        // квик трейдер
        public QuikTrader _trader;

        // инструмент
        public Security _security;

        // портфель
        public Portfolio _portfolio;

        // дата начала отрисовки свечек
        public DateTime startDate;

        // дата окончания отрисовки свечек
        public DateTime endDate;

        // список свечек по инструменту
        public List<Svecha> spisokSvechek = new List<Svecha>();

        // список сделок по инструменту
        public List<Trade> sdelkiInstrumenta = new List<Trade>();

        // шаг инструмента
        public decimal step;

        // Переменная содержит путь к БД
        const string _adminDB = @"Data Source=sportbook.no-ip.biz\SQLEXPRESS,1433;Network Library=DBMSSOCN;Initial Catalog=VolumeAnalyzer;User ID=test;Password=test";
        //const string _adminDB = @"Data Source=(local)\SQLEXPRESS,1433;Network Library=DBMSSOCN;Initial Catalog=VolumeAnalyzer;User ID=test;Password=test";

        public SqlConnection connSingle = new SqlConnection(_adminDB);

        public Trade SingleTrade = new Trade();

        public DateTime startClickTime;

        public MainWindow()
        {
            InitializeComponent();

            // центруем главное окно
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // находим путь к квику и вписываем его в поле
            quikPath.Text = QuikTerminal.GetDefaultPath();

            // выставляем шаг инструмента из поля главного окна
            step = Convert.ToDecimal(userStep.Text);

            startDatePicker.SelectedDate = DateTime.Today;
            endDatePicker.SelectedDate = DateTime.Today.AddDays(1);

            //////////////////////////////////// charting //////////////////////////////////////



            chart.ChartAreas.Add(new ChartArea("NewChartArea"));

            chart.ChartAreas["NewChartArea"].AxisX.LabelStyle.Format = "dd/MM HH:mm";

            chart.ChartAreas["NewChartArea"].AxisX.IsStartedFromZero = false;
            chart.ChartAreas["NewChartArea"].AxisY.IsStartedFromZero = false;

            chart.ChartAreas["NewChartArea"].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas["NewChartArea"].CursorY.IsUserSelectionEnabled = true;

            chart.ChartAreas["NewChartArea"].CursorX.IsUserEnabled = true;
            chart.ChartAreas["NewChartArea"].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas["NewChartArea"].CursorY.IsUserEnabled = true;
            chart.ChartAreas["NewChartArea"].CursorY.IsUserSelectionEnabled = true;
            chart.ChartAreas["NewChartArea"].AxisX.ScaleView.Zoomable = true;
            chart.ChartAreas["NewChartArea"].AxisX.ScrollBar.IsPositionedInside = true;
            chart.ChartAreas["NewChartArea"].AxisY.ScaleView.Zoomable = true;
            chart.ChartAreas["NewChartArea"].AxisY.ScrollBar.IsPositionedInside = true;


            Series clusters = new Series("Cluster");
            clusters.ChartType = SeriesChartType.RangeColumn;

            chart.Series.Add(clusters);

            chart.Series["Cluster"].ChartArea = "NewChartArea";

            chart.Series["Cluster"].IsValueShownAsLabel = true;

            chart.Series["Cluster"].XValueType = ChartValueType.DateTime;

            chart.Series["Cluster"].BorderColor = System.Drawing.Color.Black;
            chart.Series["Cluster"].BorderWidth = 1;

            ////////////////////////////////////////////////////////////  levuy chart ////////////////////////////////////////////////////


            chart.ChartAreas.Add(new ChartArea("NewChartArea2"));


            chart.ChartAreas["NewChartArea2"].AxisX.IsStartedFromZero = false;
            chart.ChartAreas["NewChartArea2"].AxisY.IsStartedFromZero = false;

            chart.ChartAreas["NewChartArea2"].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas["NewChartArea2"].CursorY.IsUserSelectionEnabled = true;

            chart.ChartAreas["NewChartArea2"].CursorX.IsUserEnabled = true;
            chart.ChartAreas["NewChartArea2"].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas["NewChartArea2"].CursorY.IsUserEnabled = true;
            chart.ChartAreas["NewChartArea2"].CursorY.IsUserSelectionEnabled = true;
            chart.ChartAreas["NewChartArea2"].AxisX.ScaleView.Zoomable = true;
            chart.ChartAreas["NewChartArea2"].AxisX.ScrollBar.IsPositionedInside = true;
            chart.ChartAreas["NewChartArea2"].AxisY.ScaleView.Zoomable = true;
            chart.ChartAreas["NewChartArea2"].AxisY.ScrollBar.IsPositionedInside = true;

            chart.ChartAreas["NewChartArea2"].AxisY.CustomLabels.Add((double)0, (double)0, "");

            Series leftBuyVolume = new Series("leftBuyVolume");
            leftBuyVolume.ChartType = SeriesChartType.StackedBar;
            chart.Series.Add(leftBuyVolume);
            chart.Series["leftBuyVolume"].ChartArea = "NewChartArea2";
            chart.Series["leftBuyVolume"].IsValueShownAsLabel = true;


            Series leftSellVolume = new Series("leftSellVolume");
            leftSellVolume.ChartType = SeriesChartType.StackedBar;
            chart.Series.Add(leftSellVolume);
            chart.Series["leftSellVolume"].ChartArea = "NewChartArea2";
            chart.Series["leftSellVolume"].IsValueShownAsLabel = true;


            ////////////////////////////////////////////// niz volume ///////////////////////////////////////////////////


            // Все графики находятся в пределах области построения ChartArea, создадим ее
            chart.ChartAreas.Add(new ChartArea("NewChartArea3"));

            chart.ChartAreas["NewChartArea3"].AxisX.LabelStyle.Format = "dd/MM HH:mm";

            Series nizVolumeBuy = new Series("nizVolumeBuy");
            nizVolumeBuy.ChartType = SeriesChartType.StackedColumn;

            chart.Series.Add(nizVolumeBuy);
            chart.Series["nizVolumeBuy"].ChartArea = "NewChartArea3";

            // Show point labels
            chart.Series["nizVolumeBuy"].IsValueShownAsLabel = true;

            chart.Series["nizVolumeBuy"].XValueType = ChartValueType.DateTime;


            ///////////////////////////////////////////////////////////////////////////////////////////////////

            Series nizVolumeSell = new Series("nizVolumeSell");
            nizVolumeSell.ChartType = SeriesChartType.StackedColumn;


            chart.Series.Add(nizVolumeSell);
            chart.Series["nizVolumeSell"].ChartArea = "NewChartArea3";

            // Show point labels
            chart.Series["nizVolumeSell"].IsValueShownAsLabel = true;

            chart.Series["nizVolumeSell"].XValueType = ChartValueType.DateTime;


            //////////////////////////////////// charting //////////////////////////////////////

        }

        // Кнопка "Подключение" - Метод подключения к квику
        private void connButton_Click(object sender, RoutedEventArgs e)
        {

            // если трейдер не подключен
            if (_trader == null)
            {
                //создаем нового трейдера
                _trader = new QuikTrader(quikPath.Text);

                //Подписываемся на событие появления новых портфелей и добавляем их в ComboBox Portfolios
                _trader.NewPortfolios += portfolios => this.GuiAsync(() => { userPortfolios.ItemsSource = _trader.Portfolios; });

                //Подписываемся на событие появления новых инстументов и добавляем их в ComboBox Securities
                _trader.NewSecurities += securities => this.GuiAsync(() => { userSecurities.ItemsSource = _trader.Securities; });

            }

            // подключаем квик
            _trader.Connect();

            //Начинаем Экспорт данных
            _trader.StartExport(); //получение он-лайн данных из квика Инструменты, Заявки, Портфели и так далее

            // меняем цвет кнопки
            connButton.Background = System.Windows.Media.Brushes.LightPink;

        }

        // метод нажатия на кнопку старт
        private void StrategyStartButton_Click(object sender, RoutedEventArgs e)
        {
            // очищаем стандартный контейнер правил от всех правил
            MarketRuleHelper.DefaultRuleContainer.Rules.Clear();

            startDate = startDatePicker.SelectedDate.GetValueOrDefault();
            endDate = endDatePicker.SelectedDate.GetValueOrDefault();

            // вызываем метод создания свечей
            sozdatSvechi();

            // создаем новый список сделок инструмента
            sdelkiInstrumenta = new List<Trade>();

            // если стоит флаг брать исторические сделки - то вызываем метод получения исторических сделок в общий список сделок инструмента
            if (takeTradesCB.IsChecked == true && startDate < DateTime.Today)
                takeHistoryTradesFaster();

            // если стоит флаг отправлять сделки в БД
            if (sendTradesDB.IsChecked == true)
            {
                // вызываем метод отправки сделки в БД
                sendAllTradesToDBbulk(_trader);
            }
            
            // создаем екземпляр типа SQL и присваиваем ему путь к БД
            try
            {
                // Пытаемся открыть БД
                if (connSingle.State == ConnectionState.Closed && sendTradesDB.IsChecked == true)
                connSingle.Open();
            }
            catch (Exception)
            {
                // если не получилось коннект к БД:ставим флаг, выдаем сообщение, возвращаем флаг
                System.Windows.MessageBox.Show("Ошибка подключения к БД (connSingle)");
                connSingle.Close();
            }

            // создаем правило на событие появления новой сделки по инструменту
            _security.WhenNewTrades()
            .Do(() =>
            {
                // добавляем в список сделок по инструменту последнюю сделку по инструменту
                sdelkiInstrumenta.Add(_security.LastTrade);

                // вызываем метод добавления последней сделки по инструменту в список свечек
                dobavitSdelku(_security.LastTrade);

                this.GuiAsync(() =>
                {
                    // вызываем метод отрисовки свеч
                    otrisovatSvechi();

                });

                this.GuiAsync(() =>
                {
                    // если стоит флаг отправлять сделки в БД
                    if (sendTradesDB.IsChecked == true)
                    {
                        // вызываем метод отправки сделки в БД
                        // sendTradeToDB(_security.LastTrade, SingleTrade, startClickTime);
                    }

                });

            })
            .Apply();

            // для каждой сделки в списке сделок квиктрейдера
            foreach (Trade trd in _trader.Trades)
            {
                // если эта сделка по требуемому инструменту,...
                if (trd.Security == _security)
                {

                    // если это не такая же сделка которая была импортирована с БД
                    //   if (sdelkiInstrumenta.Find(t => t.Id == trd.Id) == null)
                    //   {
                    //... то добавляем сделку в список сделок по инструменту
                    sdelkiInstrumenta.Add(trd);

                    // и вызываем метод добавления последней сделки по инструменту в список свечек
                    dobavitSdelku(trd);
                    //   }
                }
            }

            // вызываем метод отрисовки свеч
            otrisovatSvechi();


        }

        // метод создания свеч
        public void sozdatSvechi()
        {
            // создаем переменную с текущей датой и присваиваем ей значение начальной даты
            DateTime curTime = startDate;

            // пересоздаем список свечек
            spisokSvechek = new List<Svecha>();

            // выполняем даный цикл
            do
            {
                // создаем новую свечу
                Svecha novayaSvecha = new Svecha();

                // присваиваем новой свече начальную дату
                novayaSvecha.startCandleTime = curTime;

                // создаем переменную с отрезком времени в минутах и присваиваем ему значения поля главного окна
                var otrezokVremeni = TimeSpan.FromMinutes(Convert.ToDouble(userTimeFrame.Text));

                // присваиваем новой свече конечную дату (начальная дата + отрезок времени)
                novayaSvecha.endCandleTime = curTime.Add(otrezokVremeni);

                if (novayaSvecha.startCandleTime.DayOfWeek != DayOfWeek.Saturday)
                {
                    if (novayaSvecha.startCandleTime.DayOfWeek != DayOfWeek.Sunday)
                    {
                        // добавляем созданую свечу в список сделок
                        spisokSvechek.Add(novayaSvecha);
                    }
                }

                // добавляем к переменной "текущая дата" отрезок времени
                curTime += otrezokVremeni;

                // пока текущая дата не меньше конечной даты
            } while (curTime < endDate);

        }

        // метод добавления исторических сделок с БД в общий список сделок
        public void takeHistoryTradesFaster()
        {

            List<DateTime> daysList = new List<DateTime>();

            DateTime tmpStartDate = startDate;

            while (tmpStartDate < _trader.Trades.FirstOrDefault<Trade>().Time)
            {

                daysList.Add(tmpStartDate);

                tmpStartDate = tmpStartDate.AddDays(1);

            }

            // создаем екземпляр типа SQL и присваиваем ему путь к БД
            SqlConnection conn = new SqlConnection(_adminDB);
            try
            {
                // Пытаемся открыть БД
                conn.Open();
            }
            catch (Exception)
            {
                // если не получилось коннект к БД:ставим флаг, выдаем сообщение, возвращаем флаг
                System.Windows.MessageBox.Show("Ошибка подключения к БД (получение исторческих сделок)");
                return;
            }

            foreach (DateTime _day in daysList)
            {

                int exist = 0;
                string tableCheck = "SELECT count(*) as IsExists FROM dbo.sysobjects where id = object_id('[dbo].[" + _security.ToString() + "_" + _day.Day.ToString() + "_" +
                _day.Month.ToString() + "_" + _day.Year.ToString() + "]')";

                SqlCommand cmd = new SqlCommand(tableCheck, conn);

                try
                {
                    exist = (Int32)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Не могу проверить существует ли таблица по инструменту: " + ex.Message);
                }

                if (exist == 1)
                {
                    // создаем экземпляр типа SQLCommand для получения сделок из БД
                    string tradesCmd = @"Select * from " + _security.ToString() + "_" + _day.Day.ToString() + "_" + _day.Month.ToString() + "_" + _day.Year.ToString();

                    DataTable dt = new DataTable();

                    dt = Read2<SqlConnection, SqlDataAdapter>(tradesCmd);

                    for (int i=0; i < dt.Rows.Count; i++)
                    {
                        Trade newTrade = new Trade();

                        newTrade.Id = (long)Convert.ToDecimal(dt.Rows[i][1]);

                        newTrade.Time = (DateTime)dt.Rows[i][2];

                        newTrade.Price = decimal.Parse(dt.Rows[i][3].ToString());

                        newTrade.Volume = (decimal)Convert.ToDecimal(dt.Rows[i][4]);

                        if ((int)Convert.ToInt32(dt.Rows[i][5]) == 0)
                            newTrade.OrderDirection = OrderDirections.Buy;
                        if ((int)Convert.ToInt32(dt.Rows[i][5]) == 1)
                            newTrade.OrderDirection = OrderDirections.Sell;

                        sdelkiInstrumenta.Add(newTrade);
                        
                        dobavitSdelku(newTrade);


                    }


                }

            }

            conn.Dispose();
        }

        public DataTable Read2<S, T>(string query)
            where S : IDbConnection, new()
            where T : IDbDataAdapter, IDisposable, new()
        {
            using (var conn = new S())
            {
                using (var da = new T())
                {
                    using (da.SelectCommand = conn.CreateCommand())
                    {
                        da.SelectCommand.CommandText = query;
                        da.SelectCommand.Connection.ConnectionString = _adminDB;
                        DataSet ds = new DataSet(); //conn is opened by dataadapter
                        da.Fill(ds);
                        return ds.Tables[0];
                    }
                }
            }
        }

        // метод добавления новой сделки БД
        public void sendTradeToDB(Trade newTrade, Trade _pastTrade, DateTime _startClickTime)
        {


                // проверяем наличие таблици и заносим результаты в переменную exist
                int exist = 0;
                string tableCheck = "SELECT count(*) as IsExists FROM dbo.sysobjects where id = object_id('[dbo].[" + _security.ToString() + "_" + newTrade.Time.Day.ToString() + "_" +
                    newTrade.Time.Month.ToString() + "_" + newTrade.Time.Year.ToString() + "]')";

                SqlCommand cmd = new SqlCommand(tableCheck, connSingle);

                try
                {
                    exist = (Int32)cmd.ExecuteScalar();
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Не могу проверить существует ли таблица по инструменту при отправке одиночной сделки");
                    connSingle.Close();
                    // очищаем стандартный контейнер правил от всех правил
                    MarketRuleHelper.DefaultRuleContainer.Rules.Clear();
                    return;
                }

                if (exist == 0)
                {
                    SqlCommand cmdCreateTable = new SqlCommand("CREATE TABLE " + _security.ToString() + "_" + newTrade.Time.Day + "_" +
                    +newTrade.Time.Month + "_" + newTrade.Time.Year +
                     " (porNomer decimal IDENTITY NOT NULL PRIMARY KEY" +
                      ", idSdelki decimal not null" +
                         ", vremyaSdelki datetime not null" +
                             ", price float not null" +
                             ", volume int not null" +
                              ", direction int not null);", connSingle);

                    cmdCreateTable.ExecuteNonQuery();

                }


            // проверка сделки на то есть ли она уже в БД?
            string tradeExist = "";
            SqlCommand checkTrade = new SqlCommand("Select * from " + _security.ToString() + "_" + newTrade.Time.Day + "_" +
                +newTrade.Time.Month + "_" + newTrade.Time.Year + " where idSdelki=" + newTrade.Id, connSingle);
            SqlDataReader newReader = checkTrade.ExecuteReader();
            try
            {
                newReader.Read();
                tradeExist = newReader.GetValue(1).ToString();
                newReader.Dispose();
                return;
            }
            catch (Exception)
            {
                newReader.Dispose();
            }

            SqlCommand insertToTable = new SqlCommand("Insert into " + _security.ToString() + "_" + newTrade.Time.Day.ToString() + "_" + newTrade.Time.Month.ToString() + "_"
                + newTrade.Time.Year.ToString() + "(idSdelki,vremyaSdelki,price,volume,direction) Values (@idSdelki,@vremyaSdelki,@price,@volume,@direction)", connSingle);

            SqlParameter param = new SqlParameter();
            param.ParameterName = "@idSdelki";
            param.Value = newTrade.Id;
            param.SqlDbType = System.Data.SqlDbType.Decimal;
            insertToTable.Parameters.Add(param);

            param = new SqlParameter();
            param.ParameterName = "@vremyaSdelki";
            param.Value = newTrade.Time;
            param.SqlDbType = System.Data.SqlDbType.DateTime;
            insertToTable.Parameters.Add(param);

            param = new SqlParameter();
            param.ParameterName = "@price";
            param.Value = newTrade.Price;
            param.SqlDbType = System.Data.SqlDbType.Float;
            insertToTable.Parameters.Add(param);

            param = new SqlParameter();
            param.ParameterName = "@volume";
            param.Value = newTrade.Volume;
            param.SqlDbType = System.Data.SqlDbType.Int;
            insertToTable.Parameters.Add(param);

            param = new SqlParameter();
            param.ParameterName = "@direction";
            if (newTrade.OrderDirection == OrderDirections.Buy)
                param.Value = 0;
            if (newTrade.OrderDirection == OrderDirections.Sell)
                param.Value = 1;
            param.SqlDbType = System.Data.SqlDbType.Int;
            insertToTable.Parameters.Add(param);

            try
            {

                insertToTable.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Не могу вставить новую сделку в существующую БД по инструменту: " + ex.Message);
            }

        }

        public void sendAllTradesToDBbulk(QuikTrader Trader)
        {

            // создаем екземпляр типа SQL и присваиваем ему путь к БД
            SqlConnection conn = new SqlConnection(_adminDB);
            try
            {
                // Пытаемся открыть БД
                conn.Open();
            }
            catch (Exception)
            {
                // если не получилось коннект к БД:ставим флаг, выдаем сообщение, возвращаем флаг
                System.Windows.MessageBox.Show("Ошибка подключения к БД (отправка множества сделок в БД)");
                conn.Close();
                return;
            }

            DateTime tradeTime = _trader.Trades.FirstOrDefault<Trade>().Time;

            int exist = 0;
            string tableCheck = "SELECT count(*) as IsExists FROM dbo.sysobjects where id = object_id('[dbo].[" + _security.ToString() + "_" + tradeTime.Day.ToString() + "_" +
                tradeTime.Month.ToString() + "_" + tradeTime.Year.ToString() + "]')";

            string tableName = _security.ToString() + "_" + tradeTime.Day.ToString() + "_" + tradeTime.Month.ToString() + "_" + tradeTime.Year.ToString();

            SqlCommand cmd = new SqlCommand(tableCheck, conn);

            try
            {
                exist = (Int32)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Не могу проверить существует ли таблица по инструменту: " + ex.Message);
            }

            if (exist == 1)
            {
                SqlCommand deleteAll = new SqlCommand(@"DELETE FROM " + tableName, conn);
                deleteAll.ExecuteNonQuery();
            }

            if (exist == 0)
            {
                SqlCommand cmdCreateTable = new SqlCommand("CREATE TABLE " + _security.ToString() + "_" + tradeTime.Day.ToString() + "_" +
                tradeTime.Month.ToString() + "_" + tradeTime.Year.ToString() +
                   " (porNomer decimal IDENTITY NOT NULL PRIMARY KEY" +
                      ", idSdelki decimal not null" +
                       ", vremyaSdelki datetime not null" +
                         ", price float not null" +
                         ", volume int not null" +
                         ", direction int not null);", conn);

                cmdCreateTable.ExecuteNonQuery();

            }

            DataTable dt = new DataTable();

            DataColumn porNom = new DataColumn("porNomer");
            porNom.DataType = typeof(decimal);
            dt.Columns.Add(porNom);

            DataColumn id = new DataColumn("idSdelki");
            id.DataType = typeof(decimal);
            dt.Columns.Add(id);

            DataColumn time = new DataColumn("vremyaSdelki");
            time.DataType = typeof(DateTime);
            dt.Columns.Add(time);

            DataColumn price = new DataColumn("price");
            price.DataType = typeof(float);
            dt.Columns.Add(price);

            DataColumn vol = new DataColumn("volume");
            vol.DataType = typeof(int);
            dt.Columns.Add(vol);

            DataColumn dir = new DataColumn("direction");
            dir.DataType = typeof(int);
            dt.Columns.Add(dir);

            foreach (Trade newTrade in _trader.Trades)
            {
                if (newTrade.Security == _security)
                {

                    DataRow newRow = dt.NewRow();

                    newRow["idSdelki"] = newTrade.Id;

                    newRow["vremyaSdelki"] = newTrade.Time;

                    newRow["price"] = (float)newTrade.Price;

                    newRow["volume"] = (int)newTrade.Volume;

                    if (newTrade.OrderDirection == OrderDirections.Buy)
                    newRow["direction"] = (int)0;

                    if (newTrade.OrderDirection == OrderDirections.Sell)
                        newRow["direction"] = (int)1;

                    dt.Rows.Add(newRow);

                }

            }

            SqlBulkCopy bc = new SqlBulkCopy(conn);

            bc.DestinationTableName = tableName;

            bc.WriteToServer(dt);

            conn.Close();
        

        }

        // метод добавления сделки в свечу
        public void dobavitSdelku(Trade trd)
        {
            // для каждой свечки в списке свечек
            foreach (Svecha sv in spisokSvechek)
            {
                // если начальная дата свечи меньше времени сделки и время сделки меньше конечной даты свечи
                if (sv.startCandleTime <= trd.Time && trd.Time < sv.endCandleTime)
                {
                    // добавляем сделку в список сделок свечи
                    sv.sdelkiSvechi.Add(trd);

                    // вызваем метод построения свечи
                    sv.postroitSvechu(trd, step);
                }
            }

        }

        // метод отрисовки свечей
        public void otrisovatSvechi()
        {
            // chart.Series["Bars"].Points.AddXY(spisokSvechek.First( s => s.startCandleTime.Hour == 10).startCandleTime, 10611, 10616, 10650, 10602);


            ///////////////////////////////////////////////////    clusteru ///////////////////////////

            if (chart.Series["Cluster"].Points.Count > 0)
                chart.Series["Cluster"].Points.Clear();

            // для каждой свечки в списке свечек
            foreach (Svecha sv in spisokSvechek)
            {
                // для каждого кластера в списке кластеров свечи
                foreach (Cluster cl in sv.clusteruSvechi)
                {
                    // график кластеров
                    chart.Series["Cluster"].Points.AddXY(sv.startCandleTime, (double)cl.minPrice, (double)cl.maxPrice);
                    chart.Series["Cluster"].Points.Last().Label = cl.clusterVolume.ToString() + " x " + cl.delta.ToString() + " (" + cl.clusterPercent.ToString() + ")";

                }

            }
            ///////////////////////////////////////////////////////////////////////////////////////////////




            /////////////////////////////////    horizontVolume  ////////////////////////////
            if (chart.Series["leftBuyVolume"].Points.Count > 0)
                chart.Series["leftBuyVolume"].Points.Clear();
            if (chart.Series["leftSellVolume"].Points.Count > 0)
                chart.Series["leftSellVolume"].Points.Clear();
            Svecha tmpSvecha = new Svecha();

            // для каждой свечки в списке свечек
            foreach (Svecha sv in spisokSvechek)
            {
                // для каждого кластера в списке кластеров свечи
                foreach (Cluster cl in sv.clusteruSvechi)
                {

                    if (tmpSvecha.clusteruSvechi != null && tmpSvecha.clusteruSvechi.FirstOrDefault(s => s.minPrice == cl.minPrice) != null)
                    {
                        tmpSvecha.clusteruSvechi.First(s => s.minPrice == cl.minPrice).buyVolume += cl.buyVolume;
                        tmpSvecha.clusteruSvechi.First(s => s.minPrice == cl.minPrice).sellVolume += cl.sellVolume;
                        tmpSvecha.clusteruSvechi.First(s => s.minPrice == cl.minPrice).delta += cl.delta;
                    }
                    else if (tmpSvecha.clusteruSvechi != null && tmpSvecha.clusteruSvechi.FirstOrDefault(s => s.minPrice == cl.minPrice) == null)
                    {
                        Cluster newCluster = new Cluster();
                        newCluster.buyVolume = cl.buyVolume;
                        newCluster.sellVolume = cl.sellVolume;
                        newCluster.minPrice = cl.minPrice;
                        newCluster.delta = cl.delta;

                        tmpSvecha.clusteruSvechi.Add(newCluster);
                    }

                    else if (tmpSvecha.clusteruSvechi == null)
                    {
                        Cluster newCluster = new Cluster();
                        newCluster.buyVolume = cl.buyVolume;
                        newCluster.sellVolume = cl.sellVolume;
                        newCluster.minPrice = cl.minPrice;
                        newCluster.delta = cl.delta;

                        tmpSvecha.clusteruSvechi.Add(newCluster);
                    }

                }
            }

            foreach (Cluster clust in tmpSvecha.clusteruSvechi)
            {
                if (clust.delta >= 0)
                {
                    chart.Series["leftBuyVolume"].Points.AddXY((double)clust.minPrice, clust.buyVolume);
                    chart.Series["leftBuyVolume"].Points.Last().Label = clust.buyVolume.ToString() + " x " + clust.delta.ToString();
                    chart.Series["leftSellVolume"].Points.AddXY((double)clust.minPrice, clust.sellVolume);
                }
                if (clust.delta < 0)
                {
                    chart.Series["leftBuyVolume"].Points.AddXY((double)clust.minPrice, clust.buyVolume);
                    chart.Series["leftSellVolume"].Points.AddXY((double)clust.minPrice, clust.sellVolume);
                    chart.Series["leftSellVolume"].Points.Last().Label = clust.sellVolume.ToString() + " x " + clust.delta.ToString();
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////




            //////////////////////////////////    nizVolume////////////////////////////////////////////////
            if (chart.Series["nizVolumeBuy"].Points.Count > 0)
                chart.Series["nizVolumeBuy"].Points.Clear();
            if (chart.Series["nizVolumeSell"].Points.Count > 0)
                chart.Series["nizVolumeSell"].Points.Clear();

            foreach (Svecha sv in spisokSvechek)
            {
                if (sv.sdelkiSvechi.Count > 0)
                {

                    if (sv.deltaSvechi >= 0)
                    {

                        chart.Series["nizVolumeBuy"].Points.AddXY(sv.startCandleTime, sv.buyVolumeSvechi);
                        chart.Series["nizVolumeBuy"].Points.Last().Label = sv.buyVolumeSvechi.ToString() + " x " + sv.deltaSvechi.ToString() + " (" + sv.percent.ToString() + ")";
                        chart.Series["nizVolumeSell"].Points.AddXY(sv.startCandleTime, sv.sellVolumeSvechi);
                    }
                    if (sv.deltaSvechi < 0)
                    {
                        chart.Series["nizVolumeBuy"].Points.AddXY(sv.startCandleTime, sv.buyVolumeSvechi);
                        chart.Series["nizVolumeSell"].Points.AddXY(sv.startCandleTime, sv.sellVolumeSvechi);
                        chart.Series["nizVolumeSell"].Points.Last().Label = sv.sellVolumeSvechi.ToString() + " x " + sv.deltaSvechi.ToString() + " (" + sv.percent.ToString() + ")";
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////


        }

        // метод измеенеия инструмента в ComboBox
        private void userSecurities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _security = (Security)userSecurities.SelectedItem;
        }

        // метод измеенеия портфеля в ComboBox
        private void userPortfolios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _portfolio = (Portfolio)userPortfolios.SelectedItem; // присваеваем переменной _portfolio имя  выбраного портфеля
        }

        private void userStep_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                step = Convert.ToDecimal(userStep.Text);
            }
            catch (Exception)
            { }
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            this.chartControl.Width = (this.Width / 100) * 98;
            this.chartControl.Height = (this.Height / 100) * 89;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_trader != null)
            {
                if (_trader.IsExportStarted == true)
                    _trader.StopExport();
                if (_trader.IsConnected == true)
                    _trader.Disconnect();
            }
        }

    }

}
