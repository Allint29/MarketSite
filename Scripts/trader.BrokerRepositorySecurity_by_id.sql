use Trader_DataBase;

if object_id('trader.sp_broker_repository_security_get_by_id', 'P') is null
exec('create procedure trader.sp_broker_repository_security_get_by_id as return null;');                
go 

alter procedure trader.sp_broker_repository_security_get_by_id
@id int = null 
as 
begin 
select top 1 * from trader.BrokerRepositorySecurity brs where brs.Id = @id 
end

go

/*
ѕроцедура провер€ет наличие трейда на данну дату и возвращает bit 1 - если есть, 0 - если нет
*/
if object_id('trader.sp_impersonal_trades_check_by_date', 'P') is null
exec('create procedure trader.sp_impersonal_trades_check_by_date as return null;');                
go 

alter procedure trader.sp_impersonal_trades_check_by_date
@result bit out,
@date smalldatetime = null 
as 
begin 
	if exists (select top 1 * from trader.ImpersonalTrades it where it.Date_Time = @date)
		set @result = 1
	else 
		set @result = 0
end
