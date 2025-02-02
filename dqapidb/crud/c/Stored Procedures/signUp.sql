create procedure [c].[signUp] (
    @params nvarchar(max)
) as
begin
    set nocount on

    set @params = '[' + @params + ']'

    -- secure user identity for logging
    declare @secured_params nvarchar(max)

    begin try
        set @secured_params = json_modify(@params, '$[0].password', lower(convert(varchar(256), hashbytes('SHA2_512', json_value(@params,'$[0].password')), 2)))
    end try
    begin catch
        set @secured_params = @params
    end catch
    --

    declare @trace_uuid varchar(128),
            @response_code smallint = 0,
            @response_message varchar(1024),
            --
            @id int,
            @uuid uniqueidentifier,
            @role_id int = 3, -- user by default
            --
            @invitation_code varchar(128),
            @email varchar(255),
            @title varchar(128),
            @full_name nvarchar(256),
            @user_object nvarchar(max),
            --
            @hash varchar(128),
            @salt varchar(128)
    begin try
        select	@trace_uuid = isnull(json_value(value,'$.traceUuid'), newid()),
                @invitation_code = json_value(value,'$.data.invitationToken'),
                @hash = json_value(value,'$.data.passwordHash'),
                @salt = json_value(value,'$.data.passwordSalt'),
                @email = json_value(value,'$.data.email'),
                @title = json_value(value,'$.data.title'),
                @full_name = json_value(value,'$.data.fullName')
        from openjson(@params) p

        if exists (
            select 1
                from dbo.invitations (nolock)
            where code = @invitation_code
                and used_dtm is null
        )
        begin
            insert into dbo.users
            (
                role_id,
                email,
                password_hash,
                password_salt,
                title,
                full_name,
                status_id
            )
            values
            (
                @role_id, -- user
                @email,
                @hash,
                @salt,
                @title,
                @full_name,
                1  -- Active by default
            )

            select	@id = id,
                    @uuid = uuid
                from dbo.users (nolock)
            where id = scope_identity()

            select  @trace_uuid as traceUuid,
                    @uuid as uuid,
                    201 as responseCode,
                    'Created' as responseMessage,
                    json_query(
                    (
                        select	usr.uuid,
                                usr.email,
                                usr.title,
                                usr.full_name as fullName
                            from dbo.users usr (nolock)
                            join dbo.user_roles rol (nolock) on rol.id = usr.role_id
                            join dbo.user_statuses sts (nolock) on sts.id = usr.status_id
                        where usr.id = @id
                        for json path, without_array_wrapper
                    ), '$') as [data]
            for json path, without_array_wrapper
        end
        else
        begin
            select  @trace_uuid as traceUuid,
                    400 as responseCode,
                    'Invalid Invitaion Code' as responseMessage
            for json path, without_array_wrapper
        end
    end try
    begin catch
        set @response_code = 400

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
            @secured_params,
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
    on object::c.signUp to api_user
    as dbo;
go

