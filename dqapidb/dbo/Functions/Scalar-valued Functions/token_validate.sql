create function dbo.token_validate (
    @token varchar(1024)
) returns bit as
begin
    return
    iif	(
    (
            select top 1 1
                from dbo.user_sessions (nolock)
            where token = replace(@token, 'Bearer ', '')
            and expired_dtm > getutcdate()
    ) is null, 0, 1)
end