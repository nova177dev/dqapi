create procedure [c].[signOut] (
    @params varchar(max)
) as
begin
    set nocount on

    set @params = '[' + @params + ']'

    declare @operation varchar(128),
            @response_code smallint = 0,
            @response_message varchar(1024) = 'ok',
            --
            @trace_uuid varchar(128),
            @user_uuid varchar(128),
            @token varchar(1024),
            @expired datetime2,
            @user_id int

    begin try
        select	@trace_uuid = isnull(json_value(value,'$.traceUuid'), 0),
                @user_uuid = json_value(value,'$.userUuid'),
                @token = replace(coalesce(json_value(value,'$.token'), json_value(value,'$.authToken')), 'Bearer ', '')
        from openjson(@params) p

        select	@user_id = usr.id
            from dbo.users usr (nolock)
        where uuid = @user_uuid
            and status_id = 1 -- active

        if @user_id is not null
        begin
            delete from dbo.user_sessions
            where [user_id] = @user_id
                and token = @token

            select	@trace_uuid as traceUuid,
                    200 as responseCode,
                    'Ok' as responseMessage
            for json path, without_array_wrapper
        end
    end try
    begin catch
        insert into dbo.error_log
        (
            uuid,
            causer,
            params,
            error
        )
        values
        (
            @trace_uuid,
            schema_name() + '.' + object_name(@@procid),
            @params,
            isnull(error_message(), @response_message)
        )

        select	@response_code as responseCode,
                'Unexpected error (trace ID: ' + lower(convert(varchar, @trace_uuid)) + '). Please contact support team.' as responseMessage
        for json path, without_array_wrapper
    end catch
end
go
grant execute
    on object::c.signOut to api_user
    as dbo;
go
