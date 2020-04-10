using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;



namespace MyWebSite.Services
{
    public class MyDbContext
    {
        /// <summary>
        /// наименование строки подключения к БД
        /// </summary>
        private string _connectionString = "Server=DESKTOP-OBTLBE5;Integrated security=SSPI;";
        /// <summary>
        /// Наименование базы данных
        /// </summary>
        private string _nameDatabase = "Trader_DataBase";
        /// <summary>
        /// Наименование пространства таблиц и процедур базы данных
        /// </summary>
        private string _nameDataSpace = "trader";

        /// <summary>
        /// Путь к хранению файлов данных базы
        /// </summary>
        private string _mainPathDatabase = @"C:\TraderDatabase";

        /// <summary>
        /// Путь к хранению файлов истории операций с БД
        /// </summary>
        private string _historyPathDatabase = @"C:\TraderDatabase";

        /// <summary>
        /// Путь к храниению логов БД
        /// </summary>
        private string _logPathDataBase = @"C:\TraderDatabase";

        /// <summary>
        /// Флаг состояния подключения к БД
        /// </summary>
        private bool _connected = false;

        /// <summary>
        /// Флаг - говорящий инициализирована БД или нет
        /// </summary>
        private bool _initialized = false;


        public bool DataseseIsInitialized
        {
            get { return _initialized; }
            set { _initialized = value; }
        }

        private string _create_database_string;
        private string _create_database_space_string;
        private string _create_database_sources_securities;
        private string _fill_sources_securities_base_data;
        private string _create_database_table_one_source_securities;
        private string _create_database_table_ImpersonalTrades_string;
        

        private Dictionary<string, string> _dict_connection_string
        {
            get
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                var list = _connectionString.Split(";");

                foreach (var l in list)
                {
                    var list1 = l.Split("=");
                    if(list1.Count() > 1)
                    {
                        dict.Add(list1[0], list1[1]);
                    }
                    
                }

                return dict;
            }
        }

        public MyDbContext() 
        {
            #region Строка создания базы данных, пространства, и таблицы обезличенных сделок

            //Строка создания базы данных
            _create_database_string =
                   $@"if db_id('{_nameDatabase}') is null " +
                   $@"begin " +
                   $@"create database {_nameDatabase} on primary " +
                   $@"(name = N'{_nameDatabase}_data', " +
                   $@"filename = N'{_mainPathDatabase}\{_nameDatabase}_data.mdf'), " +
                   $@"filegroup[OrderHistoryGroup] " +
                   $@"(name = N'Hist{_nameDatabase}1', filename = N'{_historyPathDatabase}\hist_{_nameDatabase}.ndf') " +
                   $@"log on " +
                   $@"(name = N'{_nameDatabase}_log', filename = N'{_logPathDataBase}\hist_log_{_nameDatabase}.ldf') " +
                   $@"end "; 

            //Строка создания пространства имен
            _create_database_space_string =
                   $@"use {_nameDatabase}; " +
                   $@"if schema_id('{_nameDataSpace}') is null exec ('create schema {_nameDataSpace}') ";

            _create_database_sources_securities =
                $@"use {_nameDatabase}; " +
                $@"if object_id(N'{_nameDataSpace}.SourcesSecurities') is null " +
                $@"create table {_nameDataSpace}.SourcesSecurities( " +
                $@"Id int identity not null, " +
                $@"Name nvarchar(200) not null, " +
                $@"constraint pk_{_nameDataSpace}_SourcesSecurities_Id primary key(Id) " +
                $@") ";

            _fill_sources_securities_base_data =
                //$"go; \n" +
                //$@"set identity_insert {_nameDatabase}.SourcesSecurities on; " +
                //$"\n" +
                //$"go; \n" + EXISTS (SELECT TOP (1) 1 FROM [Table] where field='abcd')
                $@"use {_nameDatabase}; " +
                $@"if object_id(N'{_nameDataSpace}.SourcesSecurities') is not null and (not exists (select top (1) 1 from {_nameDataSpace}.SourcesSecurities as ss where ss.Name like 'quik'))" +
                $@"insert into {_nameDataSpace}.SourcesSecurities(Name) "  +
                $@"values " +
                $@"('quik'); " +
                $@"if object_id(N'{_nameDataSpace}.SourcesSecurities') is not null and (not exists (select top (1) 1 from {_nameDataSpace}.SourcesSecurities as ss where ss.Name like 'finam.export'))" +
                $@"insert into {_nameDataSpace}.SourcesSecurities(Name) " +
                $@"values " +
                $"('finam.export'); " +
                //$"go; \n" +
                //$@"set identity_insert {_nameDatabase}.SourcesSecurities off; " +
                //$"\n" +
                //$"go; \n" +
                "";

            _create_database_table_one_source_securities =
                $@"use {_nameDatabase}; " +
                $@"if object_id(N'{_nameDataSpace}.OneSourceSecurities') is null " +
                $@"create table {_nameDataSpace}.OneSourceSecurities( " +
                $@"Id int identity not null, " +
                $@"IdInt int null, " +
                $@"IdString nvarchar(100) null, " +
                $@"Name nvarchar(200) not null, " +
                $@"Code nvarchar(100) not null, " +
                $@"Market nvarchar(500) not null, " +
                $@"MarketId nvarchar(100) not null, " +
                $@"Decp nvarchar(300) not null, " +
                $@"EmitentChild nvarchar(20) not null, " +
                $@"Url nvarchar(1000) not null, " +
                $@"Source_Id int not null, " +
                $@"constraint pk_{_nameDataSpace}_OneSourceSecurities_Id primary key(Id), " +
                $@"constraint fk_{_nameDataSpace}_OneSourceSecurities_SourcesSecurities foreign key (Source_Id) references {_nameDataSpace}.SourcesSecurities (Id) on delete cascade " +
                $@") ";

            //Строка создания таблицы обезличенных сделок
            _create_database_table_ImpersonalTrades_string =
                   $@"use {_nameDatabase}; " +
                   $@"if object_id(N'{_nameDataSpace}.ImpersonalTrades') is null " +
                   $@"create table {_nameDataSpace}.ImpersonalTrades( " +
                   $@"Id int identity not null, " +
                   $@"Ticker nvarchar(100) null, " +
                   $@"Ticker_Short nvarchar(20) null, " +
                   $@"Date int not null, " +
                   $@"Time int not null, " +
                   $@"Date_Time smalldatetime not null, " +
                   $@"Last decimal not null, " +
                   $@"Vol int not null, " +
                   $@"Oper nvarchar(20) null, " +
                   $@"Exchange_Id bigint null, " +
                   $@"OneSourceSecurities_Id int null, " +
                   $@"constraint pk_{_nameDataSpace}_ImpersonalTrades_Id primary key(Id), " +
                   $@"constraint fk_{_nameDataSpace}_ImpersonalTrades_OneSourceSecurities foreign key (OneSourceSecurities_Id) references {_nameDataSpace}.OneSourceSecurities (Id) on delete cascade " +
                   $@") ";

            #endregion

        }


        private bool IsConnected(string str)
        {
            if (_connected)
            {
                Debug.WriteLine(str + ": подключена БД");
                return true;
            }
            Debug.WriteLine(str + ": НЕ подключена БД");
            return false;
        }


        private bool IsInitialized(string str)
        {
            if (_initialized)
            {
                Debug.WriteLine(str + ": уже инициализирована БД");
                return true;
            }
            Debug.WriteLine(str + ": еще не инициализирована БД");
            return false;
        }

        public async Task CreateNewDatabase(string connectionString)
        {
            if (IsConnected("CreateNewDatabase")) return;
            if (IsInitialized("CreateNewDatabase: ")) return;

            _connectionString = connectionString;

            string connectionToServer = "";

            foreach (var d in _dict_connection_string)
            {
                //Server=DESKTOP-OBTLBE5;Integrated security=SSPI;
                if (d.Key.Equals("Server") || d.Key.Equals("Integrated security"))
                {
                    connectionToServer = connectionToServer + d.Key + "=" + d.Value + ";";
                }
            }

            DirectoryInfo mainDirInfo = new DirectoryInfo(_mainPathDatabase);
            if (!mainDirInfo.Exists)
            {
                mainDirInfo.Create();
            }
            DirectoryInfo histDirInfo = new DirectoryInfo(_historyPathDatabase);
            if (!histDirInfo.Exists)
            {
                histDirInfo.Create();
            }


            using (SqlConnection connection = new SqlConnection(connectionToServer))
            {
                await connection.OpenAsync();
                Debug.WriteLine("Подключение открыто");
                try
                {
                    
                    


                    SqlCommand cmdCreateDatabase = new SqlCommand(_create_database_string, connection);
                    SqlCommand cmdCreateNameSpace = new SqlCommand(_create_database_space_string, connection);
                    SqlCommand cmdCreateTableSourcesSecurities = new SqlCommand(_create_database_sources_securities, connection);
                    SqlCommand cmdFillSourcesSecurities = new SqlCommand(_fill_sources_securities_base_data, connection);

                    SqlCommand cmdCreateTableOneSourceSecurities = new SqlCommand(_create_database_table_one_source_securities, connection);
                    SqlCommand cmdCreateTableImpersonalTrades = new SqlCommand(_create_database_table_ImpersonalTrades_string, connection);

                    cmdCreateDatabase.ExecuteNonQuery();
                    cmdCreateNameSpace.ExecuteNonQuery();
                    cmdCreateTableSourcesSecurities.ExecuteNonQuery();
                    cmdFillSourcesSecurities.ExecuteNonQuery();

                    cmdCreateTableOneSourceSecurities.ExecuteNonQuery();
                    cmdCreateTableImpersonalTrades.ExecuteNonQuery();

                    Debug.WriteLine("Новая база создана.");
                    _connected = true;



                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }

            }


            Debug.WriteLine("Подключение закрыто...");

        }
    }
}
