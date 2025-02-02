create role api_user
go
alter role db_datareader add member api_user;
go
alter role db_datawriter add member api_user;
go
alter role [api_user] add member dqapi_app;
go
alter role [db_datareader] add member [dqapi_app];
go
alter role [db_datawriter] add member [dqapi_app];
go

