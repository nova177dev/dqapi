create table dbo.user_sessions
(
    uuid uniqueidentifier primary key
        constraint df_user_sessions_uuid default newid(),
    created_dtm datetime2 null
        constraint user_sessions_created_dtm default getutcdate(),
    [user_id] int not null
        index ix_user_sessions
        constraint df_user_sessions_user_id foreign key references dbo.users (id),
    expired_dtm datetime not null
        index ix_user_sessions_expired_dtm
        constraint df_user_sessions_expired_dtm default dateadd(hh, 72, getutcdate()),
    token varchar(1024) not null
)
