create procedure c.signIn (
    @params nvarchar(max)
) as
begin
    set @params = '[' + @params + ']'

    declare @trace_uuid varchar(128),
            @response_code smallint = 0,
            @response_message varchar(1024),
            --
            @login varchar(256),
            @user_uuid uniqueidentifier

    begin try
        select	@trace_uuid = isnull(json_value(value,'$.traceUuid'), newid()),
                @login = json_value(value,'$.login')
            from openjson(@params) p

        set @user_uuid =
        (
            select uuid
                from dbo.users (nolock)
            where email = @login
        )

        if @user_uuid is not null
        begin
            select	@trace_uuid as traceUuid,
                    200 as responseCode,
                    @user_uuid as "data.userUuid",
                    password_hash as "data.passwordHash",
                    password_salt as "data.passwordSalt"
                from dbo.users (nolock)
            where email = @login
            for json path, without_array_wrapper
        end
        else
        begin
            select	@trace_uuid as traceUuid,
                    404 as responseCode,
                    'Not Found' as responseMessage
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

        select	@trace_uuid as traceUuid,
                @response_code as responseCode,
                'Unexpected error (trace ID: ' + lower(convert(varchar, @trace_uuid)) + '). Please contact support team.' as responseMessage
        for json path, without_array_wrapper
    end catch
end
go
grant execute
    on object::c.signIn to api_user
    as dbo;
go
