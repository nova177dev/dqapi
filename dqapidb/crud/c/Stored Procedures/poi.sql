create procedure c.poi (
    @params nvarchar(max)
) as
begin
    set nocount on

    set @params = iif(left(@params, 1) != '[', '[' + @params + ']', @params)

    declare @auth_token varchar(1024),
            @trace_uuid varchar(128),
            @response_code smallint = -1,
            @response_message varchar(1024),
            @error_message varchar(128),
            --
            @id bigint,
            @uuid varchar(128),
            --
            @code varchar(128),
            @name nvarchar(4000),
            @address nvarchar(4000),
            @description nvarchar(4000),
            @latitude decimal(12, 9),
            @longitude decimal(12, 9)

    begin try
        select  @auth_token = json_value(value,'$.authToken'),
                @trace_uuid = isnull(json_value(value,'$.traceUuid'), 0),
                --
                @code = json_value(value,'$.data.code'),
                @name = json_value(value,'$.data.name'),
                @address = json_value(value,'$.data.address'),
                @description = json_value(value,'$.data.description'),
                @latitude = json_value(value,'$.data.latitude'),
                @longitude = json_value(value,'$.data.longitude')
        from openjson(@params) p

        if dbo.token_validate(@auth_token) = 1
        begin
            insert into dbo.poi
            (
                code,
                name,
                address,
                description,
                latitude,
                longitude
            )
            values
            (
                @code,
                @name,
                @address,
                @description,
                @latitude,
                @longitude
            )

            set @id = scope_identity()
            set @uuid =
            (
                select uuid
                    from dbo.poi (nolock)
                where id = @id
            )

            if @id is not null
            begin
                select  @trace_uuid as traceUuid,
                        @uuid as uuid,
                        201 as responseCode,
                        'Created' as responseMessage,
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
                            where id = @id
                            for json path, without_array_wrapper
                        ), '$') as [data]
                for json path, without_array_wrapper
            end
            else
            begin
                select  @trace_uuid as requestId,
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
                @response_code as responseCode,
                'Unexpected error (trace ID: ' + lower(convert(varchar, @trace_uuid)) + '). Please contact support team.' as responseMessage
        for json path, without_array_wrapper
    end catch
end
go
grant execute
    on object::c.poi to api_user
    as dbo;
go
