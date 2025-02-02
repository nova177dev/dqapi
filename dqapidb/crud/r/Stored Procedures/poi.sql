create procedure r.poi (
    @params nvarchar(max)
) as
begin
    set nocount on

    set @params = iif(left(@params, 1) != '[', '[' + @params + ']', @params)

    declare @auth_token varchar(1024),
            @trace_uuid varchar(36),
            @response_code smallint = -1,
            @response_message varchar(1024),
            @error_message varchar(128),
            --
            @uuid varchar(36)

    begin try
        select  @auth_token = replace(json_value(value,'$.authToken'), 'bearer ', ''),
                @trace_uuid = isnull(json_value(value,'$.traceUuid'), 0),
                --
                @uuid = json_value(value,'$.uuid')
        from openjson(@params) p

        if dbo.token_validate(@auth_token) = 1
        begin
            if exists (
                select 1
                    from dbo.poi (nolock)
                where uuid = @uuid
            )
            begin
                select  @trace_uuid as traceUuid,
                        200 as responseCode,
                        'Ok' as responseMessage,
                        @uuid as uuid,
                        json_query(
                        (
                            select	uuid,
                                    code,
                                    [name],
                                    [address],
                                    [description],
                                    latitude,
                                    longitude
                            from dbo.poi (nolock)
                            where uuid = @uuid
                            for json path, without_array_wrapper
                        ), '$') [data]
                for json path, without_array_wrapper
            end
            else
            begin
                select  @auth_token as authToken,
                        @trace_uuid as traceUuid,
                        404 as responseCode,
                        'Not Found' as responseMessage
                for json path, without_array_wrapper
            end
        end
        else
        begin
            set @response_code = 401
            set @response_message = 'Invalid authorization token.'
            raiserror(@response_message, 16, 1)
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
                400 as responseCode,
                'Unexpected error (trace UUID: ' + lower(convert(varchar, @trace_uuid)) + '). Please contact support team.' as responseMessage
        for json path, without_array_wrapper
    end catch
end
go
grant execute
    on object::r.poi to api_user
    as dbo;
go
