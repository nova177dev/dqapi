create table dbo.system_log
(
    id bigint identity(1,1) not null primary key,
    created_dtm datetime2
        index ix_system_log_created_dtm
        constraint df_system_log_created_dtn default (getutcdate()),
    --
    [user_id] int null
        index ix_system_log_user_id,
    method varchar(128) null,
    params nvarchar(max) null,
    response nvarchar(max) null
)
