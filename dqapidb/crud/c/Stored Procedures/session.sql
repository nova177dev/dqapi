create procedure c.[session] (
    @params nvarchar(max)
) as
begin
    set @params = '[' + @params + ']'

    declare @trace_uuid varchar(128),
            @response_code smallint = 0,
            @response_message varchar(1024),
            --
            @user_uuid varchar(36),
            @token varchar(1024),
            @expired datetime2 = dateadd(hh, 72, getutcdate()),
            --
            @user_id int

    begin try
        select	@trace_uuid = isnull(json_value(value,'$.traceUuid'), newid()),
                @user_uuid = json_value(value,'$.userUuid'),
                @token = replace(coalesce(json_value(value,'$.token'), json_value(value,'$.authToken')), 'Bearer ', '')
        from openjson(@params) p

        select	@user_id = usr.id
            from dbo.users usr (nolock)
        where uuid = @user_uuid
            and status_id = 1 -- active

        if @user_id is not null
        begin
            update dbo.users
                set last_login = getutcdate()
            where id = @user_id

            merge dbo.user_sessions as target
            using (
                select	@user_id as [user_id],
                        @token as token,
                        @expired as expired_dtm
            ) as source
            on
            (
                (target.[user_id] = source.[user_id])
            )
            -- matched
            when	matched
                    and exists
                    (
                        select	target.token,
                                target.expired_dtm
                        except
                        select	source.token,
                                source.expired_dtm
                    )
                then update set
                    target.token = source.token,
                    target.expired_dtm = source.expired_dtm
            -- not matched
            when not matched by target
            then
                insert
                (
                    [user_id],
                    expired_dtm,
                    token
                )
                values
                (
                    [user_id],
                    expired_dtm,
                    token
                )
            -- not mached by source
            when not matched by source
                and
                (
                    (target.[user_id] = @user_id)
                )
                then delete;

            -- in case authorization was successful, reset the login attempts counter.
            update dbo.users
                set login_attempts = 0
            where id = @user_id

            select	@trace_uuid as traceUuid,
                    201 as responseCode,
                    'Created' as responseMessage,
                    @user_uuid as "data.userUuid",
                    @token as "data.authToken"
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
    on object::c.[session] to api_user
    as dbo;
go
