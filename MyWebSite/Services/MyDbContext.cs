using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyWebSite.Models;
using MyWebSite.Models.Entities;
using MyWebSite.Tools;


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

        //private Dictionary<string, string> _dict_of_sql_command = new Dictionary<string, string>();


        public bool DatabaseIsInitialized
        {
            get { return _initialized; }
            set { _initialized = value; }
        }

        Queue<string> _queue_of_sql_commands = new Queue<string>();

        //создание хранимых процедур
        private Dictionary<string, string> _dict_connection_string
        {
            get
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                var list = _connectionString.Split(";");

                foreach (var l in list)
                {
                    var list1 = l.Split("=");
                    if (list1.Count() > 1)
                    {
                        dict.Add(list1[0], list1[1]);
                    }
                }

                return dict;
            }
        }

        public MyDbContext()
        {
            #region Строка создания базы данных, пространства имен
            //Строка создания базы данных

            _queue_of_sql_commands.Enqueue(
                $@"if db_id('{_nameDatabase}') is null " +
                $@"begin " +
                $@"create database {_nameDatabase} on primary " +
                $@"(name = N'{_nameDatabase}_data', " +
                $@"filename = N'{_mainPathDatabase}\{_nameDatabase}_data.mdf'), " +
                $@"filegroup[OrderHistoryGroup] " +
                $@"(name = N'Hist{_nameDatabase}1', filename = N'{_historyPathDatabase}\hist_{_nameDatabase}.ndf') " +
                $@"log on " +
                $@"(name = N'{_nameDatabase}_log', filename = N'{_logPathDataBase}\hist_log_{_nameDatabase}.ldf') " +
                $@"end "
                );

            //Строка создания пространства имен
            _queue_of_sql_commands.Enqueue(
                   $@"use {_nameDatabase}; " +
                   $@"if schema_id('{_nameDataSpace}') is null exec ('create schema {_nameDataSpace}') "
            );


            #endregion

            #region Таблица Источники инструментов

            //создание таблицы ресурса информации о торгах
            _queue_of_sql_commands.Enqueue(
                $@"use {_nameDatabase}; " +
                $@"if object_id(N'{_nameDataSpace}.SourcesSecurities') is null " +
                $@"create table {_nameDataSpace}.SourcesSecurities( " +
                $@"Id int identity not null, " +
                $@"Name nvarchar(200) not null, " +
                $@"constraint pk_{_nameDataSpace}_SourcesSecurities_Id primary key(Id) " +
                $@") "
            );

            //ввод информации по первым 2 источникам
            _queue_of_sql_commands.Enqueue(

                $@"use {_nameDatabase}; " +
                $@"if object_id(N'{_nameDataSpace}.SourcesSecurities') is not null and (not exists (select top (1) 1 from {_nameDataSpace}.SourcesSecurities as ss where ss.Name like 'Ручной ввод'))" +
                $@"insert into {_nameDataSpace}.SourcesSecurities(Name) " +
                $@"values " +
                $@"('Ручной ввод'); " +
                $@"if object_id(N'{_nameDataSpace}.SourcesSecurities') is not null and (not exists (select top (1) 1 from {_nameDataSpace}.SourcesSecurities as ss where ss.Name like 'finam.export'))" +
                $@"insert into {_nameDataSpace}.SourcesSecurities(Name) " +
                $@"values " +
                $"('finam.export'); " +
                $@"if object_id(N'{_nameDataSpace}.SourcesSecurities') is not null and (not exists (select top (1) 1 from {_nameDataSpace}.SourcesSecurities as ss where ss.Name like 'Quik'))" +
                $@"insert into {_nameDataSpace}.SourcesSecurities(Name) " +
                $@"values " +
                $@"('Quik'); " +
                ""
            );

            #endregion

            #region Таблица Инструменты по источникам
            //Создание таблицы инструменты по источникам
            _queue_of_sql_commands.Enqueue(
                $@"use {_nameDatabase}; " +
                $@"if object_id(N'{_nameDataSpace}.BrokerRepositorySecurity') is null " +
                $@"create table {_nameDataSpace}.BrokerRepositorySecurity( " +
                $@"Id int identity not null, " +
                $@"IdInt int null, " +
                $@"IdString nvarchar(100) null, " +
                $@"Name nvarchar(200) not null, " +
                $@"Code nvarchar(100) not null, " +
                $@"Market nvarchar(500) not null, " +
                $@"MarketId nvarchar(100) not null, " +
                $@"Decp nvarchar(300) null, " +
                $@"EmitentChild nvarchar(20) null, " +
                $@"Url nvarchar(1000) null, " +
                $@"Source_Id int not null, " +
                $@"constraint pk_{_nameDataSpace}_BrokerRepositorySecurity_Id primary key(Id), " +
                $@"constraint fk_{_nameDataSpace}_BrokerRepositorySecurity_SourcesSecurities foreign key (Source_Id) references {_nameDataSpace}.SourcesSecurities (Id) on delete cascade " +
                $@") "
            );


            #endregion

            #region Таблица обезличенных сделок

            //Строка создания таблицы обезличенных сделок
            _queue_of_sql_commands.Enqueue(
                $@"use {_nameDatabase}; " +
                $@"if object_id(N'{_nameDataSpace}.ImpersonalTrades') is null " +
                $@"create table {_nameDataSpace}.ImpersonalTrades( " +
                $@"Id bigint identity not null, " +
                $@"Ticker nvarchar(100) null, " +
                $@"Ticker_Short nvarchar(20) null, " +
                $@"Date int not null, " +
                $@"Time int not null, " +
                $@"Date_Time smalldatetime not null, " +
                $@"Last decimal not null, " +
                $@"Vol int not null, " +
                $@"Oper nvarchar(20) null, " +
                $@"Exchange_Id bigint null, " +
                $@"BrokerRepositorySecurity_Id int null, " +
                $@"constraint pk_{_nameDataSpace}_ImpersonalTrades_Id primary key(Id), " +
                $@"constraint fk_{_nameDataSpace}_ImpersonalTrades_BrokerRepositorySecurity foreign key (BrokerRepositorySecurity_Id) references {_nameDataSpace}.BrokerRepositorySecurity (Id) on delete cascade " +
                $@") "
            );



            #endregion

            #region Хранимые процедуры для Источника инструментов

            //вношу логику в ранее созданную хранимую процедуру "Взять все источники для инструментов"
            _queue_of_sql_commands.Enqueue(
                $@"create or alter procedure {_nameDataSpace}.sp_get_all_source_security " + " " +
                $@"as " + " " +
                $@"begin " + " " +
                $@"select * from {_nameDataSpace}.SourcesSecurities ss order by ss.Id " + " " +
                $@"end " + " " +
                ""
            );

            //вношу логику в ранее созданную хранимую процедуру "Взять источник по ид"
            _queue_of_sql_commands.Enqueue(
                $@"create or alter procedure {_nameDataSpace}.sp_get_by_id_source_security " + " " +
                $@"@id int = null " + " " +
                $@"as " + " " +
                $@"begin " + " " +
                $@"select top 1 * from {_nameDataSpace}.SourcesSecurities ss where ss.Id = @id " + " " +
                $@"end " + " " +
                ""
            );

            #endregion

            #region Хранимые процедуры для инструмента
            _queue_of_sql_commands.Enqueue(
                $@" 
                     create or alter procedure {_nameDataSpace}.sp_broker_repository_security_get_list
                    	@Name nvarchar(200) null = null
                    	, @Market nvarchar(500) null =  null
                    	, @MarketId nvarchar(100) null = null
                    	, @Decp nvarchar(300) null = null
                    	, @Source_Id int null = null
                    as 
                    begin
	                    if @Name is not null
	                    	select * from {_nameDataSpace}.BrokerRepositorySecurity where [Name] like '%'+ @Name + '%'
	                    else if @Market is not null
	                    	select * from {_nameDataSpace}.BrokerRepositorySecurity where Market like '%'+ @Market + '%'
	                    else if @MarketId is not null
	                    	select * from {_nameDataSpace}.BrokerRepositorySecurity where MarketId like @MarketId
	                    else if @Decp is not null
	                    	select * from {_nameDataSpace}.BrokerRepositorySecurity where Decp like @Decp
	                    else if @Source_Id is not null
	                    	select * from {_nameDataSpace}.BrokerRepositorySecurity where Source_Id = @Source_Id
	                    else if @Name is null and @Market is null and @MarketId is null and @Decp is null and @Source_Id is null
	                    	select * from {_nameDataSpace}.BrokerRepositorySecurity
                    end                                 
                    ;"
            );

            _queue_of_sql_commands.Enqueue(
                $@"
                      create or alter procedure {_nameDataSpace}.sp_broker_repository_security_update
                        	@Id int = null out
                        	, @Idint int
                        	, @IdString nvarchar(100)
                        	, @Name nvarchar(200)
                        	, @Code nvarchar(100)
                        	, @Market nvarchar(500)
                        	, @MarketId nvarchar(100)
                        	, @Decp nvarchar(300) null = null
                        	, @EmitentChild nvarchar(20) null = null
                        	, @Url nvarchar(1000) null = null
                        	, @Source_Id int
                        as 
                        begin
                        	if @Idint is null
                        		or @IdString is null
                        		or @Name is null
                        		or @Code is null
                        		or @Market is null
                        		or @MarketId is null
                        		or @Source_Id is null
                        		begin
                        			raiserror('Some variable is null in {_nameDataSpace}.update_broker_repository_security', 16, 1)
                        			return @@error;
                        		end		
                        
                        
                        	if @Id is not null begin	
                        		update {_nameDataSpace}.BrokerRepositorySecurity
                        		set 
                        			IdInt					=@Idint
                        			, IdString				=@IdString
                        			, [Name]				=@Name
                        			, Code					=@Code
                        			, Market				=@Market
                        			, MarketId				=@MarketId
                        			, Decp					=@Decp
                        			, EmitentChild			=@EmitentChild
                        			, [Url]					=@Url
                        			, Source_Id				=@Source_Id
                        		where
                        			Id = @Id
                        	end
                        	else begin
                        	     if exists(
                        	     	select 
                        	     		*
                        	     	from
                        	     		{_nameDataSpace}.BrokerRepositorySecurity brs
                        	     	where
                        	     		isnull(brs.IdInt, 0) = isnull(@Idint, 0)
                        	     		and isnull(brs.IdString, 0) = isnull(@IdString, 0)
                        	     		and isnull(brs.[Name], 0) = isnull(@Name, 0)
                        	     		and isnull(brs.Code, 0) = isnull(@Code, 0)
                        	     		and isnull(brs.Market, 0) = isnull(@Market, 0)
                        	     		and isnull(brs.MarketId, 0) = isnull(@MarketId, 0)
                        	     		--and isnull(brs.Decp, 0) = isnull(@Decp, 0)
                        	     		--and isnull(brs.EmitentChild, 0) = isnull(@EmitentChild, 0)
                        	     		--and isnull(brs.[Url], 0) = isnull(@Url, 0)
                        	     		and isnull(brs.Source_Id, 0) = isnull(@Source_Id, 0)
                        	     )
                        	     begin
                        
                        	     	raiserror('You can`t create record with ident all fields', 16, 1)
                        	     	return @@error;
                        	     end 
                        		insert into {_nameDataSpace}.BrokerRepositorySecurity(
                        			IdInt			
                        			, IdString		
                        			, [Name]			
                        			, Code			
                        			, Market			
                        			, MarketId		
                        			, Decp			
                        			, EmitentChild	
                        			, [Url]			
                        			, Source_Id	
                        		)
                        		values(
                        			@Idint
                        			, @IdString
                        			, @Name
                        			, @Code
                        			, @Market
                        			, @MarketId
                        			, @Decp
                        			, @EmitentChild
                        			, @Url
                        			, @Source_Id
                        		)
                        
                        		set @Id = scope_identity();                        			
                        	end      
                        end
                    ;"

            );

            _queue_of_sql_commands.Enqueue($@" 
                create or alter procedure {_nameDataSpace}.sp_broker_repository_security_get_by_id
                    @id int = null 
                as 
                begin 
                    select top 1 * from {_nameDataSpace}.BrokerRepositorySecurity brs where brs.Id = @id 
                end
            ;"

            );
            #endregion

            #region Хранимые процедуры обезличенных сделок

            _queue_of_sql_commands.Enqueue(
                @$"
                        /*
                        Процедура проверяет наличие трейда на данну дату и возвращает bit 1 - если есть, 0 - если нет
                        */
                        create or alter procedure {_nameDataSpace}.sp_impersonal_trades_check_by_date
                            @result bit out
                            , @date smalldatetime = null 
                        as 
                        begin 
                        	if exists (select top 1 * from {_nameDataSpace}.ImpersonalTrades it where it.Date_Time = @date)
                        		set @result = 1
                        	else 
                        		set @result = 0
                        end

                    ;"
            );

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

        /// <summary>
        /// Метод возвращает строку подключения для sql - запроса
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {
            string connectionToServer = "";

            foreach (var d in _dict_connection_string)
            {
                //Server=DESKTOP-OBTLBE5;Integrated security=SSPI;
                if (d.Key.Equals("Server") || d.Key.Equals("Integrated security"))
                {
                    connectionToServer = connectionToServer + d.Key + "=" + d.Value + ";";
                }
            }

            return connectionToServer;
        }


        #region Метод первой инициализации БД и всех ее таблиц и хранимых процедур
        /// <summary>
        /// Метод инициализации БД, при первом запуске формирует запрос на создание новой БД, если она не создана и обновление старых
        /// </summary>
        /// <param name="connectionString">строка для подключения к БД</param>
        /// <returns></returns>
        public async Task CreateNewDatabase(string connectionString)
        {
            if (IsConnected("CreateNewDatabase")) return;
            if (IsInitialized("CreateNewDatabase: ")) return;

            _connectionString = connectionString;

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

            await using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                await connection.OpenAsync();

                Debug.WriteLine("Подключение открыто");
                try
                {
                    Queue<SqlCommand> queue_commands_to_exec = new Queue<SqlCommand>();

                    int count_queue = _queue_of_sql_commands.Count();

                    object locker = new object(); ;

                    for (int i = 0; i < count_queue; i++)
                    {
                        lock (locker)
                        {
                            new SqlCommand(_queue_of_sql_commands.Dequeue(), connection).ExecuteNonQuery();
                        }

                    }

                    Debug.WriteLine("Новая база создана.");


                    List<SourceSecurity> result = new List<SourceSecurity>();

                    //если id не передан вызываем процедуру выборки всех сущьностей
                    string sqlExpression = $@"{_nameDataSpace}.sp_get_all_source_security";

                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    // указываем, что команда представляет хранимую процедуру
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                try
                                {
                                    SourceSecurity ss = new SourceSecurity()
                                    {
                                        //Id = reader.GetInt32(0)
                                        //string name = reader.GetString(1)
                                        Id = Convert.ToInt32(reader["Id"]),
                                        Name = Convert.ToString(reader["Name"]),
                                    };

                                    result.Add(ss);
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine("GetAllSourceSecurity: " + e.ToString());
                                }
                            }
                        }
                    }


                    _connected = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }

            Debug.WriteLine("Подключение закрыто...");

        }



        #endregion

        #region Вызов хранимых процедур для источников инструментов

        public async Task<SourceSecurity> GetSourceSecurityById(int id)
        {
            SourceSecurity result = null;

            string sqlExpression = $@"{_nameDataSpace}.sp_get_by_id_source_security";

            string gg = GetConnectionString() + $"Initial Catalog={_nameDatabase};"; ;

            await using (SqlConnection connection = new SqlConnection(gg))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                // указываем, что команда представляет хранимую процедуру
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@id", id));

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                result = new SourceSecurity()
                                {
                                    //Id = reader.GetInt32(0)
                                    //string name = reader.GetString(1)
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = Convert.ToString(reader["Name"]),
                                };
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("GetAllSourceSecurity: " + e.ToString());
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Вызывает хранимую процедуру из БД, возвращает список всех SourceSecurity
        /// </summary>
        /// <returns></returns>
        public async Task<List<SourceSecurity>> GetAllSourceSecurity()
        {
            List<SourceSecurity> result = new List<SourceSecurity>();

            //если id не передан вызываем процедуру выборки всех сущьностей
            string sqlExpression = $@"{_nameDataSpace}.sp_get_all_source_security";

            string gg = GetConnectionString() + $"Initial Catalog={_nameDatabase};"; ;

            await using (SqlConnection connection = new SqlConnection(gg))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                // указываем, что команда представляет хранимую процедуру
                command.CommandType = System.Data.CommandType.StoredProcedure;
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                SourceSecurity ss = new SourceSecurity()
                                {
                                    //Id = reader.GetInt32(0)
                                    //string name = reader.GetString(1)
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = Convert.ToString(reader["Name"]),
                                };

                                result.Add(ss);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("GetAllSourceSecurity: " + e.ToString());
                            }
                        }
                    }
                }
            }

            return result;
        }


        #endregion

        #region Вызов хранимых процедур для инструментов

        public async Task UpdateAllSecuritiesFromSource(List<BrokerRepositorySecurity> source_list, int source_id)
        {
            if (source_id < 0)
                return;

            //выбираю из базы все инструменты по данному источнику
            var list_already_exists_securities = await GetListBrokerRepositorySecurity(sourceId: source_id);

            //если id не передан вызываем процедуру выборки всех сущьностей
            string sqlExpression = $@"{_nameDataSpace}.sp_broker_repository_security_update";

            string gg = GetConnectionString() + $"Initial Catalog={_nameDatabase};"; ;

            await using (SqlConnection connection = new SqlConnection(gg))
            {
                await connection.OpenAsync();

                //перезаписываем новые данные для старых инструментов
                foreach (var sec_from_db in list_already_exists_securities)
                {

                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    // указываем, что команда представляет хранимую процедуру
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    var sec_frome_internet = source_list.FirstOrDefault(s => string.Equals(s.IdString, sec_from_db.IdString));

                    if (sec_frome_internet != null)
                    {
                        command.Parameters.Add(new SqlParameter("@Id", sec_from_db.Id));
                        command.Parameters.Add(new SqlParameter("@IdInt", sec_frome_internet.IdInt));
                        command.Parameters.Add(new SqlParameter("@IdString", sec_frome_internet.IdString));
                        command.Parameters.Add(new SqlParameter("@Name", sec_frome_internet.Name));
                        command.Parameters.Add(new SqlParameter("@Code", sec_frome_internet.Code));
                        command.Parameters.Add(new SqlParameter("@Market", sec_frome_internet.Market));
                        command.Parameters.Add(new SqlParameter("@MarketId", sec_frome_internet.MarketId));
                        if (string.IsNullOrEmpty(sec_frome_internet.Decp))
                            command.Parameters.Add(new SqlParameter("@Decp", sec_frome_internet.Decp));
                        if (string.IsNullOrEmpty(sec_frome_internet.EmitentChild))
                            command.Parameters.Add(new SqlParameter("@EmitentChild", sec_frome_internet.EmitentChild));
                        if (string.IsNullOrEmpty(sec_frome_internet.Url))
                            command.Parameters.Add(new SqlParameter("@Url", sec_frome_internet.Url));

                        command.Parameters.Add(new SqlParameter("@Source_id", sec_frome_internet.SourceSecurityId));

                        await command.ExecuteNonQueryAsync();

                        source_list.Remove(sec_frome_internet);
                    }
                }

                //добавляю новые записи, которые остались после обновления старых
                foreach (var sec_from_internet in source_list)
                {

                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    // указываем, что команда представляет хранимую процедуру
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // параметр для id
                    SqlParameter idParam = new SqlParameter
                    {
                        ParameterName = "@Id",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output // параметр выходной
                    };
                    command.Parameters.Add(idParam);
                    command.Parameters.Add(new SqlParameter("@IdInt", sec_from_internet.IdInt));
                    command.Parameters.Add(new SqlParameter("@IdString", sec_from_internet.IdString));
                    command.Parameters.Add(new SqlParameter("@Name", sec_from_internet.Name));
                    command.Parameters.Add(new SqlParameter("@Code", sec_from_internet.Code));
                    command.Parameters.Add(new SqlParameter("@Market", sec_from_internet.Market));
                    command.Parameters.Add(new SqlParameter("@MarketId", sec_from_internet.MarketId));
                    if (string.IsNullOrEmpty(sec_from_internet.Decp))
                        command.Parameters.Add(new SqlParameter("@Decp", sec_from_internet.Decp));
                    if (string.IsNullOrEmpty(sec_from_internet.EmitentChild))
                        command.Parameters.Add(new SqlParameter("@EmitentChild", sec_from_internet.EmitentChild));
                    if (string.IsNullOrEmpty(sec_from_internet.Url))
                        command.Parameters.Add(new SqlParameter("@Url", sec_from_internet.Url));

                    command.Parameters.Add(new SqlParameter("@Source_id", sec_from_internet.SourceSecurityId));

                    await command.ExecuteNonQueryAsync();

                }
            }

        }

        public async Task<int?> UpdateOneBrokerRepositorySecurity(BrokerRepositorySecurity security, int? id = null)
        {
            if (
                security.IdInt == null ||
                string.IsNullOrEmpty(security.IdString) ||
                string.IsNullOrEmpty(security.Name) ||
                string.IsNullOrEmpty(security.Code) ||
                string.IsNullOrEmpty(security.Market) ||
                string.IsNullOrEmpty(security.MarketId))
            {

                return null;
            }

            //если id не передан вызываем процедуру выборки всех сущьностей
            string sqlExpression = $@"{_nameDataSpace}.sp_broker_repository_security_update";

            string gg = GetConnectionString() + $"Initial Catalog={_nameDatabase};"; ;

            await using (SqlConnection connection = new SqlConnection(gg))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                // указываем, что команда представляет хранимую процедуру
                command.CommandType = System.Data.CommandType.StoredProcedure;

                if (id == null)
                {
                    // параметр для id
                    SqlParameter idParam = new SqlParameter
                    {
                        ParameterName = "@Id",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output // параметр выходной
                    };
                    command.Parameters.Add(idParam);
                    command.Parameters.Add(new SqlParameter("@IdInt", security.IdInt));
                    command.Parameters.Add(new SqlParameter("@IdString", security.IdString));
                    command.Parameters.Add(new SqlParameter("@Name", security.Name));
                    command.Parameters.Add(new SqlParameter("@Code", security.Code));
                    command.Parameters.Add(new SqlParameter("@Market", security.Market));
                    command.Parameters.Add(new SqlParameter("@MarketId", security.MarketId));
                    if (string.IsNullOrEmpty(security.Decp))
                        command.Parameters.Add(new SqlParameter("@Decp", security.Decp));
                    if (string.IsNullOrEmpty(security.EmitentChild))
                        command.Parameters.Add(new SqlParameter("@EmitentChild", security.EmitentChild));
                    if (string.IsNullOrEmpty(security.Url))
                        command.Parameters.Add(new SqlParameter("@Url", security.Url));

                    command.Parameters.Add(new SqlParameter("@Source_id", security.SourceSecurityId));

                    await command.ExecuteNonQueryAsync();

                    return (int?)idParam.Value;
                }
                else
                {
                    command.Parameters.Add(new SqlParameter("@Id", id));
                    command.Parameters.Add(new SqlParameter("@IdInt", security.IdInt));
                    command.Parameters.Add(new SqlParameter("@IdString", security.IdString));
                    command.Parameters.Add(new SqlParameter("@Name", security.Name));
                    command.Parameters.Add(new SqlParameter("@Code", security.Code));
                    command.Parameters.Add(new SqlParameter("@Market", security.Market));
                    command.Parameters.Add(new SqlParameter("@MarketId", security.MarketId));
                    if (string.IsNullOrEmpty(security.Decp))
                        command.Parameters.Add(new SqlParameter("@Decp", security.Decp));
                    if (string.IsNullOrEmpty(security.EmitentChild))
                        command.Parameters.Add(new SqlParameter("@EmitentChild", security.EmitentChild));
                    if (string.IsNullOrEmpty(security.Url))
                        command.Parameters.Add(new SqlParameter("@Url", security.Url));

                    command.Parameters.Add(new SqlParameter("@Source_id", security.SourceSecurityId));

                    await command.ExecuteNonQueryAsync();

                    return id;
                }
            }
        }

        /// <summary>
        /// Метод делает выборку из БД по одному из параметров или всех, если параметров нет
        /// Параметр выбирается по приоритету, если есть name то выборка по name, если нет name то по market и т.д.
        /// </summary>
        /// <param name="name">часть имени инструмента</param>
        /// <param name="market">часть имени market</param>
        /// <param name="marketId">полное совпадение по marketId</param>
        /// <param name="decp">полное совпадение по decp</param>
        /// <param name="sourceId">полное совпадение по источнику вввода</param>
        /// <returns></returns>
        public async Task<List<BrokerRepositorySecurity>> GetListBrokerRepositorySecurity(string name = null, string market = null, string marketId = null, string decp = null, int? sourceId = null)
        {
            List<BrokerRepositorySecurity> result = new List<BrokerRepositorySecurity>();


            //если id не передан вызываем процедуру выборки всех сущьностей
            string sqlExpression = $@"{_nameDataSpace}.sp_broker_repository_security_get_list";

            string gg = GetConnectionString() + $"Initial Catalog={_nameDatabase};"; ;

            await using (SqlConnection connection = new SqlConnection(gg))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                // указываем, что команда представляет хранимую процедуру
                command.CommandType = System.Data.CommandType.StoredProcedure;

                //в процедуру может попасть только один параметр по иерархии или если все нулл, то вернется весь список
                if (name != null)
                    command.Parameters.Add(new SqlParameter("@Name", name));
                else if (market != null)
                    command.Parameters.Add(new SqlParameter("@Market", market));
                else if (marketId != null)
                    command.Parameters.Add(new SqlParameter("@MarketId", marketId));
                else if (decp != null)
                    command.Parameters.Add(new SqlParameter("@Decp", decp));
                else if (sourceId != null)
                    command.Parameters.Add(new SqlParameter("@Source_Id", sourceId));

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                BrokerRepositorySecurity ss = new BrokerRepositorySecurity()
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    IdString = Convert.ToString(reader["IdString"]),
                                    Name = Convert.ToString(reader["Name"]),
                                    Code = Convert.ToString(reader["Code"]),
                                    Market = Convert.ToString(reader["Market"]),
                                    MarketId = Convert.ToString(reader["MarketId"]),
                                    Decp = Convert.ToString(reader["Decp"]),
                                    EmitentChild = Convert.ToString(reader["EmitentChild"]),
                                    Url = Convert.ToString(reader["Url"]),
                                    SourceSecurityId = Convert.ToInt32(reader["Source_Id"])
                                };

                                result.Add(ss);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("GetListBrokerRepositorySecurity: " + e.ToString());
                            }
                        }
                    }
                }
            }

            var list = GetAllSourceSecurity().Result;

            //присваиваю источники для инструментов
            result.ForEach(s => s.SourceSecurity = list.FirstOrDefault(ss => ss.Id == s.SourceSecurityId));

            return result;
        }

        public async Task<BrokerRepositorySecurity> GetSecurityById(int id)
        {
            BrokerRepositorySecurity result = null;

            string sqlExpression = $@"{_nameDataSpace}.sp_broker_repository_security_get_by_id";

            string gg = GetConnectionString() + $"Initial Catalog={_nameDatabase};"; ;

            await using (SqlConnection connection = new SqlConnection(gg))
            {
                await connection.OpenAsync();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                // указываем, что команда представляет хранимую процедуру
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@Id", id));

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                result = new BrokerRepositorySecurity()
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    IdString = Convert.ToString(reader["IdString"]),
                                    Name = Convert.ToString(reader["Name"]),
                                    Code = Convert.ToString(reader["Code"]),
                                    Market = Convert.ToString(reader["Market"]),
                                    MarketId = Convert.ToString(reader["MarketId"]),
                                    Decp = Convert.ToString(reader["Decp"]),
                                    EmitentChild = Convert.ToString(reader["EmitentChild"]),
                                    Url = Convert.ToString(reader["Url"]),
                                    SourceSecurityId = Convert.ToInt32(reader["Source_Id"])
                                };

                                result.SourceSecurity = await this.GetSourceSecurityById(result.SourceSecurityId);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("GetAllSourceSecurity: " + e.ToString());
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Вызов хранимых процедур для обезличенных сделок

        

        #endregion
    }
}
