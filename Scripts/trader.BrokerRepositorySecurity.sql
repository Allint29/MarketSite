--Хранимая процедура берет из БД все инструменты

create or alter procedure trader.sp_broker_repository_security_get_list
	@Name nvarchar(200) null = null
	, @Market nvarchar(500) null =  null
	, @MarketId nvarchar(100) null = null
	, @Decp nvarchar(300) null = null
	, @Source_Id int null = null
as 
begin
	if @Name is not null
		select * from trader.BrokerRepositorySecurity where [Name] like '%'+ @Name + '%'
	else if @Market is not null
		select * from trader.BrokerRepositorySecurity where Market like '%'+ @Market + '%'
	else if @MarketId is not null
		select * from trader.BrokerRepositorySecurity where MarketId like MarketId
	else if @Decp is not null
		select * from trader.BrokerRepositorySecurity where Decp like @Decp
	else if @Source_Id is not null
		select * from trader.BrokerRepositorySecurity where Source_Id = @Source_Id
	else if @Name is null and @Market is null and @MarketId is null and @Decp is null and @Source_Id is null
		select * from trader.BrokerRepositorySecurity
end

go

--Хранимая процедура берет из БД все инструменты

create or alter procedure trader.sp_broker_repository_security_get_list
	@Name nvarchar(200) = null
	, @Market nvarchar(500) =  null
	, @MarketId nvarchar(100) = null
	, @Decp nvarchar(300) = null
	, @Source_Id int = null
as 
begin
	if @Name is not null
		select * from trader.BrokerRepositorySecurity where [Name] like '%'+ @Name + '%'
	else if @Market is not null
		select * from trader.BrokerRepositorySecurity where Market like '%'+ @Market + '%'
	else if @MarketId is not null
		select * from trader.BrokerRepositorySecurity where MarketId like @MarketId
	else if @Decp is not null
		select * from trader.BrokerRepositorySecurity where Decp like @Decp
	else if @Source_Id is not null
		select * from trader.BrokerRepositorySecurity where Source_Id = @Source_Id
	else if @Name is null and @Market is null and @MarketId is null and @Decp is null and @Source_Id is null
		select * from trader.BrokerRepositorySecurity
end

go

/*
Процедура обновления данных в таблице инструментов
*/
create or alter procedure trader.sp_broker_repository_security_update
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
		--or @Decp is null
		--or @EmitentChild is null
		--or @Url is null
		or @Source_Id is null
		begin
			raiserror('Some variable is null in trader.update_broker_repository_security', 16, 1)
			return @@error;
		end		


	if @Id is not null begin	
		update trader.BrokerRepositorySecurity
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
				trader.BrokerRepositorySecurity brs
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

		insert into trader.BrokerRepositorySecurity(
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